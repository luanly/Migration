using System;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.ExternalAccount)]
    public class ExternalAccount
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public ExternalAccount()
            :
            base(CrmEntityNames.ExternalAccount)
        {

        }

        #endregion

        #region Eigenschaften

        #region ADAccess

        /// <summary>
        /// Für IT-Admins sichtbar
        /// </summary>
        [CrmProperty]
        public bool ADAccess
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

        #region GLAccess

        /// <summary>
        /// Für GL sichtbar
        /// </summary>
        [CrmProperty]
        public bool GLAccess
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

        #region Name

        [CrmProperty]
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

        static Type _properyEnumType = typeof(ExternalAccountPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region SAAccess

        /// <summary>
        /// Für Verkauf & Support sichbar
        /// </summary>
        [CrmProperty]
        public bool SAAccess
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
