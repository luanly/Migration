using Microsoft.Azure.KeyVault.Models;
using Microsoft.Rest;

namespace SwissAcademic.KeyVaultUtils
{
    public static class KeyVaultSecrets
    {
        public static class ServiceAccounts
        {
            public const string CitaviWeb_CrmOnline = "ServiceAccount-CitaviWeb-CrmOnline";
        }

        public static class StorageAccounts
        {
            public const string CitaviWeb = "StorageAccount-CitaviWeb-AccessKey";
            public const string CitaviFunctions = "StorageAccount-CitaviFunctions-AccessKey";
            public const string CitaviLongRunningFunctions = "StorageAccount-CitaviLongRunningFunctions-AccessKey";
            public const string CrmFunctions = "StorageAccount-CrmFunctions-AccessKey";
            public const string BackOffice = "StorageAccount-BackOffice-AccessKey";
            public const string BackOffice6 = "StorageAccount-BackOffice6-AccessKey";
            public const string ProjectData = "StorageAccount-ProjectData-AccessKey";
            public const string CitaviStatistics = "StorageAccount-CitaviStatistics-AccessKey";
            public const string CloudSearch = "StorageAccount-CloudSearch-AccessKey";
        }

        public static class Databases
        {
            public const string ProjectData_Citavi6User = "Database-ProjectData-Citavi6User-ConnectionString";
            public const string ProjectData_CitaviStatistics = "Database-ProjectData-CitaviStatistics-ConnectionString";
            public const string ProjectData_Admin = "Database-ProjectData-Admin-ConnectionString";
            public const string CitaviBackOffice5DB_BackOffice6Reader = "Database-CitaviBackOffice5DB-BackOffice6Reader-ConnectionString";
            public const string CitaviBackOffice5DB_BackOffice6Writer = "Database-CitaviBackOffice5DB-BackOffice6Writer-ConnectionString";
            public const string UnitTests_UnitTests = "Database-UnitTests-UnitTests-ConnectionString";
            public const string C6UserTests_C6UserTests = "Database-C6UserTests-C6UserTests-ConnectionString";
        }

        public static class ApiKeys
        {
            public const string InternalQueueSupportTicket = "ApiKey-InternalQueueSupportTicket";
            public const string Ably = "ApiKey-Ably";
            public const string InternalCloudSearch = "ApiKey-InternalCloudSearch";
            public const string SendGrid = "ApiKey-SendGrid";
            public const string SendGridBounce = "ApiKey-SendGridBounce";
            public const string TipiMail = "ApiKey-TipiMail";
            public const string MailChimp = "ApiKey-MailChimp";
            public const string Mango = "ApiKey-Mango";
            public const string GitHub = "ApiKey-GitHub";
            public const string Uluru = "ApiKey-Uluru";
        }

        public static class Credentials
        {
            public const string CleverbridgeApi = "Credential-CleverbridgeApi";
            public const string CleverbridgeOrder = "Credential-CleverbridgeOrder";
        }
        public static class Redis
        {
            public const string RedisCitaviWeb = "Redis-CitaviWeb-AccessKey";
        }

        public static class Secrets
        {
            public const string ZenDesk = "Secret-ZenDesk";
            public const string Facebook = "Secret-Facebook";
            public const string Google = "Secret-Google";
            public const string Microsoft = "Secret-Microsoft-{0}";
            public const string Microsoft_TiP = "Secret-Microsoft-TiP-{0}";
            public const string Mango_Pass_ME = "Secret-Mango-Pass-ME";
            public const string GitHub_AccessToken = "Secret-GitHub-AccessToken";
        }

        public static class Urls
        {
            public const string AblyPresenceQueue = "Url-AblyPresenceQueue";
        }

        public static class AzureB2CKeys
        {
            public const string GraphApiClientId = "AzureB2C-GraphApi-ClientId";
            public const string GraphApiClientSecret = "AzureB2C-GraphApi-ClientSecret";
            public const string TenantId = "AzureB2C-TenantId";
        }
    }
}
