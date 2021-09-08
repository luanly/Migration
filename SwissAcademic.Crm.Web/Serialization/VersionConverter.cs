using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
    public class VersionConverter : JsonConverter<Version>
    {
        public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string s = (string)reader.Value;
            if (string.IsNullOrEmpty(s))
            {
                return null;
            }
            if (s.Contains("Major"))
            {
                var dict = serializer.Deserialize<Dictionary<string, int>>(reader);
                return new Version(dict["Major"], dict["Minor"], dict["Build"], dict["Revision"]);
            }
            return new Version(s);
        }
    }
}
