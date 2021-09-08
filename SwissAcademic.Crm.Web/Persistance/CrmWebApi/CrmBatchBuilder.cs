using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;

namespace SwissAcademic.Crm.Web
{
    public class CrmBatchBuilder
    {
        #region Felder

        StringBuilder _batchBuilder = new StringBuilder();
        string _url;
        int _contentId = 1;

        #endregion

        #region Konstruktor

        public CrmBatchBuilder()
            :
            this(CrmWebApi.APIUrl)
        {

        }
        public CrmBatchBuilder(string apiUrl, bool withChangeset = true)
        {
            WithChangeset = withChangeset;
            _url = apiUrl;
            Id = DateTime.UtcNow.Ticks.ToString();
        }

        #endregion

        #region Eigenschaften

        public string Id { get; private set; }
        public bool HasContent { get; set; }

        bool WithChangeset { get; }

        #endregion

        #region Methoden

        void AddChangesetHeader()
        {
            _batchBuilder.AppendLine($"--batch_{Id}");
            _batchBuilder.AppendLine($"Content-Type: multipart/mixed;boundary=changeset_{ChangesetId}");
            _batchBuilder.AppendLine();
        }

        string ChangesetId;
        public void AppendEntityChangedHeader(HttpMethod httpMethod, string entityUrl, CrmEntityChanged e)
        {
            var changesetId = string.IsNullOrEmpty(e.TransactionId) ? Id : e.TransactionId;

            if (ChangesetId != changesetId)
            {
                if(!string.IsNullOrEmpty(ChangesetId))
                {
                    _batchBuilder.AppendLine($"--changeset_{ChangesetId}--");
                    _batchBuilder.AppendLine();
                }
                ChangesetId = changesetId;
                AddChangesetHeader();
            }
            e.ContentId = _contentId.ToString();
            HasContent = true;
            _batchBuilder.AppendLine($"--changeset_{ChangesetId}");
            _batchBuilder.AppendLine("Content-Type: application/http");
            _batchBuilder.AppendLine("Content-Transfer-Encoding:binary");
            _batchBuilder.AppendLine();
            _batchBuilder.AppendLine($"{httpMethod.Method} {_url}/{entityUrl} HTTP/1.1");
            _batchBuilder.AppendLine("Content-Type: application/json;type=entry");
            _batchBuilder.AppendLine($"Content-ID: {_contentId}");
            _batchBuilder.AppendLine();
            _contentId++;
        }

        public void AppendRequest(HttpMethod httpMethod, string entityUrl)
        {
            HasContent = true;
            _batchBuilder.AppendLine($"--batch_{Id}");
            _batchBuilder.AppendLine("Content-Type: application/http");
            _batchBuilder.AppendLine("Content-Transfer-Encoding:binary");
            _batchBuilder.AppendLine();
            _batchBuilder.AppendLine($"{httpMethod.Method} {_url}/{entityUrl} HTTP/1.1");
            _batchBuilder.AppendLine("Content-Type: application/json");
            _batchBuilder.AppendLine("OData-Version: 4.0");
            _batchBuilder.AppendLine("OData-MaxVersion: 4.0");
            _batchBuilder.AppendLine();
            _contentId++;
        }

        public void AppendEntityChanged(CrmEntityChanged e, string entityUrl, HttpMethod httpMethod)
        {
            HasContent = true;
            var json = new JObject();
            var entity = e.Entity;
            if (entity.EntityState == EntityState.Created)
            {
                if (EntityNameResolver.HasKeyAttribute(entity.LogicalName))
                {
                    json.Add(CrmAttributeNames.Key, entity.Key);
                }
                json.Add(EntityNameResolver.ResolveAttributeName(entity.LogicalName, nameof(CitaviCrmEntity.Id)), entity.Id);
            }
            foreach (var property in e.Properties)
            {
                if (property == CrmAttributeNames.EntityState ||
                    property == CrmAttributeNames.Key)
                {
                    continue;
                }

                var value = entity[property];

                switch (value)
                {
                    case Enum _:
                        json[property] = (int)value;
                        break;

                    case DateTime datetime:
                        if (datetime.Kind != DateTimeKind.Utc)
                        {
                            json[property] = DateTime.SpecifyKind(datetime, DateTimeKind.Utc);
                        }
                        else
                        {
                            json[property] = datetime;
                        }
                        break;

                    case Decimal dec:
                        json[property] = dec;
                        break;

                    case byte[] bytes:
                        json[property] = bytes;
                        break;

                    default:
                        json[property] = value?.ToString();
                        break;
                }

                if (property == CrmAttributeNames.StatusCode)
                {
                    var state = entity.StatusCode == StatusCode.Inactive ? StateCode.Inactive : StateCode.Active;
                    json.Add("statecode", (int)state);
                }
            }

            AppendEntityChangedHeader(httpMethod, entityUrl, e);
            AppendEntityChangedContent(json);
        }

        public void AppendEntityChangedContent(JObject json)
        {
            HasContent = true;
            _batchBuilder.AppendLine(json.ToString(Formatting.None));
        }

        public void AppendLine()
        {
            _batchBuilder.AppendLine();
        }
        public void AppendLine(string text)
        {
            _batchBuilder.AppendLine(text);
        }

        public void AppendFooter()
        {
            if (WithChangeset)
            {
                _batchBuilder.AppendLine($"--changeset_{ChangesetId}--");
            }
            _batchBuilder.AppendLine();
            _batchBuilder.AppendLine($"--batch_{Id}--");
            _batchBuilder.AppendLine();
        }

        public override string ToString()
        {
            return _batchBuilder.ToString();
        }

        #endregion
    }
}
