using System;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Workflow)]
    public class Workflow
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public Workflow()
            :
            base(CrmEntityNames.Workflow)
        {

        }

        #endregion

        #region Eigenschaften

        #region Name

        [CrmProperty(IsBuiltInAttribute = true)]
        public string Name
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

        #region PropertyEnumType

        static Type _properyEnumType = typeof(WorkflowPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region PrimaryEntity

        [CrmProperty(IsBuiltInAttribute = true)]
        public string PrimaryEntity
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

        #region Status

        public WorkflowStateCode WorkflowStatus
        {
            get
            {
                return (WorkflowStateCode)GetValue<int>(nameof(StateCode));
            }
        }

        #endregion

        #region StatusReason

        public WorkflowStatusCode WorkflowStatusReason
        {
            get
            {
                return (WorkflowStatusCode)GetValue<int>(nameof(StatusCode));
            }
        }

        #endregion

        #region Type

        [CrmProperty(IsBuiltInAttribute = true)]
        public WorkflowType Type
        {
            get
            {
                return GetValue<WorkflowType>();
            }
            set
            {
                SetValue(value);
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region ToString

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }

            return base.ToString();
        }

        #endregion

        #endregion
    }
}
