using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    [EntityLogicalName(CrmEntityNames.BulkMailQuery)]
    [DataContract]
    public class BulkMailQuery
        :
        CitaviCrmEntity
    {
        #region Konstanten

        internal const string ODataQueryPrefix = "odata:";
        const string LicensesByLicenseCodeQuery = "contacts?$select=new_key,new_language,emailaddress1&$filter=new_contact_new_citavilicense_enduser/all(o:o/new_licensetypid/new_licensecode eq '{0}' and o/statuscode eq 1) and statuscode eq 1";
        const string LicensesByPricingCodeQuery = "contacts?$select=new_key,new_language,emailaddress1&$filter=new_contact_new_citavilicense_enduser/all(o:o/new_pricingid/new_pricingcode eq '{0}' and o/statuscode eq 1) and statuscode eq 1";
        const string LicensesByProductCodeQuery = "contacts?$select=new_key,new_language,emailaddress1&$filter=new_contact_new_citavilicense_enduser/all(o:o/new_citaviproductid/new_citaviproductcode eq '{0}' and o/statuscode eq 1) and statuscode eq 1";
        const string LicensesByCampusContractQuery = "contacts?$select=new_key,new_language,emailaddress1&$filter=new_contact_new_citavilicense_enduser/any(o:o/new_campuscontractid/new_key eq '{0}' and o/statuscode eq 1) and statuscode eq 1";
        const string LicensesByLicenseCodePricingCodeProductCodeQuery = "contacts?$select=new_key,new_language,emailaddress1&$filter=new_contact_new_citavilicense_enduser/all(o:o/new_licensetypid/new_licensecode eq '{0}' and o/new_citaviproductid/new_citaviproductcode eq '{1}' and o/new_pricingid/new_pricingcode eq '{2}' and o/statuscode eq 1) and statuscode eq 1";

        #endregion

        #region Konstruktor

        public BulkMailQuery()
            :
            base(CrmEntityNames.BulkMailQuery)
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

        #region FetchXml

        [CrmProperty]
        public string FetchXml
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

        static Type _properyEnumType = typeof(BulkMailQueryPropertyId);
        protected override Type PropertyEnumType
        {
            get
            {
                return _properyEnumType;
            }
        }

        #endregion

        #region QueryName

        [CrmProperty]
        public string QueryName
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

        #region Statische Methoden

        public static BulkMailQuery ByCampusContract(CrmDbContext context, CampusContract campusContract)
        {
            if (context == null)
            {
                throw new NotSupportedException("CrmDbContext must not be null");
            }
            if (campusContract == null)
            {
                throw new NotSupportedException("CampusContract must not be null");
            }
            var query = context.Create<BulkMailQuery>();
            query.FetchXml = ODataQueryPrefix + string.Format(CultureInfo.InvariantCulture, LicensesByCampusContractQuery, campusContract.Key);
            return query;
        }

        public static BulkMailQuery ByLicenseType(CrmDbContext context, LicenseType licenseType)
        {
            if (context == null)
            {
                throw new NotSupportedException("CrmDbContext must not be null");
            }
            if (licenseType == null)
            {
                throw new NotSupportedException("LicenseType must not be null");
            }
            var query = context.Create<BulkMailQuery>();
            query.FetchXml = ODataQueryPrefix + string.Format(CultureInfo.InvariantCulture, LicensesByLicenseCodeQuery, licenseType.LicenseCode);
            return query;
        }

        public static BulkMailQuery ByPricing(CrmDbContext context, Pricing pricing)
        {
            if (context == null)
            {
                throw new NotSupportedException("CrmDbContext must not be null");
            }
            if (pricing == null)
            {
                throw new NotSupportedException("Pricing must not be null");
            }
            var query = context.Create<BulkMailQuery>();
            query.FetchXml = ODataQueryPrefix + string.Format(CultureInfo.InvariantCulture, LicensesByPricingCodeQuery, pricing.PricingCode);
            return query;
        }

        public static BulkMailQuery ByProduct(CrmDbContext context, Product product)
        {
            if(context == null)
            {
                throw new NotSupportedException("CrmDbContext must not be null");
            }
            if (product == null)
            {
                throw new NotSupportedException("Product must not be null");
            }
            var query = context.Create<BulkMailQuery>();
            query.FetchXml = ODataQueryPrefix + string.Format(CultureInfo.InvariantCulture, LicensesByProductCodeQuery, product.CitaviProductCode);
            return query;
        }

        public static BulkMailQuery ByLicenseTypeProductPricing(CrmDbContext context, LicenseType licenseType, Product product, Pricing pricing)
        {
            if (context == null)
            {
                throw new NotSupportedException("CrmDbContext must not be null");
            }
            if (product == null)
            {
                throw new NotSupportedException("Product must not be null");
            }
            if (licenseType == null)
            {
                throw new NotSupportedException("LicenseType must not be null");
            }
            if (pricing == null)
            {
                throw new NotSupportedException("Pricing must not be null");
            }
            var query = context.Create<BulkMailQuery>();
            query.FetchXml = ODataQueryPrefix + string.Format(CultureInfo.InvariantCulture, LicensesByLicenseCodePricingCodeProductCodeQuery, licenseType.LicenseCode, product.CitaviProductCode, pricing.PricingCode);
            return query;
        }

        #endregion
    }
}
