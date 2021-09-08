namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetUserImage
    {
        #region Constructors

        public GetUserImage(string contactKey)
        {
            ContactKey = contactKey;
        }

        #endregion

        #region Properties

        public string ContactKey { get; set; }

        #endregion
    }
}
