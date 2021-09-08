using Sustainsys.Saml2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Security;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SwissAcademic.Crm.Web
{
    internal static class ShibbolethServiceProvider
    {
        internal static void Configure(SPOptions spOptions)
        {
            #region ServiceCertificates

            //Erneuerung von Zertifikat:
            //https://github.com/Sustainsys/Saml2/issues/355
            //https://doku.tid.dfn.de/de:certificates

            //Vorgehen:
            //Neues Zertifikat erstellen und als shib_future verwenden
            //Das neue Zertifkate bei allen Föderationen zusätzlich hochladen
            //Nach 24 Stunden das alte Zertifikat in den Föderationen löschen
            //Später dann shib_current durch shib_future ersetzen und altes Zertifikat löschen

            var shib_current = SecurityUtility.FindCertificate(KnownCertificate.Shibboleth2);
            X509Certificate2 shib_future = null;

            var cert = new ServiceCertificate
            {
                Certificate = shib_current,
                Use = CertificateUse.Both,
                Status = CertificateStatus.Current
            };
            spOptions.ServiceCertificates.Add(cert);

            if (shib_future != null)
            {
                Telemetry.TrackTrace($"Second shibboleth certificate available. Rollout - Phase. {shib_future.Thumbprint}", SeverityLevel.Warning);
                //Neues Zertifikat. Da Shib1-Zertifikat noch vorhanden ist, befinden wir uns in der Phase wo beide Zertifkate noch aktiv sind.
                cert = new ServiceCertificate
                {
                    Certificate = shib_future,
                    Use = CertificateUse.Both,
                    Status = CertificateStatus.Future
                };
                spOptions.ServiceCertificates.Add(cert);
            }

            #endregion

            #region Contacts / Organization

            var contact = new ContactPerson
            {
                Type = ContactType.Technical,
                GivenName = "Marc",
                Surname = "Eichenberger"
            };
            contact.EmailAddresses.Add("support@citavi.com");
            spOptions.Contacts.Add(contact);
            contact = new ContactPerson
            {
                Type = ContactType.Support,
                GivenName = "Peter",
                Surname = "Meurer"
            };
            contact.EmailAddresses.Add("support@citavi.com");
            spOptions.Contacts.Add(contact);

            contact = new ContactPerson
            {
                Type = ContactType.Administrative,
                GivenName = "Hans Siem",
                Surname = "Schweiger"
            };
            contact.EmailAddresses.Add("support@citavi.com");
            spOptions.Contacts.Add(contact);

            spOptions.Organization = new Organization();
            spOptions.Organization.Names.Add(new LocalizedName("Swiss Academic Software", "en"));
            spOptions.Organization.DisplayNames.Add(new LocalizedName("Swiss Academic Software", "en"));
            spOptions.Organization.Urls.Add(new LocalizedUri(new Uri("https://www.citavi.com"), "en"));

            #endregion

            #region AttributeConsumingService

            var name = Environment.Build == BuildType.Release ? "Citavi Account" : $"Citavi Account ({Environment.Build.ToString()})";

            var attributeConsumingService = new AttributeConsumingService()
            {
                IsDefault = true,

            };
            attributeConsumingService.RequestedAttributes.Add(new RequestedAttribute(SAML2ClaimTypes.PersonScopedAffiliation)
            {
                FriendlyName = "eduPersonScopedAffiliation",
                IsRequired = false,
                NameFormat = RequestedAttribute.AttributeNameFormatUri
            });
            attributeConsumingService.RequestedAttributes.Add(new RequestedAttribute(SAML2ClaimTypes.PersonTargetedID)
            {
                FriendlyName = "eduPersonTargetedID",
                IsRequired = false,
                NameFormat = RequestedAttribute.AttributeNameFormatUri
            });
            attributeConsumingService.RequestedAttributes.Add(new RequestedAttribute(SAML2ClaimTypes.GivenName)
            {
                FriendlyName = "givenName",
                IsRequired = false,
                NameFormat = RequestedAttribute.AttributeNameFormatUri
            });
            attributeConsumingService.RequestedAttributes.Add(new RequestedAttribute(SAML2ClaimTypes.Surname)
            {
                FriendlyName = "sn",
                IsRequired = false,
                NameFormat = RequestedAttribute.AttributeNameFormatUri
            });
            attributeConsumingService.RequestedAttributes.Add(new RequestedAttribute(SAML2ClaimTypes.Email)
            {
                FriendlyName = "mail",
                IsRequired = false,
                NameFormat = RequestedAttribute.AttributeNameFormatUri
            });
            attributeConsumingService.RequestedAttributes.Add(new RequestedAttribute(SAML2ClaimTypes.PersonUniqueId)
            {
                FriendlyName = "eduPersonUniqueId",
                IsRequired = false,
                NameFormat = RequestedAttribute.AttributeNameFormatUri
            });
            attributeConsumingService.RequestedAttributes.Add(new RequestedAttribute(SAML2ClaimTypes.PersonAffiliation)
            {
                FriendlyName = "eduPersonAffiliation",
                IsRequired = false,
                NameFormat = RequestedAttribute.AttributeNameFormatUri
            });
            attributeConsumingService.RequestedAttributes.Add(new RequestedAttribute(SAML2ClaimTypes.PersonalUniqueCode)
            {
                FriendlyName = "schacPersonalUniqueCode",
                IsRequired = false,
                NameFormat = RequestedAttribute.AttributeNameFormatUri
            });

            spOptions.AttributeConsumingServices.Add(attributeConsumingService);

            #endregion

            //Rückwärtskompatibel: Können wir irgendwann mal ändern -> Einträge in den Federationen anpassen
            spOptions.ModulePath = "identity/AuthServices";
            spOptions.Saml2PSecurityTokenHandler = new CrmSaml2SecurityTokenHandler();

            CryptoConfig.AddAlgorithm(typeof(AesGcmAlgorithm), AesGcmAlgorithm.AesGcm128Identifier);
        }
    }
}
