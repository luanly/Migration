using Microsoft.Extensions.Hosting;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class CrmCacheBackgroundService
        :
        BackgroundService
    {
        #region Felder

        static readonly TimeSpan Delay = TimeSpan.FromMinutes(1);

        #endregion

        #region Konstruktor

        public CrmCacheBackgroundService()
        {
        }

        #endregion

        #region ExecuteAsync

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var now = DateTime.UtcNow;
            var fullReset = now.AddDays(1).Date.AddHours(5);
            int counter = 0;
            var entityChangesCacheInitialized = false;

            using (var context = new CrmDbContext())
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        if (!CrmCache.IsInitialized)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(10));
                            continue;
                        }

                        if (!entityChangesCacheInitialized)
                        {
                            await CrmCache.InitializeEntityChangesCache(context);
                            entityChangesCacheInitialized = true;
                        }

                        await Task.Delay(Delay, stoppingToken);
                        if (stoppingToken.IsCancellationRequested)
                        {
                            return;
                        }

                        counter++;
                        if(counter % 60 == 0)
                        {
                            using (Telemetry.StartOperation("Refresh CleverbridgeProducts"))
                            {
                                await CrmCache.ResetCleverbridgeProductsCache();
                            }
                        }

                        if (DateTime.UtcNow.Day == fullReset.Day &&
                            DateTime.UtcNow.Hour == fullReset.Hour)
                        {
                            now = DateTime.UtcNow;
                            fullReset = now.AddDays(1).Date.AddHours(5);
                        }
                        
                        if (!CrmCache._refreshCampusContractsPending)
                        {
                            await CrmCache.RefreshCampusContractCache(context);
                            continue;
                        }

                        CrmCache._refreshCampusContractsPending = false;

                        using (Telemetry.StartOperation("Refresh CampusContracts"))
                        {
                            await CrmCache.ResetEmailDomainCache(false);
                            await CrmCache.ResetIPRangesCache(false);
                            await CrmCache.ResetCampusContractsCache(false);
                        }
                    }
                    catch (Exception ignored)
                    {
                        if (stoppingToken.IsCancellationRequested) return;
                        Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat);
                        await Task.Delay(Delay, stoppingToken);
                    }
                }
            }
        }

        #endregion
    }
}
