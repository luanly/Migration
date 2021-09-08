using System;

namespace SwissAcademic.Crm.Web
{
    public class CitaviRegistrationCookie
        :
        CitaviCookie
    {
        public CitaviRegistrationCookie(string clientId)
        {
            Value = clientId;
        }

        public override string Domain { get; set; }

        public override DateTime? Expires { get; set; } = DateTime.UtcNow.AddMinutes(15);

        public override string Name { get; set; } = CookieNames.RegistrationInfo;
    }
}
