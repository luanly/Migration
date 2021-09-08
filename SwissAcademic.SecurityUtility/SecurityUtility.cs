using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace SwissAcademic.Security
{
    public static class SecurityUtility
    {
        #region Sign Xml Document

        public static void Sign(this XmlDocument xmlDocument, KnownCertificate certificate)
        {
            Sign(xmlDocument, certificate.Thumbprint);
        }

        public static void Sign(this XmlDocument xmlDocument, string thumbprint)
        {
            var provider = GetCryptoServiceProvider(thumbprint, false);
            Sign(xmlDocument, provider);
        }

        public static void Sign(this XmlDocument xmlDocument, RSA key)
        {
            if (xmlDocument == null) throw new ArgumentException("xmlDocument");
            if (key == null) throw new ArgumentException("key");

            var signedXml = new SignedXml(xmlDocument);
            signedXml.SigningKey = key;

            var reference = new Reference();
            reference.Uri = "";

            var env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);
            signedXml.AddReference(reference);

            signedXml.ComputeSignature();

            var xmlDigitalSignature = signedXml.GetXml();

            xmlDocument.DocumentElement.AppendChild(xmlDocument.ImportNode(xmlDigitalSignature, true));
        }

        #endregion

        #region Check Xml Document Signature

        public static bool CheckSignature(this XmlDocument xmlDocument, KnownCertificate certificate)
        {
            return CheckSignature(xmlDocument, certificate.Thumbprint);
        }

        public static bool CheckSignature(this XmlDocument xmlDocument, string thumbprint)
        {
            var provider = GetCryptoServiceProvider(thumbprint, true);
            return CheckSignature(xmlDocument, provider);
        }

        public static bool CheckSignature(this XmlDocument xmlDocument, RSA key)
        {
            if (xmlDocument == null) throw new ArgumentException("xmlDocument");
            if (key == null) throw new ArgumentException("key");

            var signedXml = new SignedXml(xmlDocument);
            var nodeList = xmlDocument.GetElementsByTagName("Signature");

            if (nodeList.Count <= 0)
                throw new CryptographicException("Verification failed: No Signature was found in the document.");
            if (nodeList.Count >= 2)
                throw new CryptographicException("Verification failed: More that one signature was found for the document.");

            signedXml.LoadXml((XmlElement)nodeList[0]);
            return signedXml.CheckSignature(key);
        }

        #endregion

        #region DecryptSymmetric

        public static string DecryptSymmetric(this string encryptedValue, string password)
        {
            return AESThenHMAC.SimpleDecryptWithPassword(encryptedValue, password);
        }

        #endregion

        #region DecryptWithCertificate

        public static string DecryptWithCertificate(this string encryptedValue, X509Certificate2 certificate)
        {
            var cipherData = Convert.FromBase64String(encryptedValue);
            // TSu: Changed to private key. Dont know why the public key was used for decryption
            var provider = (RSACryptoServiceProvider)certificate.PrivateKey;
            return Encoding.UTF8.GetString(provider.Decrypt(cipherData, true));
        }

        public static string DecryptWithCertificate(this string encryptedValue, KnownCertificate certificate)
        {
            return DecryptWithCertificate(encryptedValue, certificate.Thumbprint);
        }

        public static string DecryptWithCertificate(this string encryptedValue, string thumbprint)
        {
            var cipherData = Convert.FromBase64String(encryptedValue);
            var provider = GetCryptoServiceProvider(thumbprint, false);
            return Encoding.UTF8.GetString(provider.Decrypt(cipherData, RSAEncryptionPadding.OaepSHA1));
        }

        #endregion

        #region DecryptWithPasswordAndCertifcate

        public static string DecryptWithPasswordAndCertificate(this string encryptedValue, string encryptedPassword, KnownCertificate certificate)
        {
            return DecryptWithPasswordAndCertificate(encryptedValue, encryptedPassword, certificate.Thumbprint);
        }

        public static string DecryptWithPasswordAndCertificate(this string encryptedValue, string encryptedPassword, string thumbprint)
        {
            var password = DecryptWithCertificate(encryptedPassword, thumbprint);
            return DecryptSymmetric(encryptedValue, password);
        }

        #endregion

        #region EncryptWithCertificate

        public static string EncryptWithCertificate(this string value, X509Certificate2 certificate, bool usePrivateKey = false)
        {
            var provider = usePrivateKey ?
                (RSACryptoServiceProvider)certificate.PrivateKey :
                (RSACryptoServiceProvider)certificate.PublicKey.Key;

            var cipherData = provider.Encrypt(Encoding.UTF8.GetBytes(value), true);
            return Convert.ToBase64String(cipherData);
        }

        public static string EncryptWithCertificate(this string value, KnownCertificate certificate)
        {
            return EncryptWithCertificate(value, certificate.Thumbprint);
        }
        public static string EncryptWithCertificate(this string value, KnownCertificate certificate, bool usePrivateKey = false)
        {
            return EncryptWithCertificate(value, certificate.Thumbprint, usePrivateKey);
        }
        public static string EncryptWithCertificate(this string value, string thumbprint, bool usePrivateKey = false)
        {
            var provider = GetCryptoServiceProvider(thumbprint, !usePrivateKey);
            var cipherData = provider.Encrypt(Encoding.UTF8.GetBytes(value), RSAEncryptionPadding.OaepSHA1);
            return Convert.ToBase64String(cipherData);
        }

        #endregion

        #region EncryptWithPassword

        public static string EncryptWithPassword(this string value, string password)
        {
            return AESThenHMAC.SimpleEncryptWithPassword(value, password);
        }

        #endregion

        #region EncryptWithPasswordAndCertificate

        public static EncryptedValueAndPassword EncryptWithPasswordAndCertificate(this string value, KnownCertificate certificate)
        {
            return EncryptWithPasswordAndCertificate(value, certificate.Thumbprint);
        }

        public static EncryptedValueAndPassword EncryptWithPasswordAndCertificate(this string value, string thumbprint)
        {
            var password = PasswordGenerator.Default.Generate();
            var encryptedValue = EncryptWithPassword(value, password);
            var encryptedPassword = EncryptWithCertificate(password, thumbprint);

            return new EncryptedValueAndPassword { EncryptedPassword = encryptedPassword, EncryptedValue = encryptedValue };
        }

        #endregion

        #region SignWithCertificate

        static readonly RSASignaturePadding padding = RSASignaturePadding.Pkcs1;

        [Obsolete("This method uses SHA1 and is not considered safe. Use the overload with HashAlgorithmName instead.")]
        public static string SignWithCertificate(this string value, KnownCertificate certificate)
        {
            return SignWithCertificate(value, certificate, HashAlgorithmName.SHA1);
        }

        public static string SignWithCertificate(this string value, KnownCertificate certificate, HashAlgorithmName hashAlgorithmName)
        {
            using (var provider = GetCryptoServiceProvider(certificate.Thumbprint, false))
            {
                var cypher = provider.SignData(Encoding.UTF8.GetBytes(value), hashAlgorithmName, padding);
                return Convert.ToBase64String(cypher);
            }
        }

        #endregion

        #region VerifyWithCertificate

        public static bool VerifyWithCertificate(this string text, string hash, X509Certificate2 certificate)
        {
            var data = Encoding.UTF8.GetBytes(text);
            var signature = Convert.FromBase64String(hash);

            using (var provider = certificate.GetRSAPublicKey())
            {
                return provider.VerifyData(data, signature, HashAlgorithmName.SHA1, padding);
            }
        }

        #endregion

        #region ValidateSASFileCertifcate

        public static bool ValidateSASFileCertifcate(string filePath)
        {
            if (!File.Exists(filePath)) return false;
            try
            {
                var cert = X509Certificate.CreateFromSignedFile(filePath);
                if (cert != null)
                {
                    //CN=Swiss Academic Software, O=Swiss Academic Software, L=Waedenswil, S=Zurich, C=CH
                    var organisation = Regex.Match(cert.Subject, "O=(?<ORG>.+?)(,|$)").Groups["ORG"].Value.Trim();
                    if (string.Compare(organisation, "Swiss Academic Software", StringComparison.InvariantCulture) == 0) return true; //C6
                    if (string.Compare(organisation, "Swiss Academic Software GmbH", StringComparison.InvariantCulture) == 0) return true; //C6 Neu

                    organisation = Regex.Match(cert.Subject, "CN=(?<ORG>.+?)(,|$)").Groups["ORG"].Value.Trim();
                    if (string.Compare(organisation, "Swiss Academic Software", StringComparison.InvariantCulture) == 0) return true;//C6
                    if (string.Compare(organisation, "Swiss Academic Software GmbH", StringComparison.InvariantCulture) == 0) return true; //C6 Neu
                }
            }
            catch
            {
                //Tritt auf wenn nicht signiert
            }
            return false;
        }

        #endregion

        #region EvaluatePasswordStrength

        public static PasswordStrength EvaluatePasswordStrength(string password)
        {
            return PasswordStrengthChecker.EvaluatePasswordStrength(password);
        }

        #endregion

        #region FindCertificate

        public static X509Certificate2 FindCertificate(KnownCertificate certificate)
        {
            var storeLocation = StoreLocation.LocalMachine;

            // We are running on Azure
            if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")))
            {
                storeLocation = StoreLocation.CurrentUser;
            }

            return FindCertificate(StoreName.My, storeLocation, X509FindType.FindByThumbprint, certificate.Thumbprint);
        }

        public static X509Certificate2 FindCertificate(StoreName storeName, StoreLocation storeLocation, X509FindType findType, object findValue)
        {
            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                var certificates = store.Certificates.Find(findType, findValue, false);

                if (certificates.Count == 0)
                {
                    throw new ApplicationException(string.Format("No certificate was found for findValue {0}", findValue));
                }

                return certificates[0];
            }
            finally
            {
                store.Close();
            }
        }

        #endregion

        #region GetCryptoServiceProvider

        static RSA GetCryptoServiceProvider(string thumbprint, bool isEncryption)
        {
            var storeLocation = StoreLocation.LocalMachine;

            // We are running on Azure
            if (!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")))
            {
                storeLocation = StoreLocation.CurrentUser;
            }

            var cert = FindCertificate(StoreName.My, storeLocation, X509FindType.FindByThumbprint, thumbprint);

            if (isEncryption) return cert.GetRSAPublicKey();
            return cert.GetRSAPrivateKey();
        }

        #endregion

        #region GetSha1Hash

        public static string GetSha1Hash(this string value)
        {
#pragma warning disable CA5350, SCS0006 // Keine schwachen kryptografischen Algorithmen verwenden
            using (var sha1 = new SHA1CryptoServiceProvider())
#pragma warning restore CA5350, SCS0006 // Keine schwachen kryptografischen Algorithmen verwenden
            {
                var textToHash = Encoding.Unicode.GetBytes(value);
                var result = sha1.ComputeHash(textToHash);

                var s = new StringBuilder();
                foreach (var b in result) s.Append(b.ToString("x2").ToLower());

                return s.ToString();
            }
        }

        #endregion

        #region ToSecureString


        public static SecureString ToSecureString(this string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(value));

            unsafe
            {
                fixed (char* chars = value)
                {
                    var securePassword = new SecureString(chars, value.Length);
                    securePassword.MakeReadOnly();
                    return securePassword;
                }
            }
        }

        #endregion
    }

    public class EncryptedValueAndPassword
    {
        public string EncryptedPassword { get; set; }
        public string EncryptedValue { get; set; }
    }
}
