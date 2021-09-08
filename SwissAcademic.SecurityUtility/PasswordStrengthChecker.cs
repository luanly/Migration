using System;

namespace SwissAcademic.Security
{
    // A simple version of http://www.csharphacker.com/technicalblog/index.php/2009/09/13/better-way-to-determine-and-police-password-strengths/
    internal static class PasswordStrengthChecker
    {
        #region Constructors

        static PasswordStrengthChecker()
        {
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        #region EvaluatePasswordStrength

        internal static PasswordStrength EvaluatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password)) return PasswordStrength.None;

            int bitStrength = GetPasswordPseudoEntropy(password);

            if (bitStrength < 30)
            {
                return PasswordStrength.Weak;
            }

            if (bitStrength < 50)
            {
                return PasswordStrength.Medium;
            }

            return PasswordStrength.Strong;
        }

        #endregion

        #region GetPasswordPseudoEntropy

        /// <summary>
        /// Returns an approximation to the entropy for the supplied password
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>Calculated strength of password in bits</returns>
        /// <remarks>
        /// Note as this is relatively subjective (unless the password is random)
        /// so it rarely will make sense to compare values between different
        /// password strength providers
        /// </remarks>
        static int GetPasswordPseudoEntropy(string password)
        {
            // Determine the max entropy for each character
            double entropyPerCharacter = GetPasswordEntropyPerCharacter(password);

            // return the strength removing any sequences and the biggest dictionary word
            return Convert.ToInt32(entropyPerCharacter * password.Length);
        }

        #endregion

        #region GetPasswordEntropyPerCharacter

        /// <summary>
        /// Gets the password entropy per character.
        /// </summary>
        /// <param name="password">The password to tbe determined.</param>
        /// <returns>Max number of entropy bits per character in the password</returns>
        static double GetPasswordEntropyPerCharacter(string password)
        {
            int maxCombinationsPerCharacterInPassword = 0;
            bool lowerCaseCharactersDetected = false;
            bool upperCaseCharactersDetected = false;
            bool digitsDetected = false;
            bool specialCharactersDetected = false;

            foreach (char character in password)
            {
                lowerCaseCharactersDetected |= char.IsLower(character);
                upperCaseCharactersDetected |= char.IsUpper(character);
                digitsDetected |= char.IsNumber(character);
                specialCharactersDetected |= (!char.IsLetter(character) && !char.IsNumber(character));
            }

            if (lowerCaseCharactersDetected) maxCombinationsPerCharacterInPassword += 26;
            if (upperCaseCharactersDetected) maxCombinationsPerCharacterInPassword += 26;
            if (digitsDetected) maxCombinationsPerCharacterInPassword += 10;
            if (specialCharactersDetected) maxCombinationsPerCharacterInPassword += 32;

            return Math.Log(maxCombinationsPerCharacterInPassword, 2);
        }

        #endregion

        #endregion
    }

    public enum PasswordStrength
    {
        None,
        Weak,
        Medium,
        Strong
    }
}
