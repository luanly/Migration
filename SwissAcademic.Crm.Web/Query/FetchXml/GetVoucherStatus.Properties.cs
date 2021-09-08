namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetVoucherStatus
    {
        #region Constructors

        public GetVoucherStatus(string voucherCode)
        {
            VoucherCode = voucherCode;
        }

        #endregion

        #region Properties

        public string VoucherCode { get; set; }

        #endregion
    }
}
