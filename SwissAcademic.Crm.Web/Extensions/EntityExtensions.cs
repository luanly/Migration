using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public static class EntityExtensions
    {
        public static IEnumerable<CitaviLicense> GetByProduct(this IEnumerable<CitaviLicense> license, Product product)
        {
            return license.Where(lic => product.Key == lic.DataContractProductKey);
        }

        public static bool HasCampusContractLicense(this IEnumerable<CitaviLicense> license, CampusContract campusContract, Product product)
        {
            if (product == null)
            {
                return license.Any(lic => campusContract.Key == lic.DataContractCampusContractKey);
            }

            return license.Any(lic => campusContract.Key == lic.DataContractCampusContractKey &&
                                      product.Key == lic.DataContractProductKey);
        }

        public static CitaviLicense GetCampusContractLicense(this IEnumerable<CitaviLicense> license, CampusContract campusContract, Product product)
        {
            return license.FirstOrDefault(lic => campusContract.Key == lic.DataContractCampusContractKey &&
                                                 product.Key == lic.DataContractProductKey);
        }

        public static IEnumerable<CitaviLicense> GetCampusContractLicenses(this IEnumerable<CitaviLicense> license, CampusContract campusContract)
        {
            return license.Where(lic => campusContract.Key == lic.DataContractCampusContractKey);
        }

        public static bool HasOwnerUserLicense(this CrmUser user, string productKey)
        {
            return user.Licenses.Any(lic => lic.DataContractOwnerContactKey == user.Key &&
                                            productKey == lic.DataContractProductKey);
        }

        public static bool HasEndUserLicense(this CrmUser user, Product product)
        {
            return user.Licenses.Any(lic => lic.DataContractEndUserContactKey == user.Key &&
                                            product.Key == lic.DataContractProductKey);
        }
        public static bool HasEndUserLicense(this CrmUser user, string productKey)
        {
            return user.Licenses.Any(lic => lic.DataContractEndUserContactKey == user.Key &&
                                            productKey == lic.DataContractProductKey);
        }

        public static bool HasCitavi6EndUserLicense(this CrmUser user)
        {
            return user.Licenses.Any(lic => lic.DataContractEndUserContactKey == user.Key && lic.CitaviMajorVersion == 6);
        }

    }
}
