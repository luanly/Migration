using System;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    /// <summary>
    /// Mailversand
    /// </summary>
    [EntityLogicalName(CrmEntityNames.Delivery)]
    [DataContract]
    public class Delivery
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public Delivery()
            :
            base(CrmEntityNames.Delivery)
        {

        }

        #endregion

        #region Eigenschaften

        #region Accounts

        ManyToManyRelationship<Delivery, Account> _accounts;
        public ManyToManyRelationship<Delivery, Account> Accounts
        {
            get
            {
                if (_accounts == null)
                {
                    _accounts = new ManyToManyRelationship<Delivery, Account>(this, CrmRelationshipNames.DeliveryAccount);
                    Observe(_accounts, true);
                }
                return _accounts;
            }
        }

        #endregion

        #region BounceMailCount

        [CrmProperty]
        public int BounceMailCount
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

        #region DeliveryName

        [CrmProperty]
        public string DeliveryName
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

        #region DeliveryState

        [CrmProperty]
        public DeliveryState? DeliveryState
        {
            get
            {
                return GetValue<DeliveryState?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Contacts

        ManyToManyRelationship<Delivery, Contact> _contacts;
        public ManyToManyRelationship<Delivery, Contact> Contacts
        {
            get
            {
                if (_contacts == null)
                {
                    _contacts = new ManyToManyRelationship<Delivery, Contact>(this, CrmRelationshipNames.DeliveryContact);
                    Observe(_contacts, true);
                }
                return _contacts;
            }
        }

        #endregion

        #region Description

        [CrmProperty]
        public string Description
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

        #region IsCampusNewsletter

        [CrmProperty]
        public bool IsCampusNewsletter
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

        #region PropertyEnumType

        static Type _properyEnumType = typeof(DeliveryPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region ProcessEnd

        [CrmProperty]
        public DateTime? ProcessEnd
        {
            get
            {
                return GetValue<DateTime?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region ProcessStart

        [CrmProperty]
        public DateTime? ProcessStart
        {
            get
            {
                return GetValue<DateTime?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #region Query

        ManyToOneRelationship<Delivery, BulkMailQuery> _query;
        public ManyToOneRelationship<Delivery, BulkMailQuery> Query
        {
            get
            {
                if (_query == null)
                {
                    _query = new ManyToOneRelationship<Delivery, BulkMailQuery>(this, CrmRelationshipNames.DeliveryBulkMailQuery, "new_bulkmailqueryid");
                    Observe(_query, true);
                }
                return _query;
            }
        }

        #endregion

        #region QueueItemId

        [CrmProperty]
        public string QueueItemId
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

        #region QueueItemPopReceipt

        [CrmProperty]
        public string QueueItemPopReceipt
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

        #region ReceiverCount

        [CrmProperty]
        public int ReceiverCount
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

        #region Template

        ManyToOneRelationship<Delivery, BulkMailTemplate> _template;
        public ManyToOneRelationship<Delivery, BulkMailTemplate> Template
        {
            get
            {
                if (_template == null)
                {
                    _template = new ManyToOneRelationship<Delivery, BulkMailTemplate>(this, CrmRelationshipNames.DeliveryBulkMailTemplate, "new_bulkmailtemplateid");
                    Observe(_template, true);
                }
                return _template;
            }
        }

        #endregion

        #region SendDate

        [CrmProperty]
        public DateTime? SendDate
        {
            get
            {
                return GetValue<DateTime?>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #endregion
    }
}
