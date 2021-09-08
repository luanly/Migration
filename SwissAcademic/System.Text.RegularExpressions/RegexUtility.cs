using System.Collections.Generic;

namespace System.Text.RegularExpressions
{
    public static class RegexUtility
    {
        #region Felder

        //http://referencesource.microsoft.com/#System.ComponentModel.DataAnnotations/DataAnnotations/EmailAddressAttribute.cs,54
        const string pattern = @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$";
        const RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
        static Regex _emailValidationRegex = new Regex(pattern, options);

        /* TODO
        static Regex _tweetUrlRegex = new Regex(@"", options);
        static Regex _twitterProfileUrlRegex = new Regex(@"", options);


        static Regex _facebookProfileUrlRegex = new Regex(@"", options);
        static Regex _facebookPostUrlRegex = new Regex(@"", options);
        */
        #endregion

        #region Methods

        #region IsValidPassword

        public static bool IsValidPassword(this string password)
        {
            //Requirements: at least 8 chars, 1 digit, 1 uppercase und 1 lowercase letter
            return
                password.Length >= 8 &&
                Regex.IsMatch(password, @"[a-z]") &&
                Regex.IsMatch(password, @"[A-Z]") &&
                Regex.IsMatch(password, @"\d");
        }

        #endregion

        #region IsValidEmailAddress

        public static bool IsValidEmailAddress(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
            {
                return false;
            }
            return _emailValidationRegex.IsMatch(emailAddress);
        }

        #endregion

        #region GetEmailsFromString

        public static IEnumerable<string> GetEmailsFromString(string text)
        {
            var result = new List<string>();
            foreach (Match match in Regex.Matches(text, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase))
            {
                result.Add(match.Value);
            }
            return result;
        }

        #endregion

        #region SetGlobalTimeout

        public static void SetGlobalTimeout()
         => SetGlobalTimeout(TimeSpan.FromSeconds(30));

        public static void SetGlobalTimeout(TimeSpan timeout)
        {
            AppDomain.CurrentDomain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", timeout);
        }

        #endregion

        #endregion
    }
}
