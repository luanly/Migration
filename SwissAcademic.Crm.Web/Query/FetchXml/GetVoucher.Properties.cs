namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetVoucher
    {
        #region Constructors

        public GetVoucher(string voucherCode, int voucherStatus = 1)
        {
            VoucherCode = voucherCode;
            VoucherStatus = voucherStatus.ToString();
        }

        #endregion

        #region Properties

        public string VoucherCode { get; private set; }

        public string VoucherStatus { get; private set; }

        #endregion
    }
}
