using SwissAcademic.ApplicationInsights;
using System;
using System.Text;

namespace SwissAcademic
{
    public static class NumeralSystemConverter
    {
        //JHP TODO überlegen, welche int -> (string)Numeral Conversions wir als Extension-Method
        //der int Klasse anbieten könnten.

        #region GenerateNumber

        static string GenerateNumber(ref int value, int magnitude, char letter)
        {
            StringBuilder numberstring = new StringBuilder();
            while (value >= magnitude)
            {
                value -= magnitude;
                numberstring.Append(letter);
            }
            return (numberstring.ToString());
        }

        #endregion

        #region GetNumeralSystem

        public static NumeralSystem GetNumeralSystem(string input)
        {
            NumeralSystem foundNumeralSystem = NumeralSystem.Omit;

            if (IsArabicNumber(input)) return NumeralSystem.Arabic;



            return foundNumeralSystem;
        }

        #endregion GetNumeralSystem

        #region IsArabicNumber

        public static bool IsArabicNumber(string number)
        {
            if (string.IsNullOrEmpty(number))
            {
                return false;
            }

            for (int i = 0; i < number.Length; i++)
            {
                if (!Char.IsNumber(number[i]))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IsRomanNumber

        public static bool IsRomanNumber(string number)
        {
            //TODO JHP Validierung der Römischen Zahl
            if (string.IsNullOrEmpty(number))
            {
                return false;
            }

            for (int i = 0; i < number.Length; i++)
            {
                switch (number[i])
                {
                    case 'i':
                    case 'I':
                    case 'v':
                    case 'V':
                    case 'x':
                    case 'X':
                    case 'l':
                    case 'L':
                    case 'c':
                    case 'C':
                    case 'd':
                    case 'D':
                    case 'm':
                    case 'M':
                        break;

                    default:
                        return false;
                }
            }

            return true;
        }

        #endregion

        #region IsLowercaseRomanNumber

        public static bool IsLowercaseRomanNumber(string number)
        {
            //TODO JHP Validierung der Römischen Zahl, e.g. http://www.thevbprogrammer.com/Ch08/08-10-RomanNumerals.htm
            if (string.IsNullOrEmpty(number))
                return false;

            for (int i = 0; i < number.Length; i++)
            {
                switch (number[i])
                {
                    case 'i':
                    case 'v':
                    case 'x':
                    case 'l':
                    case 'c':
                    case 'd':
                    case 'm':
                        break;

                    default:
                        return false;
                }
            }
            return true;
        }

        #endregion IsLowercaseRomanNumber

        #region IsUppercaseRomanNumber

        public static bool IsUppercaseRomanNumber(string number)
        {
            //TODO JHP Validierung der Römischen Zahl, e.g. http://www.thevbprogrammer.com/Ch08/08-10-RomanNumerals.htm
            if (string.IsNullOrEmpty(number))
                return false;

            for (int i = 0; i < number.Length; i++)
            {
                switch (number[i])
                {
                    case 'I':
                    case 'V':
                    case 'X':
                    case 'L':
                    case 'C':
                    case 'D':
                    case 'M':
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        #endregion IsUppercaseRomanNumber

        #region ToArabicNumber

        public static string ToArabicNumber(string romanNumber)
        {
            if (string.IsNullOrEmpty(romanNumber))
                return romanNumber;

            if (IsArabicNumber(romanNumber))
                return romanNumber;

            romanNumber = romanNumber.ToUpper();
            int arabicNumber = 0;

            if (romanNumber.IndexOf("IV") != -1)
            {
                arabicNumber += 4;
                romanNumber = romanNumber.Replace("IV", string.Empty);
            }

            if (romanNumber.IndexOf("IX") != -1)
            {
                arabicNumber += 9;
                romanNumber = romanNumber.Replace("IX", string.Empty);
            }

            if (romanNumber.IndexOf("XL") != -1)
            {
                arabicNumber += 40;
                romanNumber = romanNumber.Replace("XL", string.Empty);
            }

            if (romanNumber.IndexOf("XC") != -1)
            {
                arabicNumber += 90;
                romanNumber = romanNumber.Replace("XC", string.Empty);
            }

            if (romanNumber.IndexOf("CD") != -1)
            {
                arabicNumber += 400;
                romanNumber = romanNumber.Replace("CD", string.Empty);
            }
            if (romanNumber.IndexOf("CM") != -1)
            {
                arabicNumber += 900;
                romanNumber = romanNumber.Replace("CM", string.Empty);
            }

            while (romanNumber.Length > 0)
            {
                switch (romanNumber[0].ToString())
                {
                    case "I":
                        arabicNumber++;
                        break;
                    case "V":
                        arabicNumber += 5;
                        break;
                    case "X":
                        arabicNumber += 10;
                        break;
                    case "L":
                        arabicNumber += 50;
                        break;
                    case "C":
                        arabicNumber += 100;
                        break;
                    case "D":
                        arabicNumber += 500;
                        break;
                    case "M":
                        arabicNumber += 1000;
                        break;
                }
                romanNumber = romanNumber.Remove(0, 1);
            }
            if (arabicNumber != 0)
                return arabicNumber.ToString();
            else
                return string.Empty;
        }

        #endregion

        #region ToLetter

        public static string ToLetter(string number, bool lowerCase = true)
        {
            //added JHP for enumerating multiple bibliography citations that are merged into one main bibliography entry
            //e.g.: [1] (a) .... (b) .... (z) .... (aa) .... etc.

            if (string.IsNullOrEmpty(number)) return string.Empty;  //conversion failed

            string arabicNumber = string.Empty;
            if (IsRomanNumber(number)) arabicNumber = ToArabicNumber(number);

            if (string.IsNullOrEmpty(arabicNumber) && !IsArabicNumber(number)) return string.Empty;

            arabicNumber = number;
            int integerNumber;
            if (!Int32.TryParse(arabicNumber, out integerNumber))
                return arabicNumber;

            return ToLetter(integerNumber, lowerCase);
        }

        public static string ToLetter(int number, bool lowerCase = true)
        {
            //added JHP for enumerating multiple bibliography citations that are merged into one main bibliography entry
            //e.g.: [1] (a) .... (b) .... (z) .... (aa) .... etc.

            string letter = string.Empty;
            for (int i = Convert.ToInt32(Math.Log(Convert.ToDouble(25 * (Convert.ToDouble(number) + 1))) / Math.Log(26)) - 1; i >= 0; i--)
            {
                int x = Convert.ToInt32(Math.Pow(26, i + 1) - 1) / 25 - 1;
                if (number > x)
                {
                    letter += (char)(((number - x - 1) / Convert.ToInt32(Math.Pow(26, i))) % 26 + 65);
                }
            }
            return lowerCase ? letter.ToLowerInvariant() : letter;
        }

        #endregion //ToLetter

        #region IntToString

        public static string IntToString(int number, NumeralSystem targetNumeralSystem, int fillNumberToMinimumLength, string fillCharacters)
        {
            switch (targetNumeralSystem)
            {
                case NumeralSystem.LetterLowerCase:
                    return ToLetter(number, true);

                case NumeralSystem.LetterUpperCase:
                    return ToLetter(number, false);

                case NumeralSystem.RomanLowerCase:
                    return ToRomanNumber(number, true);

                case NumeralSystem.RomanUpperCase:
                    return ToRomanNumber(number, false);

                case NumeralSystem.Omit:
                    return string.Empty;

                case NumeralSystem.Arabic:
                default:
                    //return number.ToString();
                    return FillNumberToMinimumLength(number, fillNumberToMinimumLength, fillCharacters);
            }
        }

        public static string IntToString(int number, NumeralSystem targetNumeralSystem)
        {
            return IntToString(number, targetNumeralSystem, 1, "0");
        }

        #endregion IntToString

        #region ToRomanNumber

        public static string ToRomanNumber(string arabicNumber, bool lowerCase = true)
        {
            //TODO JHP Validierung der Römischen Zahl, e.g. http://www.thevbprogrammer.com/Ch08/08-10-RomanNumerals.htm
            if (string.IsNullOrEmpty(arabicNumber))
                return arabicNumber;

            if (IsRomanNumber(arabicNumber))
                return arabicNumber;

            try
            {
                int romanNumber = Convert.ToInt32(arabicNumber);

                StringBuilder stringBuilder = new StringBuilder();

                if (romanNumber < 1 || romanNumber > 5000)
                {
                    return romanNumber.ToString();
                }
                else
                {
                    stringBuilder.Append(GenerateNumber(ref romanNumber, 1000, 'M'));
                    stringBuilder.Append(GenerateNumber(ref romanNumber, 500, 'D'));
                    stringBuilder.Append(GenerateNumber(ref romanNumber, 100, 'C'));
                    stringBuilder.Append(GenerateNumber(ref romanNumber, 50, 'L'));
                    stringBuilder.Append(GenerateNumber(ref romanNumber, 10, 'X'));
                    stringBuilder.Append(GenerateNumber(ref romanNumber, 5, 'V'));
                    stringBuilder.Append(GenerateNumber(ref romanNumber, 1, 'I'));

                    stringBuilder.Replace("IIII", "IV");
                    stringBuilder.Replace("VIV", "IX");
                    stringBuilder.Replace("XXXX", "XL");
                    stringBuilder.Replace("LXL", "XC");
                    stringBuilder.Replace("CCCC", "CD");
                    stringBuilder.Replace("DCD", "CM");

                    if (lowerCase)
                        return stringBuilder.ToString().ToLower();
                    else
                        return (stringBuilder.ToString());
                }
            }

            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                return arabicNumber;
            }
        }

        public static string ToRomanNumber(int number, bool lowerCase = true)
        {
            return ToRomanNumber(number.ToString(), lowerCase);
        }

        #endregion

        #region FillNumberToMinimumLength

        public static string FillNumberToMinimumLength(int number, int minimumLength, string fillCharacters = "0")
        {
            if (minimumLength < 1) minimumLength = 1;
            if (string.IsNullOrEmpty(fillCharacters)) fillCharacters = "0";

            string value = number.ToString();

            if (value.Length < minimumLength)
            {
                StringBuilder stringBuilder = new StringBuilder(value);
                int diff = minimumLength - value.Length;
                for (int j = 0; j < diff; j++)
                {
                    stringBuilder.Insert(0, fillCharacters);
                }
                return stringBuilder.ToString();
            }

            else
            {
                return value;
            }
        }

        #endregion FillNumberToMinimumLength

    }

    #region NumberingType

    public enum NumberingType
    {
        Page = 0,
        Column = 1,
        Paragraph = 2,
        Margin = 3,
        Other = 4
    }

    #endregion


    #region NumeralSystem

    //JHP Wenn ich den erweitern könnte um die LetterNumerals, 
    //dann könnten wir uns die Enum CompositeCitationNumeralType sparen und den NumeralSystemConverter
    //entsprechend erweitern
    public enum NumeralSystem
    {
        Omit = -1,          //added JHP to enable composite citations that do not seperate their sub-citations using numerals
        Arabic,
        RomanUpperCase,
        RomanLowerCase,
        LetterLowerCase,    //added JHP
        LetterUpperCase     //added JHP
    }

    #endregion
}
