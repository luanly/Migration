using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SwissAcademic
{
    #region NumberMatch

    public class NumberMatch
    {
        #region Konstruktoren

        internal NumberMatch(Match match, decimal number)
        {
            _match = match;
            _number = number;
        }

        #endregion

        #region Eigenschaften

        #region IsRoman

        public bool IsRoman { get; internal set; }

        #endregion

        #region Match

        Match _match;

        public Match Match
        {
            get { return _match; }
        }

        #endregion

        #region Number

        decimal _number;

        public decimal Number
        {
            get { return _number; }
        }

        #endregion

        #endregion
    }

    #endregion

    #region NumberInformation

    public static class NumberInformation
    {
        #region Methoden

        #region Matches

        public static List<NumberMatch> Matches(string numberString)
        {
            return Matches(numberString, RomanNumberMatchType.None, false);
        }

        public static List<NumberMatch> Matches(string numberString, RomanNumberMatchType romanNumberMatchType)
        {
            return Matches(numberString, romanNumberMatchType, false);
        }

        public static List<NumberMatch> Matches(string numberString, bool returnAbsoluteValues)
        {
            return Matches(numberString, RomanNumberMatchType.None, returnAbsoluteValues);
        }

        static Regex _containsDecimalNumberRegex = new Regex(@"(?:\b | (?<Minus>-\p{Zs}*))(?<Number>\d+)(?:(?:,|\.)(?<DecimalPlaces>\d+))?", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        static Regex _containsDecimalOrUpperCaseRomanNumber = new Regex(@"(?<DecimalNumber>(?:\b|(?<Minus>-\p{Zs}*))(?<Number>\d+)(?:(?:,|\.)(?<DecimalPlaces>\d+))?)|(?=\b[IVXLCDM]+\b)(?<RomanNumber>\bM{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})\b)", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

        public static List<NumberMatch> Matches(string numberString, RomanNumberMatchType romanNumberMatchType, bool returnAbsoluteValues)
        {
            var matches = new List<NumberMatch>();

            try
            {
                if (string.IsNullOrEmpty(numberString))
                {
                    return matches;
                }

                Regex regex;

                switch (romanNumberMatchType)
                {
                    case RomanNumberMatchType.Uppercase:
                        regex = _containsDecimalOrUpperCaseRomanNumber;
                        break;

                    case RomanNumberMatchType.AnyCase:
                        regex = _containsDecimalOrUpperCaseRomanNumber;
                        break;

                    default:
                        regex = _containsDecimalNumberRegex;
                        break;
                }

                foreach (Match match in regex.Matches(numberString))
                {
                    //if (match.Groups["DecimalNumber"].Success)		//JHP, see bug 8553
                    if (match.Groups["Number"].Success)
                    {
                        try
                        {
                            decimal number;

                            var stringBuilder = new StringBuilder();

                            if (!string.IsNullOrEmpty(match.Groups["Minus"].Value))
                            {
                                stringBuilder.Append("-");
                            }

                            stringBuilder.Append(match.Groups["Number"].Value);


                            if (!string.IsNullOrEmpty(match.Groups["DecimalPlaces"].Value))
                            {
                                stringBuilder.Append(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                                stringBuilder.Append(match.Groups["DecimalPlaces"].Value);
                            }

                            number = decimal.Parse(stringBuilder.ToString());
                            if (returnAbsoluteValues) number = Math.Abs(number);
                            matches.Add(new NumberMatch(match, number));
                        }

                        catch (Exception ignored)
                        {
                            Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                        }
                    }

                    else if (match.Groups["RomanNumber"].Success)
                    {
                        switch (romanNumberMatchType)
                        {
                            case RomanNumberMatchType.None:
                                continue;

                            default:
                                if (!NumeralSystemConverter.IsRomanNumber(match.Groups["RomanNumber"].Value)) continue;
                                break;
                        }

                        var arabicString = NumeralSystemConverter.ToArabicNumber(match.Groups["RomanNumber"].Value);

                        int intValue;
                        if (!int.TryParse(arabicString, out intValue)) continue;

                        matches.Add(new NumberMatch(match, intValue) { IsRoman = true });
                    }
                }

                return matches;
            }

            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                return new List<NumberMatch>();
            }
        }

        #endregion

        #endregion
    }

    public enum RomanNumberMatchType
    {
        None,
        Uppercase,
        AnyCase
    }

    #endregion
}
