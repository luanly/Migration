using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Subscription)]
    public class Subscription
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public Subscription()
            :
            base(CrmEntityNames.Subscription)
        {

        }

        #endregion

        #region Eigenschaften

        #region AllowReorder

        [Obsolete]
        [CrmProperty]
        [DataMember]
        public bool AllowReorder
        {
            get
            {
                return GetValue<bool>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ChangePaymentSubscriptionUrl

        [Obsolete]
        [CrmProperty]
        [DataMember]
        public string ChangePaymentSubscriptionUrl
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CancellationUrl

        [Obsolete]
        [CrmProperty]
        [DataMember]
        public string CancellationUrl
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region CbSubscriptionId

        [CrmProperty]
        [DataMember]
        public string CbSubscriptionId
        {
            get
            {
                return GetValue<string>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region OrderProcesses

        ManyToManyRelationship<Subscription, OrderProcess> _orderProcesses;
        public ManyToManyRelationship<Subscription, OrderProcess> OrderProcesses
        {
            get
            {
                if (_orderProcesses == null)
                {
                    _orderProcesses = new ManyToManyRelationship<Subscription, OrderProcess>(this, CrmRelationshipNames.SubscriptionOrderProcesses);
                    Observe(_orderProcesses, true);
                }
                return _orderProcesses;
            }
        }

        #endregion

        #region Owner

        ManyToOneRelationship<Subscription, Contact> _owner;
        /// <summary>
        /// Besitzer der Subscription. Nur dieser kann Subscription anpassen
        /// </summary>
        public ManyToOneRelationship<Subscription, Contact> Owner
        {
            get
            {
                if (_owner == null)
                {
                    _owner = new ManyToOneRelationship<Subscription, Contact>(this, CrmRelationshipNames.SubscriptionContact, "new_contactid");
                    Observe(_owner, true);
                }
                return _owner;
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(SubscriptionPropertyId);
        protected override Type PropertyEnumType => _properyEnumType;

        #endregion

        #region SubscriptionItems

        OneToManyRelationship<Subscription, SubscriptionItem> _subscriptionItems;
        public OneToManyRelationship<Subscription, SubscriptionItem> SubscriptionItems
        {
            get
            {
                if (_subscriptionItems == null)
                {
                    _subscriptionItems = new OneToManyRelationship<Subscription, SubscriptionItem>(this, CrmRelationshipNames.SubscriptionSubscriptionItem, "new_subscriptionid");
                    Observe(_subscriptionItems, true);
                }
                return _subscriptionItems;
            }
        }

        #endregion

        #endregion
    }
}
