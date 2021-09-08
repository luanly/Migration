using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using SwissAcademic.Crm.Web;
using SwissAcademic.KeyVaultUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public class AzureRegionResolver
		:
		IAzureRegionResolver
	{
		public const string DefaultDataCenterShortName = "euw";

		Dictionary<string, DataCenter> CountryDataCenter = new Dictionary<string, DataCenter>();

		public IEnumerable<DataCenter> DataCenters { get; private set; }
		Dictionary<DataCenter, string> DataCenterShortNames = new Dictionary<DataCenter, string>();
		Dictionary<string, DataCenter> ShortNamesDataCenter = new Dictionary<string, DataCenter>();
		Dictionary<DataCenter, string> AzureRegionAuthorities = new Dictionary<DataCenter, string>();
		Dictionary<DataCenter, string> AzureRegionHosts = new Dictionary<DataCenter, string>();
		Dictionary<DataCenter, string> DataSourceForNewProjects = new Dictionary<DataCenter, string>();
		string CookieDomain;
		DataCenter CurrentDataCenter;
		string CloudSearchUri;
		string PdfServerUri;
		public bool MultiRegionSupportEnabled = true;

		static HttpClient Client;

		public void Initialize(string authority = null, DataCenter? currentDataCenter = null)
		{
			var dataCenterNames = Enum.GetNames(typeof(DataCenter)).ToList();
			if (Environment.Build == BuildType.Alpha)
			{
				dataCenterNames.Remove(nameof(DataCenter.CentralUS));
			}

			if (!string.Equals(ConfigurationManager.AppSettings["AzureMultiRegionSupport"], bool.TrueString, StringComparison.InvariantCultureIgnoreCase))
			{
				MultiRegionSupportEnabled = false;
			}

			foreach (var dataCenterName in dataCenterNames)
			{
				var datacenter_shortname = typeof(DataCenter).GetMember(dataCenterName)[0].GetCustomAttributes(false).OfType<DescriptionAttribute>().First().Description;
				var dataCenter = Enum.Parse<DataCenter>(dataCenterName);

				DataCenterShortNames[dataCenter] = datacenter_shortname;
				ShortNamesDataCenter[datacenter_shortname] = dataCenter;
			}

			if (currentDataCenter == null)
			{
				if (ConfigurationManager.AppSettings["DataCenter"] != null)
				{
					CurrentDataCenter = ShortNamesDataCenter[ConfigurationManager.AppSettings["DataCenter"].ToLowerInvariant()];
				}
				else
				{
					CurrentDataCenter = DataCenter.WestEurope;
				}
			}
			else
			{
				CurrentDataCenter = currentDataCenter.Value;
			}


			if(authority == null)
			{
				authority = ConfigurationManager.AppSettings["Authority"];
			}

			var uri = new Uri(authority);
			var authoritySegments = uri.Host.Split('.');
			if (authoritySegments.Length == 1)
			{
				CookieDomain = $"{authoritySegments[0]}";
			}
			else
			{
				if (authoritySegments.Length == 3)
				{
					CookieDomain = $".{authoritySegments.ToString(".")}";
				}
				else
				{
					CookieDomain = $".{authoritySegments.Skip(1).ToString(".")}";
				}
			}
			var dataSourceForNewProjectsSegments = ConfigurationManager.AppSettings["DataSourceForNewProjects"].Split('.');

			var dataCenters = new List<DataCenter>();
			foreach (var dataCenterName in dataCenterNames)
			{
				var datacenter_shortname = typeof(DataCenter).GetMember(dataCenterName)[0].GetCustomAttributes(false).OfType<DescriptionAttribute>().First().Description;
				var dataCenter = Enum.Parse<DataCenter>(dataCenterName);
				dataCenters.Add(dataCenter);
				if (authoritySegments.Length == 1)
				{
					AzureRegionAuthorities[dataCenter] = authority;
					AzureRegionHosts[dataCenter] = authority.Replace("https://", string.Empty);
				}
				else if (authoritySegments.Length == 3)
				{
					AzureRegionHosts[dataCenter] = $"{datacenter_shortname}.{authoritySegments.ToString(".")}";
					AzureRegionAuthorities[dataCenter] = $"https://{AzureRegionHosts[dataCenter]}";
				}
				else
				{
					AzureRegionHosts[dataCenter] = $"{authoritySegments[0]}-{datacenter_shortname}.{authoritySegments.Skip(1).ToString(".")}";
					AzureRegionAuthorities[dataCenter] = $"https://{AzureRegionHosts[dataCenter]}";
				}

				if (dataCenter == DataCenter.WestEurope)
				{
					DataSourceForNewProjects[dataCenter] = ConfigurationManager.AppSettings["DataSourceForNewProjects"];
				}
				else
				{
					if (Environment.Build == BuildType.Alpha)
					{
						DataSourceForNewProjects[dataCenter] = $"alphacitaviweb{datacenter_shortname}.{dataSourceForNewProjectsSegments.Skip(1).ToString(".")}";
					}
					else
					{
						DataSourceForNewProjects[dataCenter] = $"citaviweb{datacenter_shortname}.{dataSourceForNewProjectsSegments.Skip(1).ToString(".")}";
					}
				}
			}

			DataCenters = dataCenters;

			Client = new HttpClient();
			Client.DefaultRequestHeaders.Add("x-ms-client-id", ConfigurationManager.AppSettings["AzureMapsClientId"]);

			var azureRegionsWorld = Properties.Resources.AzureRegions.Split('\n');
			foreach(var azureRegion in azureRegionsWorld)
			{
				if (string.IsNullOrWhiteSpace(azureRegion))
				{
					continue;
				}
				var azureRegionInfo = azureRegion.Split(';');
				var isoCode = azureRegionInfo[0].Trim().ToLowerInvariant();
				var datacenterShortName = azureRegionInfo[1].Trim();
				if (Environment.Build == BuildType.Alpha && datacenterShortName == "usc") 
				{
					datacenterShortName = "euw";
				}
				var datacenter = ShortNamesDataCenter[datacenterShortName];
				CountryDataCenter[isoCode] = datacenter;
			}

			if (authoritySegments.Length == 1)
			{
				CloudSearchUri = $"https://localhost:9876";
				PdfServerUri = $"https://pdfviewer-{DataCenterShortNames[CurrentDataCenter]}.alphacitaviweb.citavi.com";
			}
			else if (authoritySegments.Length == 3)
			{
				CloudSearchUri = $"https://search-{DataCenterShortNames[CurrentDataCenter]}.{authoritySegments.ToString(".")}";
				PdfServerUri = $"https://pdfviewer-{DataCenterShortNames[CurrentDataCenter]}.{authoritySegments.ToString(".")}";
			}
			else
			{
				CloudSearchUri = $"https://search-{authoritySegments[0]}-{DataCenterShortNames[CurrentDataCenter]}.{authoritySegments.Skip(1).ToString(".")}";
				PdfServerUri = $"https://pdfviewer-{DataCenterShortNames[CurrentDataCenter]}.{authoritySegments.Skip(1).ToString(".")}";
			}
		}

		public string GetShortName(DataCenter dataCenter) => DataCenterShortNames[dataCenter];

		public string GetAuthority(DataCenter dataCenter) => AzureRegionAuthorities[dataCenter];

		public string GetCloudSearchUri() => CloudSearchUri;

		public string GetPdfTronServerUri() => PdfServerUri;

		public string GetCookieDomain() => CookieDomain;

		public DataCenter? GetDataCenterByCountryIsoCode(string isoCode)
		{
			if (string.IsNullOrEmpty(isoCode))
			{
				return null;
			}
			if (CountryDataCenter.TryGetValue(isoCode.ToLowerInvariant(), out var dataCenter))
			{
				return dataCenter;
			}
			return null;
		}

		public async Task<DataCenter> GetDataCenter(string ipAddress)
		{
			if (string.IsNullOrEmpty(ipAddress) || !MultiRegionSupportEnabled)
			{
				return DataCenter.WestEurope;
			}
			var accessToken = await AzureHelper.AzureServiceTokenProvider.GetAccessTokenAsync("https://atlas.microsoft.com");
			Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
			var response = await Client.GetAsync($"https://atlas.microsoft.com/geolocation/ip/json?api-version=1.0&ip={ipAddress}");
			if (response.IsSuccessStatusCode)
			{
				var json = JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
				var isoCode = json["countryRegion"]["isoCode"].ToString().ToLowerInvariant();
				if(CountryDataCenter.TryGetValue(isoCode, out var dataCenter))
				{
					return dataCenter;
				}
				Telemetry.TrackTrace($"DataCenter not found: {isoCode}", SeverityLevel.Error);
				return DataCenter.WestEurope;
			}
			else
			{
				Telemetry.TrackTrace($"GetDataCenter failed: {response.RequestMessage}", SeverityLevel.Error);
				return DataCenter.WestEurope;
			}
		}

		public DataCenter GetCurrentDataCenter() => CurrentDataCenter;
		public string GetHost(DataCenter dataCenter) => AzureRegionHosts[dataCenter];

		public string GetDataSourceForNewProjects(DataCenter dataCenter) => DataSourceForNewProjects[dataCenter];

		public async Task<Dictionary<DataCenter, string>> GetRegionalKeyVaultSecretAsync(string keyvaultSecretName)
		{
			var urls = new Dictionary<DataCenter, string>();
			foreach(var dataCenterShortNames in DataCenterShortNames)
			{
				urls[dataCenterShortNames.Key] = await AzureHelper.KeyVaultClient.GetSecretAsync($"{keyvaultSecretName}-{dataCenterShortNames.Value}");
			}
			return urls;
		}


		public async Task<string> GetRegionalKeyVaultSecretAsync(string keyvaultSecretName, string dataCenterShortName)
		{
			return await AzureHelper.KeyVaultClient.GetSecretAsync($"{keyvaultSecretName}-{dataCenterShortName}");
		}

		public async Task<string> GetKeyVaultSecretAsync(string keyvaultSecretName)
		{
			return await GetRegionalKeyVaultSecretAsync(keyvaultSecretName, GetCurrentDataCenter());
		}
		public async Task<string> GetRegionalKeyVaultSecretAsync(string keyvaultSecretName, DataCenter dataCenter)
		{
			return await GetRegionalKeyVaultSecretAsync(keyvaultSecretName,DataCenterShortNames[dataCenter]);
		}


		public static AzureRegionResolver Instance = new AzureRegionResolver();
	}
}
