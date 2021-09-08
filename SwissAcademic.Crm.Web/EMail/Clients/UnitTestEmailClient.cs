using SwissAcademic.Crm.Web.EMail.Clients;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public class UnitTestEmailClient
        :
        IEmailClient
    {

        [ExcludeFromCodeCoverage]
        public Task<bool> AddBounces(string email) => Task.FromResult(true);

        [ExcludeFromCodeCoverage]
        public Task<bool> DeleteBounces(string email) => Task.FromResult(true);
        [ExcludeFromCodeCoverage]
        public Task<bool> DeleteBlocks(string email) => Task.FromResult(true);

        public Task<IEnumerable<EmailBounce>> GetBounces(string email) => Task.FromResult(Enumerable.Empty<EmailBounce>());

        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<EmailBounce>> ListBounces(int limit, int offset) => Task.FromResult(Enumerable.Empty<EmailBounce>());
        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<EmailBounce>> GetBlocks(string email) => Task.FromResult(Enumerable.Empty<EmailBounce>());
        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<EmailBounce>> ListBounces(DateTimeOffset start, DateTimeOffset end) => Task.FromResult(Enumerable.Empty<EmailBounce>());
        [ExcludeFromCodeCoverage]
        public Task<IEnumerable<EmailBounce>> ListBlocks(DateTimeOffset start, DateTimeOffset end) => Task.FromResult(Enumerable.Empty<EmailBounce>());

        [ExcludeFromCodeCoverage]
        public Task InitalizeAsync() => Task.CompletedTask;

        [ExcludeFromCodeCoverage]
        public Task<bool> SendAsync(CrmEmail email, string mailAddress, IEnumerable<EmailAttachment> attachments, string fromAddress, string fromName, string tags, params string[] moreReceivers)
         => Task.FromResult(true);

        public Task<bool> SendAsync(string body, string toAddress, string fromAddress, string fromName, string subject, bool isHtml, EmailAttachment emailAttachment, string tags)
        => Task.FromResult(true);
        public Task<bool> SendBulkAsync(CrmEmail email, IEnumerable<Contact> contacts, string fromAddress, string fromName, string tags)
        => Task.FromResult(true);
		
    }
}
