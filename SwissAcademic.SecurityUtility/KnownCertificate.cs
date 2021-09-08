using System.Configuration;
using static SwissAcademic.Security.SecurityUtility;

namespace SwissAcademic.Security
{
    public class KnownCertificate
    {
    #region Constructors

        private KnownCertificate(string thumbprint)
        {
            Thumbprint = thumbprint;
        }

    #endregion

    #region Properties

    #region Thumbprint

        public string Thumbprint { get; }

    #endregion

    #endregion

    #region Static Instances

        /// <summary>
        /// Returns a certificate depending on <see cref="ApplicationSettings.BuildType"/>
        /// For BuildType.Alpha, returns the certificate CitaviWebAlpha.
        /// Otherwise, returns the certificate CitaviWebProductive.
        /// </summary>
        public static readonly KnownCertificate CitaviWeb = ConfigurationManager.AppSettings["EnvironmentName"] == "Prod" ?
            new KnownCertificate("BC801AB7048926D243D9268252CEAA6A7D4421EF") :
            new KnownCertificate("77DE5665CE16B4EE349F91E53316BF00F1E011FD");

        /// <summary>
        /// Returns the certificate with the thumbprint BC801AB7048926D243D9268252CEAA6A7D4421EF.
        /// </summary>
        public static readonly KnownCertificate CitaviWebProductive = new KnownCertificate("BC801AB7048926D243D9268252CEAA6A7D4421EF");

        /// <summary>
        /// Returns the certificate with the thumbprint 77DE5665CE16B4EE349F91E53316BF00F1E011FD.
        /// </summary>
        public static readonly KnownCertificate CitaviWebAlpha = new KnownCertificate("77DE5665CE16B4EE349F91E53316BF00F1E011FD");

        /// <summary>
        /// Returns the certificate with the thumbprint AFB8911BB395257F43C06F335F93D116598E785B.
        /// </summary>
        public static readonly KnownCertificate BackOffice = new KnownCertificate("AFB8911BB395257F43C06F335F93D116598E785B");

        /// <summary>
        /// Returns the certificate with the thumbprint 320327b22d7e64ea34f4b0d6389f0c7e027b37eb.
        /// </summary>
        public static readonly KnownCertificate DbServerLicense = new KnownCertificate("320327b22d7e64ea34f4b0d6389f0c7e027b37eb");

        /// <summary>
        /// Returns the certificate with the thumbprint 629BAAB5C8184A5B5D17A84D4BFEACF5484D7FA2.
        /// </summary>
        public static readonly KnownCertificate Shibboleth = new KnownCertificate("629BAAB5C8184A5B5D17A84D4BFEACF5484D7FA2");

        /// <summary>
        /// Returns the certificate with the thumbprint 629BAAB5C8184A5B5D17A84D4BFEACF5484D7FA2.
        /// </summary>
        public static readonly KnownCertificate Shibboleth2 = new KnownCertificate("26e7c6a5332befda83966f8d7bc292d57ee40f61");

    #endregion
    }
}
