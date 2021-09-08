using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.SubscriptionItem)]
    public class SubscriptionItem
        :
        CitaviCrmEntity
    {
        #region Ereignisse

        #region OnRelationshipChanged

        protected override void OnRelationshipChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (e.NewItems.Count == 1)
                        {
                            var newItem = e.NewItems[0];
                            if (newItem is Subscription)
                            {
                                _dataContractSubscription = newItem as Subscription;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

        #endregion

        #region Konstruktor

        public SubscriptionItem()
            :
            base(CrmEntityNames.SubscriptionItem)
        {

        }

        #endregion

        #region Eigenschaften

        #region CbSubscriptionItemId

        [CrmProperty]
        public string CbSubscriptionItemId
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

        #region CleverbridgeProduct

        ManyToOneRelationship<SubscriptionItem, CleverbridgeProduct> _cleverbridgeProduct;
        [Obsolete]
        public ManyToOneRelationship<SubscriptionItem, CleverbridgeProduct> CleverbridgeProduct
        {
            get
            {
                if (_cleverbridgeProduct == null)
                {
                    _cleverbridgeProduct = new ManyToOneRelationship<SubscriptionItem, CleverbridgeProduct>(this, CrmRelationshipNames.SubscriptionItemCleverbridgeProduct, "new_cleverbridgeproductid");
                    Observe(_cleverbridgeProduct, true);
                }
                return _cleverbridgeProduct;
            }
        }

        #endregion

        #region DisplayName

        [Obsolete]
        [CrmProperty]
        [DataMember]
        public string DisplayName
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

        #region ItemStatus

        [CrmProperty]
        [DataMember]
        public SubscriptionItemStatusType ItemStatus
        {
            get
            {
                return GetValue<SubscriptionItemStatusType>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region IntervalLengthInDays

        [Obsolete]
        [CrmProperty]
        [DataMember]
        public int? IntervalLengthInDays
        {
            get
            {
                return GetValue<int?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region IntervalLengthInMonths

        [Obsolete]
        [CrmProperty]
        [DataMember]
        public int? IntervalLengthInMonths
        {
            get
            {
                return GetValue<int?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region LicenseAmount

        [CrmProperty]
        [DataMember]
        public int LicenseAmount
        {
            get
            {
                return GetValue<int>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Licenses

        OneToManyRelationship<SubscriptionItem, CitaviLicense> _licenses;
        public OneToManyRelationship<SubscriptionItem, CitaviLicense> Licenses
        {
            get
            {
                if (_licenses == null)
                {
                    _licenses = new OneToManyRelationship<SubscriptionItem, CitaviLicense>(this, CrmRelationshipNames.SubscriptionItemLicenses, "new_subscriptionitemid");
                    Observe(_licenses, true);
                }
                return _licenses;
            }
        }

        #endregion

        #region NextBillingDate

        [CrmProperty]
        [DataMember]
        public DateTime NextBillingDate
        {
            get
            {
                return GetValue<DateTime>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region PropertyEnumType

        static Type _properyEnumType = typeof(SubscriptionItemPropertyId);
        protected override Type PropertyEnumType => _properyEnumType;

        #endregion

        #region RenewalType

        [Obsolete]
        [CrmProperty]
        public SubscriptionRenewalType RenewalType
        {
            get
            {
                return GetValue<SubscriptionRenewalType>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Subscription

        ManyToOneRelationship<SubscriptionItem, Subscription> _subscription;
        public ManyToOneRelationship<SubscriptionItem, Subscription> Subscription
        {
            get
            {
                if (_subscription == null)
                {
                    _subscription = new ManyToOneRelationship<SubscriptionItem, Subscription>(this, CrmRelationshipNames.SubscriptionSubscriptionItem, "new_subscriptionid");
                    Observe(_subscription, true);
                }
                return _subscription;
            }
        }

        #endregion

        #endregion

        #region DataContract

        #region DataContractSubscriptionKey

        Subscription _dataContractSubscription;

        [DataMember(Name = "SubscriptionKey")]
        public string DataContractSubscriptionKey
        {
            get
            {
                if (_dataContractSubscription != null)
                {
                    return _dataContractSubscription.Key;
                }

                return GetAliasedValue<string>(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.Key);
            }
            set
            {
                if (_dataContractSubscription != null)
                {
                    _dataContractSubscription.Key = value;
                }
                SetAliasedValue(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.Key, value);
            }
        }

        #endregion

        #region DataContractSubscriptionName

        [DataMember(Name = "SubscriptionName")]
        public string DataContractSubscriptionName
        {
            get
            {
                if (_dataContractSubscription != null)
                {
                    return _dataContractSubscription.CbSubscriptionId;
                }

                return GetAliasedValue<string>(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.CbSubscriptionId);
            }
            set
            {
                if (_dataContractSubscription != null)
                {
                    _dataContractSubscription.CbSubscriptionId = value;
                }
                SetAliasedValue(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.CbSubscriptionId, value);
            }
        }

        #endregion

        #region DataContractChangePaymentSubscriptionUrl

        [DataMember(Name = "ChangePaymentSubscriptionUrl")]
        public string DataContractChangePaymentSubscriptionUrl
        {
            get
            {
                if (_dataContractSubscription != null)
                {
                    return _dataContractSubscription.ChangePaymentSubscriptionUrl;
                }

                return GetAliasedValue<string>(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.ChangePaymentSubscriptionUrl);
            }
            set
            {
                if (_dataContractSubscription != null)
                {
                    _dataContractSubscription.ChangePaymentSubscriptionUrl = value;
                }
                SetAliasedValue(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.ChangePaymentSubscriptionUrl, value);
            }
        }

        #endregion

        #region DataContractCancellationUrl

        [DataMember(Name = "CancellationUrl")]
        public string DataContractCancellationUrl
        {
            get
            {
                if (_dataContractSubscription != null)
                {
                    return _dataContractSubscription.CancellationUrl;
                }

                return GetAliasedValue<string>(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.CancellationUrl);
            }
            set
            {
                if (_dataContractSubscription != null)
                {
                    _dataContractSubscription.CancellationUrl = value;
                }
                SetAliasedValue(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.CancellationUrl, value);
            }
        }

        #endregion

        #region DataContractSubscriptionAllowReorder

        [DataMember(Name = "SubscriptionAllowReorder")]
        public bool DataContractSubscriptionAllowReorder
        {
            get
            {
                if (_dataContractSubscription != null)
                {
                    return _dataContractSubscription.AllowReorder;
                }

                return GetAliasedValue<bool>(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.AllowReorder);
            }
            set
            {
                if (_dataContractSubscription != null)
                {
                    _dataContractSubscription.AllowReorder = value;
                }
                SetAliasedValue(CrmRelationshipNames.SubscriptionSubscriptionItem, CrmEntityNames.Subscription, SubscriptionPropertyId.AllowReorder, value);
            }
        }

        #endregion

        #region DataContractCleverbridgeProductKey

        [DataMember(Name = "CleverbridgeProductKey")]
        public string DataContractCleverbridgeProductKey
        {
            get
            {
                return GetAliasedValue<string>(CrmRelationshipNames.SubscriptionItemCleverbridgeProduct, CrmEntityNames.CleverbridgeProduct, CleverbridgeProductPropertyId.Key);
            }
            set
            {
                SetAliasedValue(CrmRelationshipNames.SubscriptionItemCleverbridgeProduct, CrmEntityNames.CleverbridgeProduct, CleverbridgeProductPropertyId.Key, value);
            }
        }

        #endregion

        #region DataContractCleverbridgeProductName

        [DataMember(Name = "CleverbridgeProductName")]
        public string DataContractCleverbridgeProductName
        {
            get
            {
                var productKey = DataContractCleverbridgeProductKey;
                if (string.IsNullOrEmpty(productKey))
                {
                    return string.Empty;
                }
                return CrmCache.CleverbridgeProductsByKey[productKey].Name;
            }
        }

        #endregion

        #endregion
    }
}
