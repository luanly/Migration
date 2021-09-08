
namespace SwissAcademic.Crm.Web.Query.FetchXml
{
    public partial class GetOrderProcess
    {
        #region Constructors

        public GetOrderProcess(string cleverbridgePurchaseId)
        {
            PurchaseId = cleverbridgePurchaseId;
        }

        #endregion

        #region Properties

        public string PurchaseId { get; set; }

        #endregion
    }
}
