using System;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.AutoNumberSequence)]
    public class AutoNumberSequence
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public AutoNumberSequence()
            :
            base(CrmEntityNames.AutoNumberSequence)
        {

        }

        #endregion

        #region Eigenschaften

        #region EntityName

        [CrmProperty]
        public string EntityName
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

        #region Format

        [CrmProperty]
        public string Format
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

        #region LastNumber

        [CrmProperty]
        public int LastNumber
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

        #region PropertyEnumType

        static Type _properyEnumType = typeof(AutoNumberSequencePropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #endregion
    }
}
