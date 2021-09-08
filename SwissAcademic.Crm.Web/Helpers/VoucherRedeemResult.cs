using System;

namespace SwissAcademic.Crm.Web
{
    public class VoucherRedeemResult
    {
        public DateTime? ExpiryDate { get; set; }
        public string LicenseKey { get; set; }
        public VoucherRedeemResultType Status { get; set; }
    }
}
