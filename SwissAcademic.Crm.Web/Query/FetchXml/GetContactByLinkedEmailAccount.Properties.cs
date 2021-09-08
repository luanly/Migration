using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetContactByLinkedEmailAccount
    {
        #region Constructors

        public GetContactByLinkedEmailAccount(LinkedEmailAccountPropertyId attribute, string value)
        {
            Attribute = EntityNameResolver.ResolveAttributeName(CrmEntityNames.LinkedEmailAccount, attribute.ToString());
            Value = SecurityElement.Escape(value);
        }

        #endregion

        #region Properties

        public string Attribute { get; private set; }
        public string Value { get; private set; }

        #endregion
    }
}
