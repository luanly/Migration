using System;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.TransactionCurrency)]
    public class TransactionCurrency
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public TransactionCurrency()
            :
            base(CrmEntityNames.TransactionCurrency)
        {

        }

        #endregion

        #region Eigenschaften

        #region CurrencyName

        [CrmProperty(IsBuiltInAttribute = true)]
        public string CurrencyName
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

        static Type _properyEnumType = typeof(TransactionCurrencyPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region ToString

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(CurrencyName))
            {
                return CurrencyName;
            }

            return base.ToString();
        }

        #endregion

        #endregion
    }
}
