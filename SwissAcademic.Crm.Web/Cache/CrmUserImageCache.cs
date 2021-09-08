using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class CrmUserImageCache
    {
        readonly static byte[] Emtpy = Array.Empty<byte>();

        #region Konstruktor

        static CrmUserImageCache()
        {
            Repo = new BlobStorageRepository();
        }

        #endregion

        #region Eigenschaften

        internal static BlobStorageRepository Repo { get; private set; }

        #endregion

        #region Methoden

        #region AddOrUpdateAsync
        internal static Task AddOrUpdateAsync(string userKey, byte[] image) => Repo.AddOrUpdateAsync(image, userKey);
        #endregion

        #region GetAsync

        public static async Task<byte[]> GetAsync(string key, DataCenter? dataCenter = null) => await GetAsync(null, key, dataCenter);

        internal static async Task<byte[]> GetAsync(CrmDbContext context, string key, DataCenter? dataCenter)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var disposeContext = false;
            try
            {
                var image = await Repo.GetAsync<byte[]>(key, dataCenter);

                if (image == null)
                {
                    if (context == null)
                    {
                        disposeContext = true;
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                        context = new CrmDbContext();
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                    }

                    var query = new Query.FetchXml.GetUserImage(key).TransformText();
                    var result = await context.Fetch(FetchXmlExpression.Create<Contact>(query));

                    if (result != null && result.Any())
                    {
                        var item = result.First() as Contact;
                        image = item.EntityImage;
                    }
                    if (image == null)
                    {
                        return Array.Empty<byte>();
                    }

                    await AddOrUpdateAsync(key, image);
                }
                return image;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                return Array.Empty<byte>();
            }
            finally
            {
                if (disposeContext && context != null)
                {
                    context.Dispose();
                }
            }
        }

        public static async Task<string> GetSharedAccessSignature(string key) => await GetSharedAccessSignature(null, key);
        public static async Task<string> GetSharedAccessSignature(CrmDbContext context, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var disposeContext = false;
            try
            {
                var sas = await Repo.GetSharedAccessSignature(key);

                if (!sas.Success)
                {
                    if (context == null)
                    {
                        disposeContext = true;
#pragma warning disable CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                        context = new CrmDbContext();
#pragma warning restore CA2000 // Objekte verwerfen, bevor Bereich verloren geht
                    }

                    byte[] image = null;
                    var query = new Query.FetchXml.GetUserImage(key).TransformText();
                    var result = await context.Fetch(FetchXmlExpression.Create<Contact>(query));

                    if (result != null && result.Any())
                    {
                        var item = result.First() as Contact;
                        image = item.EntityImage;
                    }
                    if (image == null)
                    {
                        await AddOrUpdateAsync(key, Emtpy);
                        return null;
                    }

                    await AddOrUpdateAsync(key, image);

                    return (await Repo.GetSharedAccessSignature(key)).SharedAccessSignatureUrl;
                }
                return sas.SharedAccessSignatureUrl;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                return null;
            }
            finally
            {
                if (disposeContext && context != null)
                {
                    context.Dispose();
                }
            }
        }

        #endregion

        #region RemoveAsync

        public static Task RemoveAsync(string key) => Repo.RemoveAsync(key);

        #endregion

        #endregion
    }
}
