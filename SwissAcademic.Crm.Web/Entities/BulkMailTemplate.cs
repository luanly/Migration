using System;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.BulkMailTemplate)]
    [DataContract]
    public class BulkMailTemplate
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public BulkMailTemplate()
            :
            base(CrmEntityNames.BulkMailTemplate)
        {

        }

        #endregion

        #region Eigenschaften

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

        #region MailTextDE

        [CrmProperty]
        public string MailTextDE
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

        #region MailTextEN

        [CrmProperty]
        public string MailTextEN
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

        static Type _properyEnumType = typeof(BulkMailTemplatePropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region SubjectDE

        [CrmProperty]
        public string SubjectDE
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

        #region SubjectEN

        [CrmProperty]
        public string SubjectEN
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

        #region TemplateName

        [CrmProperty]
        public string TemplateName
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

        #endregion
    }
}
