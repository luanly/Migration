using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Cleverbridge
{
    public enum UpdateSubscriptionItemAction
    {
        Update,
        Upgrade,
        Downgrade
    }

    public enum RenewSubscriptionTransactionStatus
    {
        Success,
        Error,
        Rejected,
        Pending,
        Unknown
    }
}
