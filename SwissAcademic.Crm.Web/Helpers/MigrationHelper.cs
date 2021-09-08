using Aspose.Words.Drawing;
using SwissAcademic.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public static class MigrationHelper
	{
		public static string InitialCatalog = "ProjectData1";

		public static async Task<int> UndoAsync(string projectKeyPrefix, IEnumerable<string> skipProjects)
		{
			var updated = 0;
			var updated_all = 0;
			do
			{
				updated = await UndoAsync(projectKeyPrefix, skipProjects, 1000);
				updated_all += updated;
			}
			while (updated > 0);

			return updated_all;
		}
		public static async Task<int> UndoAsync(string projectKeyPrefix, IEnumerable<string> skipProjects, int pageSize)
		{
			if(string.IsNullOrEmpty(projectKeyPrefix))
			{
				throw new NotSupportedException($"{nameof(projectKeyPrefix)} mus not be null");
			}
			var updated = 0;

			var query = QueryExpression.Create<ProjectEntry>();
			query.PageSize = pageSize;
			query.AddCondition(ProjectEntryPropertyId.Key, ConditionOperator.BeginsWith, projectKeyPrefix);
			query.AddCondition(ProjectEntryPropertyId.InitialCatalog, ConditionOperator.Equal, InitialCatalog);

			using (var context = new CrmDbContext())
			{
				var projectEntries = new List<ProjectEntry>();
				projectEntries.AddRange(await context.RetrieveMultiple<ProjectEntry>(query, true));
				foreach (var projectEntity in projectEntries)
				{
					if(skipProjects.Contains(projectEntity.Key))
					{
						Console.WriteLine($"Skip project: {projectEntity.Key}");
						continue;
					}
					projectEntity.InitialCatalog = projectEntity.InitialCatalogBeforeMigration;
					projectEntity.DataSource = "citaviweb.database.cloudapi.de";
					if (context.PendingChangesCount > 50)
					{
						await context.SaveAsync();
					}
					await CrmCache.Projects.RemoveAsync(projectEntity);
					updated++;
				}

				await context.SaveAsync();

				Console.WriteLine($"{updated} Projects updated.");
			}

			return updated;
		}

		public static async Task<int> ClearProjectCacheFailedMigration()
		{
			var count = await ClearProjectCacheFailedMigration("ProjectData");
			count += await ClearProjectCacheFailedMigration("ProjectData2");
			count += await ClearProjectCacheFailedMigration("ProjectData3");
			count += await ClearProjectCacheFailedMigration("ProjectData4");
			count += await ClearProjectCacheFailedMigration("ProjectData5");
			count += await ClearProjectCacheFailedMigration("ProjectData6");
			count += await ClearProjectCacheFailedMigration("ProjectData7");
			return count;
		}

		static async Task<int> ClearProjectCacheFailedMigration(string initialCatalog)
		{
			var updated = 0;

			var query = QueryExpression.Create<ProjectEntry>();
			query.PageSize = 1000;
			query.AddCondition(ProjectEntryPropertyId.InitialCatalogBeforeMigration, ConditionOperator.Equal, initialCatalog);
			query.AddCondition(ProjectEntryPropertyId.InitialCatalog, ConditionOperator.Equal, initialCatalog);

			using (var context = new CrmDbContext())
			{
				do
				{
					var projectEntries = await context.RetrieveMultiple<ProjectEntry>(query, true);
					foreach (var projectEntity in projectEntries)
					{
						await CrmCache.Projects.RemoveAsync(projectEntity);
						updated++;
					}
				}
				while (query.HasMoreResults);
			}

			return updated;
		}

		public static async Task<(DateTime? lastModifiedOn, bool hasCacheDiff)> GetLastModifiedAsync(string projectKey, CrmDbContext context)
        {
			var projectEntry = await context.Get<ProjectEntry>(projectKey);
			var cacheEntry = await CrmCache.Projects.GetAsync(projectKey);

			return (null, false);//(projectEntry?.ProjectLastModifiedOn, cacheEntry?.DataCenter != projectEntry?.DataCenter);            
        }

		public static async Task<Dictionary<string, (DateTime? lastModifiedOn, bool hasCacheDiff)>> GetLastModifiedAsync(IEnumerable<string> projectKeys)
		{
			var dic = new Dictionary<string, (DateTime? lastModifiedOn, bool hasCacheDiff)>();

			using (var context = new CrmDbContext())
			{
				foreach (var key in projectKeys)
				{
					if (!dic.ContainsKey(key))
					{
						dic.Add(key, await GetLastModifiedAsync(key, context));
					}
				}
			}
			return dic;
		}
	}
}
