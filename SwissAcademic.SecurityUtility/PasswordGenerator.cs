using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Threading;
using System.Configuration;

namespace SwissAcademic.Security
{
    public class PasswordGenerator
    {
        #region Fields

        int _minimum = 16;
        int _maximum = 24;
        RNGCryptoServiceProvider _randomNumberGenerator;
        char[] _allowedLeadingCharacters;
        char[] _allowedCharacters = "abcdefghijkmlnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789+*%&/()=?-_$".ToCharArray();

        #endregion

        #region Constructor

        private PasswordGenerator()
        {
            _randomNumberGenerator = new RNGCryptoServiceProvider();
        }

        #endregion

        #region Properties

        #region Default

        static Lazy<PasswordGenerator> _default = new Lazy<PasswordGenerator>(() =>
            {
                return new PasswordGenerator();
            });

        public static PasswordGenerator Default
        {
            get { return _default.Value; }
        }

        #endregion

        #region LicenseKey

        static PasswordGenerator _licenseKey;

        public static PasswordGenerator LicenseKey =>
            LazyInitializer.EnsureInitialized(ref _licenseKey, () =>
            {
                var generator = new PasswordGenerator();
                generator._minimum = 12;
                generator._maximum = 14;
                generator._allowedCharacters = "bcdefghijkmlnopqrstuvwxyz0123456789".ToUpperInvariant().ToCharArray();
                generator._allowedLeadingCharacters = "abcdefghijkmlnopqrsuvwxyz".ToUpperInvariant().ToCharArray();
                return generator;
            });

        #endregion

        #region Prefix

        public string Prefix { get; set; } = string.Empty;

        #endregion

        #region Suffix

        public string Suffix { get; set; } = string.Empty;

        #endregion

        #region WebKey

        static PasswordGenerator _webKey;

        public static PasswordGenerator WebKey =>
            LazyInitializer.EnsureInitialized(ref _webKey, () =>
                {
                    var generator = new PasswordGenerator();
                    generator._minimum = 35;
                    generator._maximum = 50;

                    // t is excluded in order to exclude "temp"
                    generator._allowedCharacters = "abcdefghijkmlnopqrstuvwxyz0123456789".ToCharArray();
                    generator._allowedLeadingCharacters = "abcdefghijkmlnopqrsuvwxyz".ToCharArray();

                    return generator;
                });

        #endregion

        #region WebKeyTemporary

        static PasswordGenerator _webKeyTemporary;

        public static PasswordGenerator WebKeyTemporary => LazyInitializer.EnsureInitialized(ref _webKeyTemporary, () =>
                                                                         {
                                                                             var generator = new PasswordGenerator();
                                                                             generator._minimum = 35;
                                                                             generator._maximum = 46;

                                                                             // t is excluded in order to exclude "temp"
                                                                             generator._allowedCharacters = "abcdefghijkmlnopqrstuvwxyz0123456789".ToCharArray();
                                                                             generator._allowedLeadingCharacters = "abcdefghijkmlnopqrsuvwxyz".ToCharArray();

                                                                             generator.Prefix = "temp";

                                                                             return generator;
                                                                         });

        #endregion

        #endregion

        #region Methods

        #region Generate

        public string Generate()
        {
            // Pick random length between minimum and maximum
            var passwordLength = GetCryptographicRandomNumber(_minimum, _maximum);

            var chars = (from i in System.Linq.Enumerable.Range(0, passwordLength)
                         select GetRandomCharacter(i == 0)).ToArray();

            return Prefix + new string(chars) + Suffix;
        }

        #endregion

        #region GetCryptographicRandomNumber

        int GetCryptographicRandomNumber(int lBound, int uBound)
        {
            uint urndnum;
            byte[] rndnum = new Byte[4];

            uint xcludeRndBase = (uint.MaxValue - (uint.MaxValue % (uint)(uBound - lBound)));

            do
            {
                _randomNumberGenerator.GetBytes(rndnum);
                urndnum = System.BitConverter.ToUInt32(rndnum, 0);
            } while (urndnum >= xcludeRndBase);

            return (int)(urndnum % (uBound - lBound)) + lBound;
        }

        #endregion

        #region GetRandomCharacter

        char GetRandomCharacter(bool isLeadingCharacter)
        {
            if (isLeadingCharacter && _allowedLeadingCharacters != null)
            {
                var randomCharPosition = GetCryptographicRandomNumber(_allowedLeadingCharacters.GetLowerBound(0), _allowedLeadingCharacters.Length);
                return _allowedLeadingCharacters[randomCharPosition];
            }
            else
            {
                var randomCharPosition = GetCryptographicRandomNumber(_allowedCharacters.GetLowerBound(0), _allowedCharacters.Length);
                return _allowedCharacters[randomCharPosition];
            }
        }

        #endregion

        #endregion
    }
}