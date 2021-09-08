using Newtonsoft.Json.Linq;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.KeyVaultUtils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public static class CrmWebApi
    {
        #region Felder

        static bool Connected;
        static HttpClient Client;
        static HttpMethod DELETE = new HttpMethod("DELETE");
        static HttpMethod PATCH = new HttpMethod("PATCH");
        static HttpMethod POST = new HttpMethod("POST");
        static string BaseUrl;
        static List<CrmAccessTokenClient> AccessTokenClients = new List<CrmAccessTokenClient>();
        public const int MaxSaveChangesCount = 75;
        static HttpClientHandler HttpClientHandler = new HttpClientHandler
        {
            UseCookies = false,
            CheckCertificateRevocationList = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        public static int AccessTokenClientsCount => AccessTokenClients.Count;

        internal static Regex ConnectionStringRegex = new Regex("ConnectionString=(?<ConnectionString>.+?);Password=(?<Password>.+)", RegexOptions.ExplicitCapture);

        #endregion

        #region Eigenschaften

        public static string APIUrl { get; private set; }

        #endregion

        #region Methoden

        #region Connect

        public static Task Connect()
         => Connect(ConfigurationManager.AppSettings["CrmClientId"],
                    ConfigurationManager.AppSettings["CitaviWeb CrmOnline ServiceAccount EmailAddresses"].Split(';'));

        public static async Task Connect(string clientId, string[] serviceAccounts, string crmConnectionUrl = null)
        {
            if (Connected)
            {
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(crmConnectionUrl))
                {
                    crmConnectionUrl = await AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.ServiceAccounts.CitaviWeb_CrmOnline);
                }

                if (AccessTokenClients.Any())
                {
                    foreach (var atc in AccessTokenClients)
                    {
                        atc.Disconnect();
                    }
                }

                AccessTokenClients.Clear();

                var api = "api/data/v9.0/";
                var cs = crmConnectionUrl.Split(';');
                var user = cs[1].Replace("Username=", "").Trim();
                var pass = cs[2].Replace("Password=", "").Replace("\"", "").Trim();
                BaseUrl = cs[0].Replace("Url=", "").Trim();
                APIUrl = UrlBuilder.Combine(BaseUrl, api);

                Client = new HttpClient(HttpClientHandler);
                Client.BaseAddress = new Uri(BaseUrl);
                Client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                Client.DefaultRequestHeaders.Add("OData-Version", "4.0");
                Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                foreach (var serviceAccount in serviceAccounts)
                {
                    var token = new CrmAccessTokenClient();
                    await token.Connect(BaseUrl, serviceAccount, pass, clientId);
                    AccessTokenClients.Add(token);
                }
                Connected = true;
            }
            catch (Exception ignored)
            {
                Connected = false;
            }
        }

        #endregion

        #region Disconnect

        public static void Disconnect()
        {
            foreach (var client in AccessTokenClients)
            {
                client.Disconnect();
            }
            Connected = false;
        }

        #endregion

        #region ExecuteWorkflow

        public static async Task<string> ExecuteWorkflow(string workflowId, string entityId)
        {
            var payload = new JObject();
            payload["EntityId"] = entityId;
            using (var msg = new HttpRequestMessage(HttpMethod.Post, $"{APIUrl}/workflows({workflowId})/Microsoft.Dynamics.CRM.ExecuteWorkflow"))
            {
                msg.Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        ex.Data.Add("ExecuteWorkflow", workflowId);
                        ex.Data.Add("ExecuteWorkflowData", payload);
                        throw ex;
                    }
                    var result = JObject.Parse(responseText);
                    return result["asyncoperationid"].ToString();
                }
            }
        }

        #endregion

        #region GetAccessToken

        static volatile int _getAccessTokenCounter = 0;
        static string GetAccessToken()
        {
            var index = ++_getAccessTokenCounter % AccessTokenClients.Count;
            return AccessTokenClients.ElementAt(index).AccessToken;
        }

        #endregion

        #region GetAsyncOperationStatus

        public static async Task<AsyncOperationStatus> GetAsyncOperationStatus(string asyncoperationid)
        {
            using (var msg = new HttpRequestMessage(HttpMethod.Get, $"{APIUrl}/asyncoperations({asyncoperationid})"))
            {
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        ex.Data.Add("AsyncoperationId", asyncoperationid);
                        throw ex;
                    }
                    var result = JObject.Parse(responseText);
                    return (AsyncOperationStatus)result["statuscode"].Value<int>();
                }
            }
        }

        #endregion

        #region GetCitaviCrmEntitySystemInfo

        internal static async Task<CitaviCrmEntitySystemInfo> GetCitaviCrmEntitySystemInfo(CitaviCrmEntity entity)
		{
            using (var msg = new HttpRequestMessage(HttpMethod.Get, $"{APIUrl}/{EntityNameResolver.GetPluralTypeName(entity.LogicalName)}({entity.Id})?$select=new_key&$expand=createdby($select=fullname,internalemailaddress),modifiedby($select=fullname,internalemailaddress)"))
            {
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new CrmServerException(responseText);
                    }

                    var jObject = JObject.Parse(responseText);
                    var info = new CitaviCrmEntitySystemInfo();
                    info.CreatedByFullName = jObject["createdby"]["fullname"].Value<string>();
                    info.CreatedById = jObject["createdby"]["systemuserid"].Value<string>();
                    info.CreatedByEMail = jObject["createdby"]["internalemailaddress"].Value<string>();

                    info.ModifiedByFullName = jObject["modifiedby"]["fullname"].Value<string>();
                    info.ModifiedById = jObject["modifiedby"]["systemuserid"].Value<string>();
                    info.ModifiedByEMail = jObject["modifiedby"]["internalemailaddress"].Value<string>();

                    return info;
                }
            }
        }

		#endregion

		#region GetRelatedEntities

		public static async Task<IEnumerable<T2>> GetRelatedEntities<T1, T2>(CrmEntityRelationship<T1, T2> relationship, IEnumerable<string> includeAttributes, CrmQueryInfo queryInfo)
            where T1 : CitaviCrmEntity
            where T2 : CitaviCrmEntity
        {
            if (relationship == null)
            {
                throw new NotSupportedException("Relationship must not be null");
            }

            var select = includeAttributes?.Any() == true ? includeAttributes.ToString(",") : "*";
            var expandName = "";
            var url = "";
            if (queryInfo != null && !string.IsNullOrEmpty(queryInfo.NextLink))
            {
                url = queryInfo.NextLink;
                queryInfo.NextLink = null;
            }
            else
            {
                if (relationship.RelationshipType == CrmEntityRelationshipType.ManyToOne)
                {
                    expandName = relationship.RelationshipQueryName;
                }
                else
                {
                    expandName = relationship.RelationshipLogicalName;
                }
                var filter = "";
                if (EntityNameResolver.HasStateCodeAttribute(relationship.SourceEntityLogicalName))
                {
                    filter = "$filter=statecode eq 0";
                }
                var select_source = "";
                if (EntityNameResolver.HasKeyAttribute(relationship.SourceEntityLogicalName))
                {
                    select_source = "$select=new_key&";
                }
                url = $"{APIUrl}/{EntityNameResolver.GetPluralTypeName(relationship.SourceEntityLogicalName)}({relationship.Source.Id})?{select_source}$expand={expandName}($select={select};{filter})";
            }
            using (var msg = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (queryInfo != null)
                {
                    msg.Headers.Add("Prefer", $"odata.maxpagesize={queryInfo.PageSize}");
                }
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                    }
                    var jObject = JObject.Parse(responseText);
                    if (queryInfo != null &&
                        jObject.ContainsKey($"{expandName}@odata.nextLink"))
                    {
                        queryInfo.NextLink = jObject[$"{expandName}@odata.nextLink"].ToString();
                    }
                    if (jObject.ContainsKey("value"))
                    {
                        expandName = "value";
                    }
                    var result = new List<T2>();
                    foreach (var item in jObject.SelectTokens(expandName))
                    {
                        if (!item.HasValues)
                        {
                            continue;
                        }

                        if (item.Type == JTokenType.Object)
                        {
                            result.Add(CrmJsonConvert.DeserializeObject<T2>(item.ToString()));
                        }
                        else
                        {
                            result.AddRange(CrmJsonConvert.DeserializeObject<IEnumerable<T2>>(item.ToString()));
                        }
                    }
                    return result;
                }
            }
        }

        #endregion

        #region GrantOrModifyAccess 

        public static async Task GrantOrModifyAccess(Team team, CitaviCrmEntity entity, CrmAccessRights accessRights, bool modify = false)
        {
            var jObject = new JObject();
            jObject["Target"] = new JObject();
            jObject["Target"][entity._idAttributeName] = entity.Id;
            jObject["Target"]["@odata.type"] = $"Microsoft.Dynamics.CRM.{entity.LogicalName}";

            jObject["PrincipalAccess"] = new JObject();
            jObject["PrincipalAccess"]["Principal"] = new JObject();
            jObject["PrincipalAccess"]["Principal"][team._idAttributeName] = team.Id;
            jObject["PrincipalAccess"]["Principal"]["@odata.type"] = $"Microsoft.Dynamics.CRM.{team.LogicalName}";

            jObject["PrincipalAccess"]["AccessMask"] = accessRights.ToString();

            using (var msg = new HttpRequestMessage(HttpMethod.Post, $"{APIUrl}/{(modify ? "ModifyAccess" : "GrantAccess")}"))
            {
                msg.Content = new StringContent(jObject.ToString(), Encoding.UTF8);
                msg.Content.Headers.Remove("Content-Type");
                msg.Content.Headers.Add("Content-Type", "application/json; charset=utf-8");

                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        throw ex;
                    }
                }
            }
        }

        #endregion

        #region HasUpdate

        public static async Task<bool> HasUpdate<T>(T entity)
            where T : CitaviCrmEntity
        {
            try
            {
                if (string.IsNullOrEmpty(entity.ETag))
                {
                    return true;
                }

                using (var msg = new HttpRequestMessage(HttpMethod.Get, $"{APIUrl}/{EntityNameResolver.GetPluralTypeName(entity.LogicalName)}({entity.Id})?$select={EntityNameResolver.ResolveAttributeName(entity.LogicalName, "id")}"))
                {
                    msg.Headers.IfNoneMatch.Add(EntityTagHeaderValue.Parse(entity.ETag));
                    using (var response = await SendAsync(msg))
                    {
                        return !(response.StatusCode == HttpStatusCode.NotModified);
                    }
                }
            }
            catch (Exception ex)
            {
                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat);
                return true;
            }
        }

        #endregion

        #region IsTooManyRequestsStatusCode

        static bool IsTooManyRequestsStatusCode(HttpResponseMessage response)
            => (int)response.StatusCode == 429 || response.StatusCode == HttpStatusCode.ServiceUnavailable || response.StatusCode == HttpStatusCode.BadGateway;

        #endregion

        #region IsRetryableStatusCode

        static async Task<(bool Retryable, TimeSpan Delay)> IsRetryableStatusCode(HttpResponseMessage response)
        {
            var msg = await response.Content.ReadAsStringAsync();
            (bool Retryable, TimeSpan Delay) retryInfo = (false, default);
            try
            {
                if(response.Headers.Contains("Retry-After"))
				{
                    if(int.TryParse(response.Headers.GetValues("Retry-After").First(), out var retryAfter))
					{
                        retryInfo = (true, TimeSpan.FromSeconds(retryAfter + 5));
                        return retryInfo;
                    }
				}
                if (msg.Contains("Generic SQL error", StringComparison.InvariantCultureIgnoreCase))
                {
                    //System.Exception: Sql error: Generic SQL error. CRM ErrorCode: -2147204784 Sql ErrorCode: -2146232060 Sql Number: 547
                    //Tritt auf wenn Entität erstellt und dann gleich wieder gelöscht wird
                    retryInfo = (true, TimeSpan.FromSeconds(5));
                }
                else if (msg.Contains("There is no active transaction", StringComparison.InvariantCultureIgnoreCase))
                {
                    //There is no active transaction.
                    //This error is usually caused by custom plug-ins that ignore errors from service calls and continue processing.
                    //Irgendein Plugin von MS oder von uns macht ab und zu probleme...
                    retryInfo = (true, TimeSpan.FromSeconds(5));
                }
                else if (msg.Contains("The plug-in execution failed because no Sandbox Hosts are currently available", StringComparison.InvariantCultureIgnoreCase))
                {
                    //The plug-in execution failed because no Sandbox Hosts are currently available.
                    //Please check that you have a Sandbox server configured and that it is running.
                    //Tritt i.d.R. nur auf Alpha auf (selten auf Prod). Imho wenn die Sandbox neu gestartet wird (Updates?)
                    return (true, TimeSpan.FromSeconds(5));
                }
                else if (msg.Contains("No object matched the query: update [AsyncOperationBase]", StringComparison.InvariantCultureIgnoreCase))
                {
                    //Plugin - Mist. Tritt selten beim Löschen auf. Kann "scheinbar" nicht verhindert werden...
                    retryInfo = (true, TimeSpan.FromSeconds(5));
                }
                else if (msg.Contains("AsyncOperation With Id ", StringComparison.InvariantCultureIgnoreCase) && msg.Contains("Does Not Exist", StringComparison.InvariantCultureIgnoreCase))
                {
                    //Plugin - Mist. Tritt selten beim Löschen auf. Kann "scheinbar" nicht verhindert werden...
                    retryInfo = (true, TimeSpan.FromSeconds(5));
                }
                else if (IsTooManyRequestsStatusCode(response))
                {
                    retryInfo = (true, TimeSpan.FromSeconds(30));
                }
            }
            finally
            {
                if (retryInfo.Retryable)
                {
                    Telemetry.TrackDiagnostics($"CrmWebApi retry after error: {msg}");
                }
            }
            return retryInfo;
        }

        #endregion

        #region ReadAsMultipartAsync
        //https://github.com/dotnet/corefx/issues/12352
        static async Task<List<HttpResponseMessage>> ReadAsMultipartAsync(HttpResponseMessage response)
        {
            var multipartContent = await response.Content.ReadAsMultipartAsync();
            var responseMessages = new List<HttpResponseMessage>();
            foreach (var content in multipartContent.Contents)
            {
                if (content.Headers.ContentType.MediaType.Equals("application/http", StringComparison.OrdinalIgnoreCase))
                {
                    if (!content.Headers.ContentType.Parameters.Any(parameter => parameter.Name.Equals("msgtype", StringComparison.OrdinalIgnoreCase) &&
                         parameter.Value.Equals("response", StringComparison.OrdinalIgnoreCase)))
                    {
                        content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("msgtype", "response"));
                    }
                    var msg = await content.ReadAsHttpResponseMessageAsync();
                    if (content.Headers.TryGetValues(CrmODataConstants.ContentId, out var val))
                    {
                        msg.Headers.Add(CrmODataConstants.ContentId, val);
                    }
                    responseMessages.Add(msg);
                }
                else
                {
                    var subMultipartContent = await content.ReadAsMultipartAsync();
                    foreach (var subContent in subMultipartContent.Contents)
                    {
                        subContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("msgtype", "response"));
                        var msg = await subContent.ReadAsHttpResponseMessageAsync();
                        if (subContent.Headers.TryGetValues(CrmODataConstants.ContentId, out var val))
                        {
                            msg.Headers.Add(CrmODataConstants.ContentId, val);
                        }
                        responseMessages.Add(msg);
                    }
                }
            }
            return responseMessages;
        }
        #endregion

        #region RetrieveAttributeMetadata

        public static Task<JObject> RetrieveAttributeMetadata<T>(params string[] selects)
            where T : CitaviCrmEntity
         => RetrieveAttributeMetadata(EntityNameResolver.GetEntityLogicalName<T>(), selects);

        public static async Task<JObject> RetrieveAttributeMetadata(string entityLocicalName, params string[] selects)
        {
            var url = $"{APIUrl}/EntityDefinitions(LogicalName='{entityLocicalName}')/Attributes(LogicalName='{selects.ToString(",")}')";
            using (var msg = new HttpRequestMessage(HttpMethod.Get, url))
            {
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        throw ex;
                    }
                    return JObject.Parse(responseText);
                }
            }
        }

		#endregion

		#region RetrieveAggregation
		static async Task<int> RetrieveAggregation(FetchXmlExpression fetch, string fldName)
        {
            using (var msg = new HttpRequestMessage(HttpMethod.Get, $"{APIUrl}/{EntityNameResolver.GetPluralTypeName(fetch.EntityName)}?fetchXml={fetch.ToOData()}"))
            {
                if (fetch.PageSize.HasValue)
                {
                    msg.Headers.Add("Prefer", "odata.include-annotations=Microsoft.Dynamics.CRM.fetchxmlpagingcookie");
                }

                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        ex.Data.Add("FetchXML", fetch.Xml);
                        throw ex;
                    }
                    var jObject = JObject.Parse(responseText);
                    if (!jObject["value"].HasValues)
                    {
                        return 0;
                    }

                    return jObject["value"][0][fldName].Value<int>();
                }
            }
        }

        #endregion

        #region RetrieveProperty

        public static async Task<T> RetrieveProperty<T>(string entityName, Guid id, string property, string expandEntityName)
        {
            HttpRequestMessage msg;
            if (!string.IsNullOrEmpty(expandEntityName))
            {
                msg = new HttpRequestMessage(HttpMethod.Get, $"{APIUrl}/{EntityNameResolver.GetPluralTypeName(entityName)}({id})?$select=new_key&$expand={expandEntityName}($select={property})");
            }
            else
            {
                msg = new HttpRequestMessage(HttpMethod.Get, $"{APIUrl}/{EntityNameResolver.GetPluralTypeName(entityName)}({id})?$select={property}");
            }
            using (msg)
            {
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        ex.Data["EntityName"] = entityName;
                        ex.Data["Id"] = id;
                        ex.Data["Property"] = property;
                        ex.Data["ExpandEntityName"] = expandEntityName;
                        ex.Data["Uri"] = msg.RequestUri.ToString();
                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                    }
                    var jObject = JObject.Parse(responseText);
                    try
                    {
                        if (!string.IsNullOrEmpty(expandEntityName))
                        {
                            if (jObject.ContainsKey(expandEntityName))
                            {
                                return jObject[expandEntityName][property].Value<T>();
                            }
                        }
                        if (jObject.ContainsKey(property))
                        {
                            return jObject[property].Value<T>();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is InvalidOperationException &&
                           jObject.ContainsKey("new_key") &&
                           jObject["new_key"].Value<string>().StartsWith(CrmConstants.UnitTestCrmEntityKeyPrefix, StringComparison.InvariantCultureIgnoreCase))
                        {
                            //Kann während webtests auftreten
                            //new_citaviproductid  null
                            return default;
                        }
                        ex.Data["EntityName"] = entityName;
                        ex.Data["Id"] = id;
                        ex.Data["Property"] = property;
                        ex.Data["ExpandEntityName"] = expandEntityName;
                        ex.Data["Uri"] = msg.RequestUri.ToString();
                        if (!string.IsNullOrEmpty(responseText))
                        {
                            ex.Data["ResponseText"] = responseText;
                        }
                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                    }
                    return default;
                }
            }
        }

        #endregion

        #region RetrieveCount

        public static async Task<int> RetrieveCount(FetchXmlExpression fetch)
            => await RetrieveAggregation(fetch, "count");

        public static async Task<int> RetrieveCount(string entityName)
        {
            var msg = new HttpRequestMessage(HttpMethod.Get, $"{APIUrl}/RetrieveTotalRecordCount(EntityNames=['{entityName}'])");
            using (msg)
            {
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        ex.Data["EntityName"] = entityName;
                        ex.Data["Uri"] = msg.RequestUri.ToString();
                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                    }
                    var jObject = JObject.Parse(responseText);
                    return jObject["EntityRecordCountCollection"]["Values"].First().Value<int>();
                }
            }
        }

        #endregion

        #region RetrieveRelationshipChanges

        public static async Task<IEnumerable<CitaviCrmIntersecEntity>> RetrieveRelationshipChanges(FetchXmlExpression expr)
        {
            var results = new List<CitaviCrmIntersecEntity>();
            var changes = new List<CitaviCrmIntersecEntity>();
            var lastResults = new List<CitaviCrmIntersecEntity>();

            if (expr.Bag.Any())
            {
                lastResults.AddRange(expr.Bag.FirstOrDefault() as List<CitaviCrmIntersecEntity>);
            }
            expr.Reset();

            var entityName = EntityNameResolver.GetPluralTypeName(expr.EntityName);
            do
            {
                var url = $"{APIUrl}/{entityName}?fetchXml={expr.ToOData()}";

                using (var msg = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    if (expr.PageSize.HasValue)
                    {
                        msg.Headers.Add("Prefer", "odata.include-annotations=Microsoft.Dynamics.CRM.fetchxmlpagingcookie");
                    }

                    using (var response = await SendAsync(msg))
                    {
                        var responseText = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode)
                        {
                            var ex = new CrmServerException(responseText);
                            ex.Data.Add("FetchXML", ((FetchXmlExpression)expr).Xml);
                            ex.Data["EntityName"] = expr.EntityName;
                            Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                        }
                        var jObject = JObject.Parse(responseText);
                        if (!jObject["value"].HasValues)
                        {
                            break;
                        }

                        if (expr.PageSize.HasValue &&
                            jObject["value"].Children().Count() == expr.PageSize.Value)
                        {
                            if (jObject.ContainsKey("@Microsoft.Dynamics.CRM.fetchxmlpagingcookie"))
                            {
                                expr.NextLink = jObject["@Microsoft.Dynamics.CRM.fetchxmlpagingcookie"].ToString();
                            }
                            else if (jObject.ContainsKey("@odata.nextLink"))
                            {
                                expr.NextLink = jObject["@odata.nextLink"].ToString();
                            }
                        }

                        foreach (var jtoken in jObject["value"].Children<JObject>())
                        {
                            var intersecEntity = CitaviCrmIntersecEntity.Parse(jtoken);
                            results.Add(intersecEntity);
                        }
                    }
                }
            }
            while (expr.HasMoreResults);

            if (expr.Bag.Any())
            {
                expr.Bag.Clear();
            }
            expr.Bag.Add(results);

            if (!lastResults.Any())
            {
                //Initial request - no changes
                return Enumerable.Empty<CitaviCrmIntersecEntity>();
            }
            foreach (var result in results)
            {
                if (!lastResults.Contains(result))
                {
                    //New relationships
                    changes.Add(result);
                }
            }
            foreach (var result in lastResults)
            {
                if (!results.Contains(result))
                {
                    //Deleted relationships
                    changes.Add(result);
                }
            }
            return changes;
        }

        #endregion

        #region RetrieveMetadata

        public static Task<JObject> RetrieveMetadata<T>(params string[] selects)
            where T : CitaviCrmEntity
         => RetrieveMetadata(EntityNameResolver.GetEntityLogicalName<T>(), selects);

        public static async Task<JObject> RetrieveMetadata(string entityLocicalName, params string[] selects)
        {
            var url = $"{APIUrl}/EntityDefinitions(LogicalName='{entityLocicalName}')";
			if (selects.Any())
			{
                url = $"{APIUrl}/EntityDefinitions(LogicalName='{entityLocicalName}')?$select={selects.ToString(",")}";
            }
            using (var msg = new HttpRequestMessage(HttpMethod.Get, url))
            {
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        throw ex;
                    }
                    return JObject.Parse(responseText);
                }
            }
        }

        #endregion

        #region RetrieveMultiple

        public static async Task<IEnumerable<CitaviCrmEntity>> RetrieveMultiple(string odata)
        {
            var url = $"{APIUrl}/{odata}";

            using (var msg = new HttpRequestMessage(HttpMethod.Get, url))
            {
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        ex.Data["odata"] = odata;

                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                    }
                    var jObject = JObject.Parse(responseText);
                    if (!jObject["value"].HasValues)
                    {
                        return Enumerable.Empty<CitaviCrmEntity>();
                    }

                    var typeName = EntityNameResolver.GetSignlarTypeName(Regex.Match(jObject["@odata.context"].ToString(), "metadata#(?<TYPE>.+?)\\(").Groups["TYPE"].Value);
                    var type = EntityNameResolver.KnownTypes.First(i => i.EntityLogicalName == typeName).IEnumerableType;
                    return CrmJsonConvert.DeserializeObject(type, jObject["value"].ToString()) as IEnumerable<CitaviCrmEntity>;
                }
            }
        }

        public static async Task<IEnumerable<CitaviCrmEntity>> RetrieveMultiple(ExpressionBase expr)
        {
            var url = "";
            var entityName = EntityNameResolver.GetPluralTypeName(expr.EntityName);
            if (expr is FetchXmlExpression)
            {
                url = $"{APIUrl}/{entityName}?fetchXml={expr.ToOData()}";
            }
            else
            {
                if (!string.IsNullOrEmpty(expr.NextLink))
                {
                    url = expr.NextLink;
                }
                else
                {
                    url = $"{APIUrl}/{entityName}?{expr.ToOData()}";
                }
            }
            expr.NextLink = null;
            using (var msg = new HttpRequestMessage(HttpMethod.Get, url))
            {
                if (expr.PageSize.HasValue)
                {
                    if (expr is FetchXmlExpression)
                    {
                        msg.Headers.Add("Prefer", "odata.include-annotations=Microsoft.Dynamics.CRM.fetchxmlpagingcookie");
                    }
                    else
                    {
                        msg.Headers.Add("Prefer", $"odata.maxpagesize={expr.PageSize.Value}");
                    }
                }

                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        if (expr is FetchXmlExpression)
                        {
                            ex.Data.Add("FetchXML", ((FetchXmlExpression)expr).Xml);
                        }
                        ex.Data["EntityName"] = expr.EntityName;

                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                    }
                    var jObject = JObject.Parse(responseText);
                    if (!jObject["value"].HasValues)
                    {
                        return Enumerable.Empty<CitaviCrmEntity>();
                    }

                    if (expr.PageSize.HasValue &&
                        jObject["value"].Children().Count() == expr.PageSize.Value)
                    {
                        if (jObject.ContainsKey("@Microsoft.Dynamics.CRM.fetchxmlpagingcookie"))
                        {
                            expr.NextLink = jObject["@Microsoft.Dynamics.CRM.fetchxmlpagingcookie"].ToString();
                        }
                        else if (jObject.ContainsKey("@odata.nextLink"))
                        {
                            expr.NextLink = jObject["@odata.nextLink"].ToString();
                        }
                    }

                    var typeName = EntityNameResolver.GetSignlarTypeName(Regex.Match(jObject["@odata.context"].ToString(), "metadata#(?<TYPE>.+?)\\(").Groups["TYPE"].Value);
                    var type = EntityNameResolver.KnownTypes.First(i => i.EntityLogicalName == typeName).IEnumerableType;
                    return CrmJsonConvert.DeserializeObject(type, jObject["value"].ToString()) as IEnumerable<CitaviCrmEntity>;
                }
            }
        }

        internal static async Task<IEnumerable<CitaviCrmEntity>> RetrieveMultiple(IEnumerable<FetchXmlExpression> fetchXmls)
        {
            var batchBuilder = new CrmBatchBuilder(APIUrl, withChangeset: false);

            foreach (var fetchXml in fetchXmls)
            {
                batchBuilder.AppendRequest(HttpMethod.Get, $"{EntityNameResolver.GetPluralTypeName(fetchXml.EntityName)}?fetchXml={fetchXml.ToOData()}");
            }

            batchBuilder.AppendFooter();

            var batchString = batchBuilder.ToString();

            using (var batchRequest = new HttpRequestMessage(HttpMethod.Post, $"{APIUrl}/$batch"))
            {
                batchRequest.Content = new StringContent(batchString, Encoding.UTF8);
                batchRequest.Content.Headers.Remove("Content-Type");
                batchRequest.Content.Headers.Add("Content-Type", $"multipart/mixed;boundary=batch_{batchBuilder.Id}");

                using (var response = await SendAsync(batchRequest))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        var ex = new CrmServerException(responseText);
                        ex.Data.Add("BatchString", batchString);
                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                    }

                    var multipart = await response.Content.ReadAsMultipartAsync();
                    var result = new List<CitaviCrmEntity>();
                    foreach (var content in multipart.Contents)
                    {
                        content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("msgtype", "response"));
                        using (var msg = await content.ReadAsHttpResponseMessageAsync())
                        {
                            var responseText = await msg.Content.ReadAsStringAsync();
                            var jObject = JObject.Parse(responseText);

                            if (!jObject["value"].HasValues)
                            {
                                continue;
                            }

                            var typeName = EntityNameResolver.GetSignlarTypeName(Regex.Match(jObject["@odata.context"].ToString(), "metadata#(?<TYPE>.+?)\\(").Groups["TYPE"].Value);
                            var type = EntityNameResolver.KnownTypes.First(i => i.EntityLogicalName == typeName).IEnumerableType;
                            try
                            {
                                var r = CrmJsonConvert.DeserializeObject(type, jObject["value"].ToString()) as IEnumerable<CitaviCrmEntity>;
                                result.AddRange(r);
                            }
                            catch (Exception ex)
                            {
                                ex.Data.Add("typeName", typeName);
                                ex.Data.Add("value", jObject["value"].ToString());
                                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                            }
                        }
                    }
                    return result;
                }
            }
        }

        internal static async Task<IEnumerable<CitaviCrmEntity>> RetrieveMultiple(IEnumerable<QueryExpression> queries)
        {
            var batchBuilder = new CrmBatchBuilder(APIUrl, withChangeset: false);

            foreach (var fetchXml in queries)
            {
                batchBuilder.AppendRequest(HttpMethod.Get, $"{EntityNameResolver.GetPluralTypeName(fetchXml.EntityName)}?{fetchXml.ToOData()}");
            }

            batchBuilder.AppendFooter();

            var batchString = batchBuilder.ToString();

            using (var batchRequest = new HttpRequestMessage(HttpMethod.Post, $"{APIUrl}/$batch"))
            {
                batchRequest.Content = new StringContent(batchString, Encoding.UTF8);
                batchRequest.Content.Headers.Remove("Content-Type");
                batchRequest.Content.Headers.Add("Content-Type", $"multipart/mixed;boundary=batch_{batchBuilder.Id}");

                using (var response = await SendAsync(batchRequest))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        var ex = new CrmServerException(responseText);
                        ex.Data.Add("BatchString", batchString);
                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                    }

                    var multipart = await response.Content.ReadAsMultipartAsync();
                    var result = new List<CitaviCrmEntity>();
                    foreach (var content in multipart.Contents)
                    {
                        content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("msgtype", "response"));
                        using (var msg = await content.ReadAsHttpResponseMessageAsync())
                        {
                            var responseText = await msg.Content.ReadAsStringAsync();
                            var jObject = JObject.Parse(responseText);

                            if (!jObject["value"].HasValues)
                            {
                                continue;
                            }

                            var typeName = EntityNameResolver.GetSignlarTypeName(Regex.Match(jObject["@odata.context"].ToString(), "metadata#(?<TYPE>.+?)\\(").Groups["TYPE"].Value);
                            var type = EntityNameResolver.KnownTypes.First(i => i.EntityLogicalName == typeName).IEnumerableType;
                            try
                            {
                                var r = CrmJsonConvert.DeserializeObject(type, jObject["value"].ToString()) as IEnumerable<CitaviCrmEntity>;
                                result.AddRange(r);
                            }
                            catch (Exception ex)
                            {
                                ex.Data.Add("typeName", typeName);
                                ex.Data.Add("value", jObject["value"].ToString());
                                Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                            }
                        }
                    }
                    return result;
                }
            }
        }

        #endregion

        #region RetrieveSharedPrincipalsAndAccess

        public static async Task<IEnumerable<(Guid TargetId, CrmAccessRights AccessRights)>> RetrieveSharedPrincipalsAndAccess(CitaviCrmEntity entity)
        {
            var target = $"'@odata.id':'{EntityNameResolver.GetPluralTypeName(entity.LogicalName)}({entity.Id})'";
            using (var msg = new HttpRequestMessage(HttpMethod.Get, $"{APIUrl}/RetrieveSharedPrincipalsAndAccess(Target=@tid)?@tid=" + "{" + target + "}"))
            {
                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        throw ex;
                    }
                    var result = new List<(Guid, CrmAccessRights)>();
                    var json = JObject.Parse(responseText);
                    foreach (var principalAccess in json["PrincipalAccesses"])
                    {
                        var access = (CrmAccessRights)principalAccess["AccessMask"].Value<string>()
                                                                  .Split(',')
                                                                  .Select(s => s.Trim().ParseEnum<CrmAccessRights>())
                                                                  .Sum(s => (int)s);
                        result.Add((Guid.Parse(principalAccess["Principal"]["ownerid"].Value<string>()), access));
                    }
                    return result;
                }
            }
        }

        #endregion

        #region RetrieveSum

        public static async Task<int> RetrieveSum(FetchXmlExpression fetch)
            => await RetrieveAggregation(fetch, "sum");

        #endregion

        #region SaveAsync

        public static async Task<int> SaveAsync(IEnumerable<CrmEntityChanged> changes, bool withSaveChangesQueueCheck = true, bool continueOnError = false, string impersonatedUserID = null)
        {
            IEnumerable<CrmEntityChanged> list;
            var page = 0;
            var changesCount = 0;
            changes.ForEach(c => c.CalculateEstimatedOperations());

            if (changes.Any(change => !string.IsNullOrEmpty(change.TransactionId)))
            {
                return await SaveWithTransactionsAsync(changes, withSaveChangesQueueCheck: withSaveChangesQueueCheck, continueOnError: continueOnError, impersonatedUserID: impersonatedUserID);
            }

            while ((list = changes.Skip(page * MaxSaveChangesCount).Take(MaxSaveChangesCount)).Any())
            {
                if (list.Sum(c => c.OperationsCount) >= MaxSaveChangesCount)
                {
                    changesCount += await SaveAsyncPrivate(list.Take(list.Count() / 2), withSaveChangesQueueCheck: withSaveChangesQueueCheck, continueOnError: continueOnError, impersonatedUserID: impersonatedUserID);
                    changesCount += await SaveAsyncPrivate(list.Skip(list.Count() / 2), withSaveChangesQueueCheck: withSaveChangesQueueCheck, continueOnError: continueOnError, impersonatedUserID: impersonatedUserID);
                }
                else
                {
                    changesCount += await SaveAsyncPrivate(list, withSaveChangesQueueCheck: withSaveChangesQueueCheck, continueOnError: continueOnError, impersonatedUserID: impersonatedUserID);
                }
                page++;
            }

            return changesCount;
        }

        static async Task<int> SaveWithTransactionsAsync(IEnumerable<CrmEntityChanged> changes, bool withSaveChangesQueueCheck = true, bool continueOnError = false, string impersonatedUserID = null)
        {
            var list = new List<CrmEntityChanged>();
            CrmServerException serverException = null;
            var changesCount = 0;
            var page = 0;
            async Task SaveTransactions()
            {
                try
                {
                    changesCount += await SaveAsyncPrivate(list, withSaveChangesQueueCheck: withSaveChangesQueueCheck, continueOnError: continueOnError, impersonatedUserID: impersonatedUserID);
                }
                catch (CrmServerException ex)
                {
                    if (serverException == null)
                    {
                        serverException = ex;
                    }
                    else
                    {
                        serverException.FailedSaveRequests.AddRange(ex.FailedSaveRequests);
                        serverException.Data[$"BatchString{page}"] = ex.Data["BatchString"];
                    }
                }
                list.Clear();
            };

            for (var i = 0; i < changes.Count(); i++)
            {
                var t = changes.ElementAt(i);
                if (!string.IsNullOrEmpty(t.TransactionId))
                {
                    var sameTransactionItems = changes.Skip(i).TakeWhile(r => r.TransactionId == t.TransactionId).ToList();
                    if (list.Count + sameTransactionItems.Count >= MaxSaveChangesCount - 1)
                    {
                        await SaveTransactions();
                    }
                    list.AddRange(sameTransactionItems);
                    i += sameTransactionItems.Count - 1;
                }
                else
                {
                    list.Add(t);
                }
                if (list.Count >= MaxSaveChangesCount)
                {
                    await SaveTransactions();
                    page++;
                }
            }
            if (list.Any())
            {
                await SaveTransactions();
            }

            if (serverException != null)
            {
                throw serverException;
            }
            return changesCount;
        }

        static async Task<int> SaveAsyncPrivate(IEnumerable<CrmEntityChanged> changes, bool withSaveChangesQueueCheck = true, bool continueOnError = false, string impersonatedUserID = null)
        {
            if (withSaveChangesQueueCheck)
            {
                var savedViaQueue = await SaveChangesQueue.TryAdd(changes);
                if (changes.Count() == savedViaQueue.Count())
                {
                    return savedViaQueue.Count();
                }
                changes = changes.Except(savedViaQueue);
            }

            var batchBuilder = new CrmBatchBuilder(APIUrl);

            foreach (var c in changes)
            {
                var operations = c.Operations;

                #region Create

                if (operations.HasFlag(CrmWebApiSaveOperations.Create))
                {
                    batchBuilder.AppendEntityChanged(c, $"{EntityNameResolver.GetPluralTypeName(c.Entity.LogicalName)}", POST);
                }

                #endregion

                #region Update

                if (operations.HasFlag(CrmWebApiSaveOperations.Update))
                {
                    batchBuilder.AppendEntityChanged(c, $"{EntityNameResolver.GetPluralTypeName(c.Entity.LogicalName)}({c.Entity.Id.ToString()})", PATCH);
                }

                #endregion

                #region Deleted

                if (operations.HasFlag(CrmWebApiSaveOperations.Delete))
                {
                    batchBuilder.AppendEntityChangedHeader(DELETE, $"{EntityNameResolver.GetPluralTypeName(c.Entity.LogicalName)}({c.Entity.Id.ToString()})", c);
                }

                #endregion

                #region Merge

                if (operations.HasFlag(CrmWebApiSaveOperations.Merge))
                {
                    var merge = c.Merge;
                    var jObject = new JObject();
                    jObject["Target"] = new JObject();
                    jObject["Target"]["@odata.type"] = $"Microsoft.Dynamics.CRM.{merge.Target.LogicalName}";
                    jObject["Target"][merge.Target._idAttributeName] = merge.Target.Id.ToString();

                    jObject["Subordinate"] = new JObject();
                    jObject["Subordinate"]["@odata.type"] = $"Microsoft.Dynamics.CRM.{merge.Target.LogicalName}";
                    jObject["Subordinate"][merge.Target._idAttributeName] = merge.SubordinateId.ToString();

                    jObject["UpdateContent"] = new JObject();
                    jObject["UpdateContent"][merge.Target._idAttributeName] = Guid.Empty.ToString();
                    jObject["UpdateContent"]["@odata.type"] = $"Microsoft.Dynamics.CRM.{merge.Target.LogicalName}";

                    jObject["PerformParentingChecks"] = false;

                    using (var mergeRequest = new HttpRequestMessage(HttpMethod.Post, $"{APIUrl}/Merge"))
                    {
                        mergeRequest.Content = new StringContent(jObject.ToString(), Encoding.UTF8);
                        mergeRequest.Content.Headers.Remove("Content-Type");
                        mergeRequest.Content.Headers.Add("Content-Type", "application/json; charset=utf-8");

                        using (var mergeResponse = await SendAsync(mergeRequest))
                        {
                            if (!mergeResponse.IsSuccessStatusCode)
                            {
                                var responseText = await mergeResponse.Content.ReadAsStringAsync();
                                var ex = new CrmServerException(responseText);
                                throw ex;
                            }
                        }
                    }
                }

                #endregion

                #region Relations

                if (operations.HasFlag(CrmWebApiSaveOperations.Relations))
                {
                    foreach (var associate in c.Associate)
                    {
                        foreach (var related in associate.relatatedEntities)
                        {
                            var rel = associate.relationship.RelationshipType == CrmEntityRelationshipType.OneToMany ?
                                      associate.relationship.RelationshipLogicalName :
                                      associate.relationship.RelationshipQueryName;
                            batchBuilder.AppendEntityChangedHeader(POST, $"{EntityNameResolver.GetPluralTypeName(c.Entity.LogicalName)}({c.Entity.Id.ToString()})/{rel}/$ref", c);
                            var json = new JObject();
                            json.Add("@odata.id", $"{APIUrl}/{EntityNameResolver.GetPluralTypeName(related.LogicalName)}({related.Id.ToString()})");
                            batchBuilder.AppendEntityChangedContent(json);
                        }
                    }
                    foreach (var disassociate in c.Disassociate)
                    {
                        foreach (var related in disassociate.relatatedEntities)
                        {
                            var rel = disassociate.relationship.RelationshipType == CrmEntityRelationshipType.OneToMany ?
                                      disassociate.relationship.RelationshipLogicalName :
                                      disassociate.relationship.RelationshipQueryName;

                            if (disassociate.relationship.RelationshipType == CrmEntityRelationshipType.OneToMany ||
                                disassociate.relationship.RelationshipType == CrmEntityRelationshipType.ManyToMany)
                            {
                                batchBuilder.AppendEntityChangedHeader(DELETE, $"{EntityNameResolver.GetPluralTypeName(c.Entity.LogicalName)}({c.Entity.Id.ToString()})/{rel}({related.Id.ToString()})/$ref", c);
                            }
                            else
                            {
                                batchBuilder.AppendEntityChangedHeader(DELETE, $"{EntityNameResolver.GetPluralTypeName(c.Entity.LogicalName)}({c.Entity.Id.ToString()})/{rel}/$ref", c);
                            }
                        }
                    }
                }

                #endregion
            }

            if (!batchBuilder.HasContent)
            {
                return 0;
            }

            batchBuilder.AppendFooter();

            var batchString = batchBuilder.ToString();

            using (var batchRequest = new HttpRequestMessage(HttpMethod.Post, $"{APIUrl}/$batch"))
            {
                batchRequest.Content = new StringContent(batchString, Encoding.UTF8);
                batchRequest.Content.Headers.Remove("Content-Type");
                batchRequest.Content.Headers.Add("Content-Type", $"multipart/mixed;boundary=batch_{batchBuilder.Id}");

				if (!string.IsNullOrEmpty(impersonatedUserID))
				{
                    batchRequest.Content.Headers.Add("MSCRMCallerID", impersonatedUserID);
                }

                if (continueOnError)
                {
                    batchRequest.Content.Headers.Add("Prefer", "odata.continue-on-error");
                }
                using (var response = await SendAsync(batchRequest))
                {
                    if (!response.IsSuccessStatusCode || continueOnError)
                    {
                        var result = await ValidateResponse(response);
                        if (!result.Success)
                        {
                            var exceptions = new List<Exception>();
                            var failedRequests = new List<FailedSaveRequest>();

                            foreach (var odataResponse in result.MultipartResponses)
                            {
                                if (odataResponse.IsSuccessStatusCode)
                                {
                                    continue;
                                }
                                var json = JObject.Parse(await odataResponse.Content.ReadAsStringAsync());
                                var error = json["error"] as JObject;
                                if (odataResponse.Headers.TryGetValues(CrmODataConstants.ContentId, out var contentId))
                                {
                                    var failed = changes.FirstOrDefault(c => c.ContentId == contentId.First());
                                    if (failed != null)
                                    {
                                        if (!string.IsNullOrEmpty(failed.TransactionId))
                                        {
                                            foreach (var f in changes.Where(c => c.TransactionId == failed.TransactionId &&
                                                                                 !failedRequests.Any(ec => c.ContentId == ec.ContentId)))
                                            {
                                                var fs = new FailedSaveRequest();
                                                fs.ContentId = f.ContentId;
                                                fs.CrmEntityChanged = f;
                                                fs.StatusCode = odataResponse.StatusCode;
                                                if (error.ContainsKey("code"))
                                                {
                                                    fs.ErrorCode = error["code"].ToString();
                                                }
                                                if (error.ContainsKey("message"))
                                                {
                                                    fs.ErrorMessage = error["message"].ToString();
                                                    var innerException = new Exception(error["message"].ToString());
                                                    exceptions.Add(innerException);
                                                    Telemetry.TrackException(innerException, SeverityLevel.Error, ExceptionFlow.Eat);
                                                }

                                                failedRequests.Add(fs);
                                            }
                                        }
                                        else
                                        {
                                            var fs = new FailedSaveRequest();
                                            fs.ContentId = failed.ContentId;
                                            fs.CrmEntityChanged = failed;
                                            fs.StatusCode = odataResponse.StatusCode;
                                            if (error.ContainsKey("code"))
                                            {
                                                fs.ErrorCode = error["code"].ToString();
                                            }
                                            if (error.ContainsKey("message"))
                                            {
                                                fs.ErrorMessage = error["message"].ToString();
                                                var innerException = new Exception(error["message"].ToString());
                                                exceptions.Add(innerException);
                                                Telemetry.TrackException(innerException, SeverityLevel.Error, ExceptionFlow.Eat);
                                            }
                                            failedRequests.Add(fs);
                                        }
                                    }
                                }
                            }
                            var ex = new CrmServerException("CrmWebApi SaveAsync failed", exceptions);
                            ex.FailedSaveRequests.AddRange(failedRequests);
                            ex.Data["BatchString"] = batchString;
                            ex.Data["FailedSaveRequests"] = CrmJsonConvert.SerializeObject(ex.FailedSaveRequests);
                            Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Rethrow);
                        }
                    }
                    changes.ForEach(c => c.Reset());
                    return changes.Count();
                }
            }
        }

        #endregion

        #region SendAsync
        static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest, string accessToken = null, bool retry = true)
        {
            HttpResponseMessage response = null;

            if (accessToken == null)
            {
                accessToken = GetAccessToken();
            }
            if (!httpRequest.Headers.IfNoneMatch.Any())
            {
                httpRequest.Headers.Add("If-None-Match", "");
            }
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            try
            {
                response = await Client.SendAsync(httpRequest);
            }
            catch (TaskCanceledException)
            {
                await Task.Delay(1000);
                using (var request = await httpRequest.CloneAsync())
                {
                    response = await Client.SendAsync(request);
                }
            }

            if (!response.IsSuccessStatusCode &&
                retry)
            {

                var (Retryable, Delay) = await IsRetryableStatusCode(response);
                if (Retryable)
                {
                    response.Dispose();
                    if (Delay != default)
                    {
                        Telemetry.TrackDiagnostics($"RateLimit: Retry-After: {Delay}");
                        await Task.Delay(Delay);
                    }

                    using (var msg = await httpRequest.CloneAsync())
                    {
                        response = await SendAsync(msg, accessToken, false);
                        if (response.IsSuccessStatusCode)
                        {
                            return response;
                        }
                    }

                    var accessTokens = AccessTokenClients.Select(c => c.AccessToken).ToList();
                    accessTokens.Remove(accessToken);
                    foreach (var at in accessTokens)
                    {
                        using (var msg = await httpRequest.CloneAsync())
                        {
                            response = await SendAsync(msg, at, false);
                            if (response.IsSuccessStatusCode)
                            {
                                return response;
                            }
                        }
                    }
                }
            }

            return response;
        }

        #endregion

        #region ValidateResponse

        static async Task<(bool Success, IEnumerable<HttpResponseMessage> MultipartResponses)> ValidateResponse(HttpResponseMessage response)
        {
            var responses = await ReadAsMultipartAsync(response);
            return (Success: responses.All(c => c.IsSuccessStatusCode), responses);
        }

        #endregion

        #region UpdateProperty

        public static async Task UpdateProperty(string entityName, string entityId, string propertyName, string propertyValue)
        {
            var jObject = new JObject();
            jObject[propertyName] = propertyValue;

            using (var msg = new HttpRequestMessage(PATCH, $"{APIUrl}/{entityName}({entityId})"))
            {
                msg.Content = new StringContent(jObject.ToString(), Encoding.UTF8);
                msg.Content.Headers.Remove("Content-Type");
                msg.Content.Headers.Add("Content-Type", "application/json; charset=utf-8");

                using (var response = await SendAsync(msg))
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        var ex = new CrmServerException(responseText);
                        throw ex;
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
