using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
    public class BulkMailQueryPreviewContact
    {
        [JsonProperty]
        public string EmailAddress { get; set; }
        [JsonProperty]
        public Guid Id { get; set; }
        [JsonIgnore]
        public string Key { get; set; }
        [JsonProperty]
        public LanguageType? Language { get; set; }

        public static explicit operator BulkMailQueryPreviewContact(Contact contact)
        {
            return new BulkMailQueryPreviewContact
            {
                EmailAddress = contact.EMailAddress1,
                Key = contact.Key,
                Id = contact.Id,
                Language = contact.Language
            };
        }
    }
}
