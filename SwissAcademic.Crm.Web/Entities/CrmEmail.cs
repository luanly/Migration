using System;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Email)]
    [DataContract]
    public class CrmEmail
        :
        CitaviCrmEntity
    {
        public CrmEmail()
            :
            base(CrmEntityNames.Email)
        {

        }

        [CrmProperty(IsBuiltInAttribute = true)]
        public Guid ActivityId
        {
            get
            {
                return GetValue<Guid>();
            }
            set
            {
                SetValue(value);
            }
        }

        public string PlainText { get; set; }


        [CrmProperty(IsBuiltInAttribute = true)]
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


        [CrmProperty(IsBuiltInAttribute = true)]
        public string Subject
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
    }
}
