using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace SwissAcademic.Crm.Web
{
    public class FetchXmlExpression
        :
        ExpressionBase
    {
        #region Felder

        int _page = 1;
        public const int DefaultPageSize = 1000;

        public const string FetchXml_ManyToMany_Count = @"<fetch distinct='false' mapping='logical' aggregate='true'> 
										<entity name='{0}'> 
										   <attribute name='{0}id' alias='count' aggregate='count' /> 
										   <link-entity name='{2}' to='{0}id' from='{0}id'>
												<filter type='and'>
												  <condition attribute='{0}id' operator='eq' value='{1}' />
												</filter>
							                </link-entity>
										</entity> 
										</fetch>";

        public const string FetchXml_OneToMany_Count = @"<fetch distinct='false' mapping='logical' aggregate='true'> 
										<entity name='{0}'> 
										   <attribute name='{0}id' alias='count' aggregate='count' /> 
											<filter type='and'>
												<condition attribute='{2}' operator='eq' value='{1}' />
											</filter>
										</entity> 
										</fetch>";


        public const string FetchXml_Count = @"<fetch distinct='false' mapping='logical' aggregate='true'> 
												<entity name='{0}'> 
												   <attribute name='{0}id' alias='count' aggregate='count' /> 
												</entity> 
												</fetch>";

        public const string FetchXml_Count_StatusCode = @"<fetch distinct='false' mapping='logical' aggregate='true'> 
										<entity name='{0}'> 
										   <attribute name='{0}id' alias='count' aggregate='count' /> 
                                           <filter type='and'>
												  <condition attribute='statuscode' operator='eq' value='{1}' />
										   </filter>
										</entity> 
										</fetch>";

        #endregion

        #region Konstruktor

        public FetchXmlExpression(string entityName, string xml)
        {
            EntityName = entityName;
            xml = Regex.Replace(xml, "\r\n", "");
            Xml = Regex.Replace(xml, "\\s+", " ");
        }

        #endregion

        #region Eigenschaften

        internal List<object> Bag = new List<object>();
        public override string EntityName { get; }
        public string Xml { get; }

        #endregion

        #region Methoden

        public override void Reset()
        {
            _page = 1;
            base.Reset();
        }

		public override string ToOData()
        {
            if (!string.IsNullOrEmpty(NextLink))
            {
                ///Write your FetchXml Query so that the primary entity ids are unique
                ///If your query includes only one link-entity and doesn't include many-to-many relationships, you can usually make the related entity the primary entity in your query to resolve this.
                ///Rather than include the paging cookie in your FetchXml, simply update the page value. This will work but there will be some performance impact.
                ///-> Bei FetchXML mit Related Entities MUSS die Primäre Enität das ID Attribute enthalten. Sonst ist es Voddoo
                //https://github.com/MicrosoftDocs/dynamics-365-customer-engagement/blob/323d69c97895ced16ab72b06fe5614e098901698/ce/developer/org-service/page-large-result-sets-with-fetchxml.md#when-not-to-use-paging-cookies

                _page++;
                var pageCookie = NextLink.ToString().Replace("+", " ");
                pageCookie = Uri.UnescapeDataString(Uri.UnescapeDataString(pageCookie));
                pageCookie = Regex.Match(pageCookie, "pagingcookie=\"(?<PC>.+?</cookie>)\"").Groups["PC"].Value;
                pageCookie = pageCookie.Replace("<", "&lt;");
                pageCookie = pageCookie.Replace("\"", "&quot;");
                pageCookie = pageCookie.Replace(">", "&gt;");
                if (!string.IsNullOrEmpty(pageCookie))
                {
                    return WebUtility.UrlEncode(Xml.Replace("<fetch ", $"<fetch paging-cookie=\"{pageCookie}\" page=\"{_page}\" count=\"{PageSize.Value.ToString()}\" "));
                }
                return WebUtility.UrlEncode(Xml.Replace("<fetch ", $"<fetch page=\"{_page}\" count=\"{PageSize.Value.ToString()}\" "));
            }
            if (PageSize.HasValue)
            {
                return WebUtility.UrlEncode(Xml.Replace("<fetch ", $"<fetch count=\"{PageSize.Value.ToString()}\" "));
            }
            return WebUtility.UrlEncode(Xml);
        }

        #endregion

        #region Statische Methoden

        public static FetchXmlExpression Create<T>(string xml)
            where T : CitaviCrmEntity
         => new FetchXmlExpression(EntityNameResolver.GetEntityLogicalName<T>(), xml);


        #endregion
    }
}
