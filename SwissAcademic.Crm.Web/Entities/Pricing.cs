using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.Pricing)]
    [DataContract]
    public class Pricing
        :
        CitaviCrmEntity
    {
        #region Konstruktor

        public Pricing()
            :
            base(CrmEntityNames.Pricing)
        {

        }

        #endregion

        #region Eigenschaften

        #region PricingCode

        [CrmProperty]
        public string PricingCode
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

        #region PricingName

        [CrmProperty]
        [DataMember]
        public string PricingName
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

        static Type _properyEnumType = typeof(PricingPropertyId);
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

        #region IsBusinessOrAcacemicPricing

        internal bool IsBusinessOrAcacemicPricing()
        {
            return Pricing.IsBusinessOrAcacemicPricing(this);
        }

        #endregion

        #region IsPersonalPricing

        internal bool IsPersonalPricing()
        {
            return Pricing.IsPersonalPricing(this);
        }

        #endregion

        #region IsStudentPricing

        internal bool IsStudentPricing()
        {
            return Pricing.IsStudentPricing(this);
        }

        #endregion

        #region ToString

        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(PricingName))
            {
                return PricingName;
            }

            return base.ToString();
        }

        #endregion

        #endregion

        #region Statische Methoden

        public static bool IsBusinessOrAcacemicPricing(Pricing pricing)
        {
            return IsBusinessOrAcacemicPricing(pricing.Key);
        }
        public static bool IsBusinessOrAcacemicPricing(string pricingKey)
        {
            return pricingKey == Standard.Key ||
                   pricingKey == AcademicNonprofit.Key;
        }

        public static bool IsPersonalPricing(Pricing pricing)
        {
            return IsPersonalPricing(pricing.Key);
        }
        public static bool IsPersonalPricing(string pricingKey)
        {
            return pricingKey == Personal.Key || pricingKey == Benefit.Key;
        }

        public static bool IsStudentPricing(Pricing pricing)
        {
            return IsStudentPricing(pricing.Key);
        }
        public static bool IsStudentPricing(string pricingKey)
        {
            return pricingKey == Student.Key;
        }

        #endregion

        #region Statische Eigenschaften

        #region Standard

        public static Pricing Standard => CrmCache.PricingsByCode["sta"];

        #endregion

        #region None
        public static Pricing None => CrmCache.PricingsByCode["non"];

        #endregion

        #region Benefit

        public static Pricing Benefit => CrmCache.PricingsByCode["ben"];

        #endregion

        #region Personal

        public static Pricing Personal => CrmCache.PricingsByCode["per"];

        #endregion

        #region Academic & Nonprofit

        public static Pricing AcademicNonprofit => CrmCache.PricingsByCode["aca"];

        #endregion

        #region Schulversion

        public static Pricing School => CrmCache.PricingsByCode["sch"];

        #endregion

        #region Student

        public static Pricing Student => CrmCache.PricingsByCode["stu"];

        #endregion

        #endregion
    }
}
