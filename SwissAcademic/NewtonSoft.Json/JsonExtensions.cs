using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Newtonsoft.Json
{
    public static class JsonExtensions
    {
        #region Methods

        #region CompressJson

        public static async Task<string> CompressJsonAsync(this string self)
        {
            if (string.IsNullOrEmpty(self)) return self;

            using (var targetStream = new MemoryStream())
            {
                using (var sourceStream = new MemoryStream())
                {
                    using (var streamWriter = new StreamWriter(sourceStream))
                    {
                        streamWriter.Write(self, Encoding.UTF8);
                    }
                    await sourceStream.ZipToStreamAsync(targetStream);
                }
                var resultingBytes = targetStream.ToArray();
                return Convert.ToBase64String(resultingBytes);
            }
        }

        #endregion

        #region DecompressJsonAsync

        public static async Task<string> DecompressJsonAsync(this string self)
        {
            if (string.IsNullOrEmpty(self)) return self;

            using (var targetStream = new MemoryStream())
            {
                using (var sourceStream = new MemoryStream())
                {
                    using (var streamWriter = new StreamWriter(sourceStream))
                    {
                        streamWriter.Write(Convert.FromBase64String(self));
                    }
                    await sourceStream.UnZipToStreamAsync(targetStream);
                }
                var resultingBytes = targetStream.ToArray();
                return Encoding.UTF8.GetString(resultingBytes);
            }
        }

        #endregion

        #region IsValidJson

        public static bool IsValidJson(this string value)
        {
            value = value.Trim();

            if ((value.StartsWith("{") && value.EndsWith("}")) || //For object
                (value.StartsWith("[") && value.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region JsonClone

        public static T JsonClone<T>(this T item)
            where T : class
        {
            var deserializationSettings = new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            return JsonClone(item, null, deserializationSettings);
        }

        public static T JsonClone<T>(this T item, JsonSerializerSettings settings)
            where T : class
        {
            return JsonClone(item, settings, settings);
        }

        public static T JsonClone<T>(this T item, JsonSerializerSettings serializationSettings, JsonSerializerSettings deserializationSettings)
            where T : class
        {
            if (serializationSettings == null) serializationSettings = new JsonSerializerSettings();
            if (deserializationSettings == null) deserializationSettings = new JsonSerializerSettings();
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(item, serializationSettings), deserializationSettings);
        }

        #endregion

        #region StringLength

        public static int StringLength(this JObject jObject)
        {
            var size = 0;

            foreach (var child in jObject.Properties())
            {
                foreach (var value in child.Values())
                {
                    StringLength(value, ref size);
                }
            }

            return size;
        }

        static void StringLength(JToken jToken, ref int size)
        {
            switch (jToken.Type)
            {
                case JTokenType.Property:
                    {
                        foreach (var value in jToken.Values())
                        {
                            StringLength(value, ref size);
                        }
                    }
                    break;

                case JTokenType.Object:
                    break;

                case JTokenType.String:
                    size += jToken.Value<string>().Length;
                    break;
            }
        }

        #endregion

        #endregion
    }
}
