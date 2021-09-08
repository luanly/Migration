using System.Security;

namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetContactByContactProperty
    {
        #region Constructors

        public GetContactByContactProperty(ContactPropertyId attribute, string value)
        {
            Attribute = EntityNameResolver.ResolveAttributeName(CrmEntityNames.Contact, attribute.ToString());
            Value = SecurityElement.Escape(value);
        }

        #endregion

        #region Properties

        public string Attribute { get; private set; }
        public string Value { get; private set; }

        #endregion
    }
}
