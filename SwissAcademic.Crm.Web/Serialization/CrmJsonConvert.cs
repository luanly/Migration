using Newtonsoft.Json;
using System;

namespace SwissAcademic.Crm.Web
{
    public static class CrmJsonConvert
    {
        #region Eigenschaften

        static Lazy<JsonSerializerSettings> _defaultSettings = new Lazy<JsonSerializerSettings>(() =>
        {
            var settings = new JsonSerializerSettings();
            settings.Configure();
            settings.Converters.Add(new CrmEntityJsonConverter(CitaviSerializationContextType.Entities));
            settings.Converters.Add(new CrmUserJsonConverter(CitaviSerializationContextType.Entities));
            settings.Converters.Add(new VersionConverter());
            settings.ContractResolver = new CrmEntityJsonContractResolver(CitaviSerializationContextType.Entities);
            settings.Formatting = Formatting.None;
            settings.SerializationBinder = new CrmEntitySerializationBinder();
            return settings;
        });
        public static JsonSerializerSettings DefaultSettings
        {
            get { return _defaultSettings.Value; }
        }

        static Lazy<JsonSerializerSettings> _breezeSettings = new Lazy<JsonSerializerSettings>(() =>
        {
            var settings = new JsonSerializerSettings();
            settings.Configure();
            settings.Converters.Add(new CrmEntityJsonConverter(CitaviSerializationContextType.Breeze));
            settings.Converters.Add(new CrmUserJsonConverter(CitaviSerializationContextType.Breeze));
            settings.Converters.Add(new VersionConverter());
            settings.ContractResolver = new CrmEntityJsonContractResolver(CitaviSerializationContextType.Breeze);
            settings.Formatting = Formatting.None;
            settings.SerializationBinder = new CrmEntitySerializationBinder();

            return settings;
        });
        public static JsonSerializerSettings BreezeSettings
        {
            get { return _breezeSettings.Value; }
        }

        #endregion

        #region DeserializeObject

        public static T DeserializeObject<T>(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(value, DefaultSettings);
        }
        public static object DeserializeObject(Type type, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return JsonConvert.DeserializeObject(value, type, DefaultSettings);
        }

        #endregion

        #region SerializeObject

        public static string SerializeObject(object value)
            => SerializeObject(value, CitaviSerializationContextType.Entities);

        public static string SerializeObject(object value, CitaviSerializationContextType serializationContextType)
        {
            if (value == null)
            {
                return null;
            }

            if (serializationContextType == CitaviSerializationContextType.Breeze)
            {
                return JsonConvert.SerializeObject(value, BreezeSettings);
            }
            return JsonConvert.SerializeObject(value, DefaultSettings);
        }

        #endregion
    }
}
