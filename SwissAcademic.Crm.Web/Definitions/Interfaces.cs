using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.Queue;
using SwissAcademic.Azure;
using SwissAcademic.Crm.Web.EMail.Clients;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public interface IAzureRegionResolver
	{
        string GetAuthority(DataCenter dataCenter);
        string GetCookieDomain();

        DataCenter GetCurrentDataCenter();
        string GetHost(DataCenter dataCenter);
    }

    public interface IAzureStorageResolver
	{
        /// <summary>
        /// TableClient in Current Datacenter
        /// </summary>
        CloudTableClient GetCloudTable();

        /// <summary>
        /// TableClient in WestEurope
        /// </summary>
        CloudTableClient GetCloudTableWestEurope();

        /// <summary>
        /// BlobClient in Current Datacenter
        /// </summary>
        CloudBlobClient GetCloudBlobClient();

        /// <summary>
        /// QueueClient in Current Datacenter
        /// </summary>
        CloudQueueClient GetCloudQueueClient();

        CloudTable GetUserSettingsCloudTable(CrmUser user);
    }

    public interface ICrmRelationship
    {
        string ReferencingEntityLogicalName { get; }
        string ReferencedEntityLogicalName { get; }
        string RelationshipQueryName { get; }
        string RelationshipLogicalName { get; }
        CrmEntityRelationshipType RelationshipType { get; }
        CitaviCrmEntity Source { get; }
        string SourceEntityLogicalName { get; }
        Type SourceEntityType { get; }
        string TargetEntityLogicalName { get; }
        Type TargetEntityType { get; }
    }

    public interface IHasVerificationData
    {
        bool? IsVerified { get; set; }
        string VerificationKey { get; set; }
        DateTime? VerificationKeySent { get; set; }
        VerificationKeyPurpose? VerificationPurpose { get; set; }
        string VerificationStorage { get; set; }
    }

    public interface IEmailClient
    {
        Task<bool> DeleteBounces(string email);
        Task<bool> DeleteBlocks(string email);
        Task<IEnumerable<EmailBounce>> GetBounces(string email);
        Task<IEnumerable<EmailBounce>> GetBlocks(string email);
        Task InitalizeAsync();
        Task<IEnumerable<EmailBounce>> ListBounces(DateTimeOffset start, DateTimeOffset end);
        Task<IEnumerable<EmailBounce>> ListBlocks(DateTimeOffset start, DateTimeOffset end);
        Task<bool> SendAsync(CrmEmail email, string mailAddress, IEnumerable<EmailAttachment> attachments, string fromAddress, string fromName, string tag, params string[] moreReceivers);
        Task<bool> SendAsync(string body, string toAddress, string fromAddress, string fromName, string subject, bool isHtml, EmailAttachment emailAttachment, string tag);

        Task<bool> SendBulkAsync(CrmEmail email, IEnumerable<Contact> toAddresses, string fromAddress, string fromName, string tag);
    }

    public interface IHasCacheETag
    {
        string CacheETag { get; set; }
    }
}
