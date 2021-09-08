using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;

namespace SwissAcademic.Crm.Web
{
    public class AuthenticationTicketLite
    {
        public string AuthenticationScheme { get; set; }
        public ClaimsPrincipal Principal { get; set; }
        public AuthenticationProperties Properties { get; set; }

    }

    public class AuthenticationTicketConverter
        :
        JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(AuthenticationTicket) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var source = serializer.Deserialize<AuthenticationTicketLite>(reader);

            if (source == null)
            {
                return null;
            }

            var ticket = new AuthenticationTicket(source.Principal, source.Properties, source.AuthenticationScheme);
            return ticket;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var source = (AuthenticationTicket)value;

            var target = new AuthenticationTicketLite
            {
                AuthenticationScheme = source.AuthenticationScheme,
                Principal = source.Principal,
                Properties = source.Properties
            };

            serializer.Serialize(writer, target);
        }
    }
}
