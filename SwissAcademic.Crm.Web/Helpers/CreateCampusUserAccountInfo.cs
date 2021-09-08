
namespace SwissAcademic.Crm.Web
{
    public class CreateCampusUserAccountInfo
    {
        public string Email { get; set; }

        public string IP { get; set; }
        public bool IPRangeCheck { get; set; }

        public string VoucherCode { get; set; }
        public bool VoucherCheck { get; set; }
    }
}
