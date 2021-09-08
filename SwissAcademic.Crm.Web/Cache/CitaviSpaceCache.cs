using Aspose.Words.Drawing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class CitaviSpaceCache
    {
        #region Konstanten

        const string AvailableCitaviSpaceCacheName = "AvailableCitaviSpaceCache";
        const string UsedCitaviSpaceCacheName = "UsedCitaviSpaceCache";
        const string ExceededCitaviSpaceCacheName = "ExceededCitaviSpaceCache";
        const string ProjectStatisticCitaviSpaceCacheName = "ProjectStatisticCitaviSpaceCache";

        #endregion

        #region Eigenschaften

        public static bool IsAvailable { get; set; }
        public static TableStorageRepository AvailableCitaviSpaceCacheRepo { get; private set; }
        internal static TableStorageRepository ExceededCitaviSpaceCacheRepo { get; private set; }
        internal static TableStorageRepository UsedCitaviSpaceCacheRepo { get; private set; }
        internal static TableStorageRepository ProjectStatisticCitaviSpaceCacheRepo { get; private set; }


        #endregion

        #region Methoden

        #region AddOrUpdateAsync

        public static async Task AddOrUpdateAsync(CrmUser user, long usedCloudSpaceInBytes, long availableCloudSpaceInBytes, bool isExceeded, CrmDbContext context)
        {
            var availableCloudSpaceTask = AvailableCitaviSpaceCacheRepo.AddOrUpdateAsync(availableCloudSpaceInBytes, user.Key);

            var usedCloudSpaceTask = UsedCitaviSpaceCacheRepo.AddOrUpdateAsync(usedCloudSpaceInBytes, user.Key);

            await Task.WhenAll(availableCloudSpaceTask, usedCloudSpaceTask);

            if (isExceeded)
			{
                await ExceededCitaviSpaceCacheRepo.AddOrUpdateAsync(1, user.Key);
            }
			else
			{
                await ExceededCitaviSpaceCacheRepo.RemoveAsync(user.Key);
            }
        }

        #endregion

        #region AddProjectStatisticAsync

        public static async Task AddProjectStatisticAsync(CitaviSpaceProjectStatistic cloudSpaceProjectStatistic)
         => await ProjectStatisticCitaviSpaceCacheRepo.AddOrUpdateAsync(cloudSpaceProjectStatistic, cloudSpaceProjectStatistic.Key);

        #endregion

        #region GetAvailableCitaviSpaceAsync

        public static Task<long> GetAvailableCitaviSpaceAsync(string contactKey, bool ignoreCache) => GetAvailableCitaviSpaceAsync(contactKey, null, false, null, ignoreCache);
        public static Task<long> GetAvailableCitaviSpaceAsync(string contactKey, CrmDbContext context = null) => GetAvailableCitaviSpaceAsync(contactKey, context, false, null, false);

        internal static async Task<long> GetAvailableCitaviSpaceAsync(string contactKey, CrmDbContext context, bool fromCacheOnly = false, DataCenter? dataCenter = null, bool ignoreCache = false)
        {
            if (string.IsNullOrEmpty(contactKey))
            {
                return 0;
            }

            var disposeContext = false;
            try
            {
                var citavispace = await AvailableCitaviSpaceCacheRepo.GetAsync<long?>(contactKey, dataCenter);

                if ((citavispace == null && !fromCacheOnly) || ignoreCache)
                {
                    if (context == null)
                    {
                        disposeContext = true;
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                        context = new CrmDbContext();
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                    }
                    var fetchXml = new Query.FetchXml.GetUserCitaviSpace(contactKey);
                    citavispace = await context.Sum(FetchXmlExpression.Create<CitaviLicense>(fetchXml.TransformText()));
                    citavispace = citavispace * 1024 * 1024;
                    await AvailableCitaviSpaceCacheRepo.AddOrUpdateAsync(citavispace.Value, contactKey);
                }

                if(citavispace == null)
                {
                    return 0;
                }

                return citavispace.Value;
            }
            finally
            {
                if (disposeContext && context != null)
                {
                    context.Dispose();
                }
            }
        }

		#endregion

		#region GetProjectStatistic

        public static async Task<CitaviSpaceProjectStatistic> GetProjectStatisticAsync(ProjectEntry projectEntry)
		 => await ProjectStatisticCitaviSpaceCacheRepo.GetAsync<CitaviSpaceProjectStatistic>(projectEntry.Key);

        #endregion

        #region GetStatusAsync

        public static async Task<(long Available, long Used, bool IsExceeded)> GetStatusAsync(CrmUser user, CrmDbContext context = null)
		{
            var availableTask = GetAvailableCitaviSpaceAsync(user.Key);
            var usedTask = GetUsedCitaviSpaceAsync(user.Key);
            var exceededTask = HasExceededCloudSpaceAsync(user.Key);

            await Task.WhenAll(availableTask, usedTask, exceededTask);

            return (availableTask.Result, usedTask.Result, exceededTask.Result);
        }

        #endregion

        #region GetUsedCitaviSpaceAsync

        internal static async Task<long> GetUsedCitaviSpaceAsync(string contactKey, DataCenter? dataCenter = null)
         => (await UsedCitaviSpaceCacheRepo.GetAsync<long?>(contactKey, dataCenter)) ?? 0;

        #endregion

        #region HasCitaviSpaceCalculated

        internal static async Task<bool> HasCitaviSpaceCalculated(string contactKey, DataCenter? dataCenter = null)
		{
            var used = await UsedCitaviSpaceCacheRepo.GetAsync<long?>(contactKey, dataCenter);
            return used != null;
        }

        #endregion

        #region HasExceededCitaviSpaceAsync

        public static async Task<bool> HasExceededCitaviSpaceAsync(IHttpContextAccessor httpContextAccessor)
        {
            try
            {
                if (!IsAvailable)
                {
                    return false;
                }

                var projectEntry = httpContextAccessor.GetProject();
                if(projectEntry == null)
				{
                    projectEntry = await CrmCache.Projects.GetAsync(httpContextAccessor.GetProjectKey());

                }
                return await HasExceededCloudSpaceAsync(projectEntry.DataContractOwnerContactKey);
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
            }
            return false;
        }

        internal static async Task<bool> HasExceededCloudSpaceAsync(string contactKey, DataCenter? dataCenter = null)
         => IsAvailable && (await ExceededCitaviSpaceCacheRepo.GetAsync<int?>(contactKey, dataCenter)).HasValue;

        #endregion

        #region InitializeAsync

        internal static async Task InitializeAsync(CrmConfigSet configSet)
        {
            IsAvailable = CrmConfig.IsShopWebAppSubscriptionAvailable;

            AvailableCitaviSpaceCacheRepo = new TableStorageRepository(isComplexObject: false);
            await AvailableCitaviSpaceCacheRepo.InitializeAsync(AvailableCitaviSpaceCacheName, multiRegionSupport: true);

            UsedCitaviSpaceCacheRepo = new TableStorageRepository(isComplexObject: false);
            await UsedCitaviSpaceCacheRepo.InitializeAsync(UsedCitaviSpaceCacheName, multiRegionSupport: true);

            ExceededCitaviSpaceCacheRepo = new TableStorageRepository(isComplexObject: false);
            await ExceededCitaviSpaceCacheRepo.InitializeAsync(ExceededCitaviSpaceCacheName, multiRegionSupport: true);

            ProjectStatisticCitaviSpaceCacheRepo = new TableStorageRepository(isComplexObject: true);
            await ProjectStatisticCitaviSpaceCacheRepo.InitializeAsync(ProjectStatisticCitaviSpaceCacheName, multiRegionSupport: true);
        }

        #endregion

        #region QueueUpdateCitaviSpace

        public static async Task QueueUpdateCitaviSpace(IHttpContextAccessor httpContextAccessor)
        {
            try
            {
                if (!IsAvailable)
                {
                    return;
                }

                var projectEntry = httpContextAccessor.GetProject();
                await QueueUpdateCitaviSpace(projectEntry.DataContractOwnerContactKey);
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
            }
        }
        public static async Task QueueUpdateCitaviSpace(string contactKey)
        {
            try
            {
                if (!IsAvailable)
                {
                    return;
                }

                var props = new Dictionary<string, string>()
                {
                    [MessageKey.ContactKey] = contactKey
                };
                await AzureHelper.AddQueueMessageAsync(null, props, MessageKey.UpdateCitaviSpace);
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
            }
        }

        #endregion

        #region RefreshAsync

        public static async Task<(long Used, long Available, bool IsExceeded)> RefreshAsync(string contactKey, CrmDbContext context, DataCenter? dataCenter = null)
		{
            if (!IsAvailable)
            {
                return (0, 0, false);
            }
            var user = await context.GetByKeyAsync(contactKey);
            return await RefreshAsync(user, context, dataCenter);
		}
        public static async Task<(long Used, long Available, bool IsExceeded)> RefreshAsync(CrmUser user, CrmDbContext context, DataCenter? dataCenter = null, bool skipCloudSpaceWarningSentCheck = false)
        {
			if (!IsAvailable)
			{
                return (0, 0, false);
			}
            if(user == null)
			{
                return (0, 0, false);
            }
            if(context.PendingChangesCount > 0)
			{
                await context.SaveAsync();
            }

            await AvailableCitaviSpaceCacheRepo.RemoveAsync(user.Key);

            var availableSpaceInBytes = await GetAvailableCitaviSpaceAsync(user.Key, context, fromCacheOnly: false, dataCenter: dataCenter);
            var usedCloudSpaceInBytes = await GetUsedCitaviSpaceAsync(user.Key);

            var isExceeded = usedCloudSpaceInBytes > availableSpaceInBytes;

            await AddOrUpdateAsync(user, usedCloudSpaceInBytes, availableSpaceInBytes, isExceeded, context);

            if (!skipCloudSpaceWarningSentCheck)
            {
                if (user.Contact.CloudSpaceWarningSent != CloudSpaceWarningSentType.None)
                {
                    await context.SaveAndUpdateUserCacheAsync(user);
                }
            }

            return (usedCloudSpaceInBytes, availableSpaceInBytes, isExceeded);
        }

        #endregion

        #region RemoveAsync

        public static async Task RemoveAsync(string contactKey)
        {
            if (await HasCitaviSpaceCalculated(contactKey))
            {
                var t1 = AvailableCitaviSpaceCacheRepo.RemoveAsync(contactKey);
                var t2 = UsedCitaviSpaceCacheRepo.RemoveAsync(contactKey);
                var t3 = ExceededCitaviSpaceCacheRepo.RemoveAsync(contactKey);

                await Task.WhenAll(t1, t2, t3);
            }
        }

        #endregion

        #region RemoveAndRecalulateCitaviSpaceAsync

        /// <summary>
        /// Wir nur für Calls aus CRM Function benötigt
        /// </summary>
        /// <param name="contactKey"></param>
        /// <returns></returns>
        public static async Task RemoveAndRecalulateCitaviSpaceAsync(string contactKey)
		{
			if (!IsAvailable)
			{
                return;
			}
            await AvailableCitaviSpaceCacheRepo.RemoveAsync(contactKey);
            await UsedCitaviSpaceCacheRepo.RemoveAsync(contactKey);
            await ExceededCitaviSpaceCacheRepo.RemoveAsync(contactKey);
            await QueueUpdateCitaviSpace(contactKey);
        }

        #endregion

        #region RemoveProjectStatisticAsync

        public static async Task RemoveProjectStatisticAsync(ProjectEntry projectEntry)
         => await ProjectStatisticCitaviSpaceCacheRepo.RemoveAsync(projectEntry.Key);

        #endregion

        #endregion
    }
}