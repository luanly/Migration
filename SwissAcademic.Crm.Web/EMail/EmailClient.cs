using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public static class EmailClient
    {
        #region Felder

        internal static readonly string FromAddress = ConfigurationManager.AppSettings["AccountEmailFrom"];
        internal static readonly string FromName = ConfigurationManager.AppSettings["AccountEmailFromName"];
        static IEmailClient Client;

        #endregion

        #region Methoden

        public static async Task<bool> DeleteBounces(string email)
        {
            try
            {
                var hasBounces = await Client.DeleteBounces(email);
                var hasBlocks = await Client.DeleteBlocks(email);
                return hasBlocks || hasBounces;
            }
            catch (Exception ex)
            {
				if (CrmConfig.IsUnittest)
				{
                    throw;
				}
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return false;
        }

        public static async Task<bool> DeleteBlocks(string email)
        {
            try
            {
                return await Client.DeleteBlocks(email);
            }
            catch (Exception ex)
            {
                if (CrmConfig.IsUnittest)
                {
                    throw;
                }
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
            }
            return false;
        }

        public static async Task<IEnumerable<EmailBounce>> GetBounces(string email) => await Client.GetBounces(email);
        public static async Task<IEnumerable<EmailBounce>> GetBlocks(string email) => await Client.GetBlocks(email);

        public static async Task<IEnumerable<EmailBounce>> ListBlocks(DateTimeOffset start, DateTimeOffset end) => await Client.ListBlocks(start, end);
        public static async Task<IEnumerable<EmailBounce>> ListBounces(DateTimeOffset start, DateTimeOffset end) => await Client.ListBounces(start, end);

        [ExcludeFromCodeCoverage]
        internal static async Task InitializeAsync(CrmConfigSet config)
        {
            Client = config.EmailClient;
            if (Client == null)
            {
                Telemetry.TrackTrace("EmailClient not set", SeverityLevel.Warning);
                Client = new SendGridEmailClient();
            }
            Telemetry.TrackTrace($"EmailClient: {Client.GetType().Name}");
            EmailFormatter.Initialize();
            await Client.InitalizeAsync();
        }

        internal static bool IsValid(string toAddress)
        {
            if (CrmConfig.IsUnittest)
            {
                return false;
            }

            if (toAddress.EndsWith(".noemail@citavi.com", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (toAddress.EndsWith(".noemail@citavi2.com", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (toAddress.EndsWith("uluruuniversity.com", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        [ExcludeFromCodeCoverage]
        internal static async Task<bool> SendAsync(CrmEmail email, string toAddress, IEnumerable<EmailAttachment> attachments, string fromAddress = null, string fromName = null, string tag = null, params string[] moreReceivers)
        {
            if (!IsValid(toAddress))
            {
                return true;
            }

            if (string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(fromName))
            {
                fromAddress = FromAddress;
                fromName = FromName;
            }
            return await Client.SendAsync(email, toAddress, attachments, fromAddress, fromName, tag, moreReceivers);
        }

        [ExcludeFromCodeCoverage]
        internal static async Task<bool> SendAsync(string body, string toAddress, string subject, bool isHtml, EmailAttachment attachment, string tag)
         => await SendAsync(body, toAddress, FromAddress, FromName, subject, isHtml, attachment, tag);

        [ExcludeFromCodeCoverage]
        public static async Task<bool> SendAsync(string body, string toAddress, string fromAddress, string fromName, string subject, bool isHtml, EmailAttachment attachment, string tag)
        {
            try
            {
                if (!IsValid(toAddress))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(fromName))
                {
                    fromAddress = FromAddress;
                    fromName = FromName;
                }

                return await Client.SendAsync(body, toAddress, fromAddress, fromName, subject, isHtml, attachment, tag);
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, flow: ExceptionFlow.Eat, property1: (nameof(TelemetryProperty.Description), toAddress));
            }
            return false;
        }

        [ExcludeFromCodeCoverage]
        internal static async Task<bool> SendBulkAsync(CrmEmail email, IEnumerable<Contact> contacts, string tag)
        {
            return await Client.SendBulkAsync(email, contacts, FromAddress, FromName, tag);
        }

        internal static async Task SetSendGridEmailClientAsync()
        {
            Client = new SendGridEmailClient();
            await Client.InitalizeAsync();
        }

        internal static async Task SetTipiMailClientAsync()
        {
            Client = new TipiMailClient();
            await Client.InitalizeAsync();
        }

        internal static async Task SetUnittestEmailClientAsync()
        {
            Client = new UnitTestEmailClient();
            await Client.InitalizeAsync();
        }

        #endregion
    }
}