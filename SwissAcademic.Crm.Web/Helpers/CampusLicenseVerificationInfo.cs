using System.Collections.Generic;

namespace SwissAcademic.Crm.Web
{
    public class CampusLicenseVerificationInfo
    {
        public bool EmailVerification { get; set; }
        public string EmailVerificationEmailAddress { get; set; }
        public List<string> EmailVerificationEmailDomains { get; } = new List<string>();
        public string HighSchool { get; set; }
        public bool IPVerification { get; set; }
        public bool ShibbolethVerification { get; set; }
        public string ShibbolethEntityId { get; set; }
        public bool VoucherVerification { get; set; }
    }
}
