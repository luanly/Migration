using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public class CrmInitializeService
		:
		IHostedService
	{
		IAzureStorageResolver AzureStorageResolver;
		CrmConfigSet Config;
		Func<string, bool, Task> CalculateCitaviSpace;

		public CrmInitializeService(CrmConfigSet crmConfigSet, IAzureStorageResolver azureRegionResolver, Func<string, bool, Task> calculateCitaviSpace)
		{
			Config = crmConfigSet;
			AzureStorageResolver = azureRegionResolver;
			CalculateCitaviSpace = calculateCitaviSpace;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await CrmConfig.Initialize(Config, AzureStorageResolver, CalculateCitaviSpace);
		}
		public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
	}
}
