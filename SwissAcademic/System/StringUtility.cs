using SmartFormat;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Globalization;
using SwissAcademic.Resources;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using CharUnicodeInfo = SwissAcademic.Globalization.CharUnicodeInfo;

namespace System
{
    #region StringUtility
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Provides utility methods for strings. </summary>
    ///
    /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public static partial class StringUtility
    {
        #region Char Constants

        //TODO JHP: For backwards compatibility only. For C7, change citation style scripts to use CharUnicodeInfo directly.
        public const char Space = CharUnicodeInfo.Space;
        public const char NonBreakingSpace = CharUnicodeInfo.NonBreakingSpace;
        public const char Hyphen = CharUnicodeInfo.Hyphen;
        public const char NonBreakingHyphen = CharUnicodeInfo.NonBreakingHyphen;
        public const char LeftDoubleQuotationMark = CharUnicodeInfo.LeftDoubleQuotationMark;
        public const char RightDoubleQuotationMark = CharUnicodeInfo.RightDoubleQuotationMark;
        public const char Divis = CharUnicodeInfo.Divis;
        public const char EnDash = CharUnicodeInfo.EnDash;
        public const char EmDash = CharUnicodeInfo.EmDash;

        #endregion

        #region AsLink

        public static string AsLink(this string self, string href)
        {
            return $"<a href=\"{href}\">{self}</a>";
        }

        #endregion

        #region Capitalize

        public static string Capitalize(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length == 1) return char.ToUpper(value[0]).ToString();
            return char.ToUpper(value[0]) + value.Substring(1);
        }

        #endregion

        #region Clean

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Converts a null string to an empty string and removes spaces, returns, and tabs at the
        /// beginning and end. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 29.04.2010. </remarks>
        ///
        /// <param name="value">	The value. </param>
        ///
        /// <returns>	The cleaned string. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string Clean(this string value)
        {
            return Clean(value, true);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Converts a null string to an empty string. If <paramref name="trim"/> is true, removes spaces,
        /// returns, and tabs at the beginning and end. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 29.04.2010. </remarks>
        ///
        /// <param name="value">	The value. </param>
        /// <param name="trim">		true to trim spaces, returns, and tabs. </param>
        ///
        /// <returns> The cleaned string. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string Clean(this string value, bool trim)
        {
            return Clean(value, IllegalCharacters.None, trim, null, null, null, null);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Converts a null string to an empty string and removes the specified illegal characters from
        /// the string. Removes spaces, returns, and tabs at the beginning and end. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 29.04.2010. </remarks>
        ///
        /// <param name="value">				The value. </param>
        /// <param name="illegalCharacters">	One or more values from the <see
        /// 									cref="IllegalCharacters"/> enumeration. The corresponding
        /// 									characters are removed from the string. </param>
        ///
        /// <returns>	The cleaned string. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string Clean(this string value, IllegalCharacters illegalCharacters)
        {
            return Clean(value, illegalCharacters, true);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Converts a null string to an empty string and removes the specified illegal characters from
        /// the string. If <paramref name="trim"/> is true, removes spaces, returns, and tabs at the
        /// beginning and end. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 29.04.2010. </remarks>
        ///
        /// <param name="value">				The value. </param>
        /// <param name="illegalCharacters">	One or more values from the <see
        /// 									cref="IllegalCharacters"/> enumeration. The corresponding
        /// 									characters are removed from the string. </param>
        /// <param name="trim">					true to trim spaces, returns, and tabs. </param>
        ///
        /// <returns>	The cleaned string. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string Clean(this string value, IllegalCharacters illegalCharacters, bool trim)
        {
            return Clean(value, illegalCharacters, trim, null, null, null, null);
        }

        static List<char> ReturnTab = new List<char> { '\r', '\n', '\t' };

        public static string Clean(this string value, IllegalCharacters illegalCharacters, bool trim, IEnumerable<char> charsToRemove, IEnumerable<char> charsToReplaceBySpace, IEnumerable<Tuple<Func<char, bool>, char>> replacements, IEnumerable<char> allowedInvalidXmlCharacters)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            List<char> charsToReplaceBySpaceList = null;
            List<char> charsToRemoveList = null;

            #region CharsToReplaceBySpace

            if (illegalCharacters != IllegalCharacters.None)
            {
                if ((illegalCharacters & IllegalCharacters.Backslash) == IllegalCharacters.Backslash ||
                    (illegalCharacters & IllegalCharacters.Return) == IllegalCharacters.Return ||
                    (illegalCharacters & IllegalCharacters.SemiColon) == IllegalCharacters.SemiColon ||
                    (illegalCharacters & IllegalCharacters.Slash) == IllegalCharacters.Slash ||
                    (illegalCharacters & IllegalCharacters.Tab) == IllegalCharacters.Tab)
                {
                    if (illegalCharacters == (IllegalCharacters.Return | IllegalCharacters.Tab))
                    {
                        charsToReplaceBySpaceList = ReturnTab;
                    }
                    else
                    {
                        charsToReplaceBySpaceList = new List<char>(6);

                        if ((illegalCharacters & IllegalCharacters.Backslash) == IllegalCharacters.Backslash) charsToReplaceBySpaceList.Add('\\');
                        if ((illegalCharacters & IllegalCharacters.Return) == IllegalCharacters.Return)
                        {
                            charsToReplaceBySpaceList.Add('\r');
                            charsToReplaceBySpaceList.Add('\n');
                        }
                        if ((illegalCharacters & IllegalCharacters.SemiColon) == IllegalCharacters.SemiColon) charsToReplaceBySpaceList.Add(';');
                        if ((illegalCharacters & IllegalCharacters.Slash) == IllegalCharacters.Slash) charsToReplaceBySpaceList.Add('/');
                        if ((illegalCharacters & IllegalCharacters.Tab) == IllegalCharacters.Tab) charsToReplaceBySpaceList.Add('\t');
                    }
                }
            }

            if (charsToReplaceBySpace != null && charsToReplaceBySpace.Any())
            {
                if (charsToReplaceBySpaceList == null) charsToReplaceBySpaceList = new List<char>(charsToReplaceBySpace);
                else charsToReplaceBySpaceList.AddRange(charsToReplaceBySpace);

                charsToReplaceBySpaceList = charsToReplaceBySpaceList.Distinct().ToList();
            }

            #endregion

            #region CharsToRemove

            if ((illegalCharacters & IllegalCharacters.ZeroWidthBreakOpportunity) == IllegalCharacters.ZeroWidthBreakOpportunity)
            {
                charsToRemoveList = new List<char>(CharUnicodeInfo.ZeroWidthBreakOpportunity);
            }

            if (charsToRemove != null && charsToRemove.Any())
            {
                if (charsToRemoveList == null) charsToRemoveList = new List<char>(charsToRemove);
                else charsToRemoveList.AddRange(charsToRemove);

                charsToRemoveList = charsToRemoveList.Distinct().ToList();
            }

            #endregion


            char[] cleanArray = null;
            var j = 0;

            for (int i = 0; i < value.Length; i++)
            {
                #region Always remove Invalid XML Characters

                if (i < value.Length - 1 && XmlConvert.IsXmlSurrogatePair(value[i + 1], value[i]))
                {
                    i++;
                    continue;
                }

                if (!XmlConvert.IsXmlChar(value[i]) &&
                    (allowedInvalidXmlCharacters == null || !allowedInvalidXmlCharacters.Contains(value[i])))

                {
                    Clean(ref cleanArray, value, i, ref j, '\0');
                    continue;
                }

                #endregion

                #region Always remove Unicode "Specials" characters

                if (value[i] >= 0xFFF0 && value[i] <= 0xFFFF)
                {
                    Clean(ref cleanArray, value, i, ref j, '\0');
                    continue;
                }

                #endregion

                if (charsToReplaceBySpaceList != null && charsToReplaceBySpaceList.Contains(value[i]))
                {
                    Clean(ref cleanArray, value, i, ref j, ' ');
                    continue;
                }

                if ((illegalCharacters & IllegalCharacters.NonStandardWhitespace) == IllegalCharacters.NonStandardWhitespace &&
                    value[i] != ' ' && char.IsWhiteSpace(value[i]))
                {
                    Clean(ref cleanArray, value, i, ref j, ' ');
                    continue;
                }

                if (charsToRemoveList != null && charsToRemoveList.Contains(value[i]))
                {
                    Clean(ref cleanArray, value, i, ref j, '\0');
                    continue;
                }

                if (replacements != null)
                {
                    var continueOuter = false;
                    foreach (var tuple in replacements)
                    {
                        if (tuple.Item1(value[i]))
                        {
                            Clean(ref cleanArray, value, i, ref j, tuple.Item2);
                            continueOuter = true;
                            break;
                        }
                    }

                    if (continueOuter) continue;
                }

                if (cleanArray != null)
                {
                    cleanArray[j] = value[i];
                    j++;
                }
            }

            if (trim) return cleanArray == null ? value.Trim(' ', '\r', '\n', '\t') : new string(cleanArray, 0, j).Trim(' ', '\r', '\n', '\t');
            else return cleanArray == null ? value : new string(cleanArray, 0, j);
        }

        static void Clean(ref char[] cleanArray, string value, int i, ref int j, char replacement)
        {
            if (cleanArray == null)
            {
                cleanArray = new char[value.Length];
                value.CopyTo(0, cleanArray, 0, i);
                j = i;
            }

            if (replacement == '\0') return;

            // Don't insert a replacement at the beginning or at the end
            if (j == 0 || i == value.Length - 1) return;

            // Don't insert the replacement twice
            if (cleanArray[j - 1] == replacement || value[i + 1] == replacement) return;

            cleanArray[j] = replacement;
            j++;
        }

        public static string Clean(string value, IEnumerable<Func<char, bool>> whiteList)
        {
            if (whiteList == null || !whiteList.Any()) return value;

            char[] cleanArray = null;
            var j = 0;

            for (int i = 0; i < value.Length; i++)
            {
                var isInWhiteList = (from item in whiteList
                                     where item(value[i])
                                     select item).Any();

                if (isInWhiteList)
                {
                    if (cleanArray != null)
                    {
                        cleanArray[j] = value[i];
                        j++;
                    }
                }
                else
                {
                    if (cleanArray == null)
                    {
                        cleanArray = new char[value.Length];
                        value.CopyTo(0, cleanArray, 0, i);
                        j = i;
                    }
                }
            }

            return cleanArray == null ? value : new string(cleanArray, 0, j);
        }

        #region CleanExceptLetterDigitsDashes

        public static string CleanExceptLetterDigitsDashes(this string value)
        {
            var whiteList = new Func<char, bool>[]
            {
                c => char.IsLetterOrDigit(c),
                c => CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.DashPunctuation
            };

            return Clean(value, whiteList);
        }

        #endregion

        public static string CleanNewlines(this string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;

            List<char> cleanedChars = new List<char>();

            int length = value.Length;
            char cPrev = '\0';
            char cCurr = '\0';
            char cNext = '\0';

            for (int i = 0; i < length; i++)
            {
                cCurr = value[i];
                cNext = i < length - 1 ? value[i + 1] : '\0';

                if
                (
                    (cCurr == '\r' && cNext != '\n') ||
                    (cCurr == '\n' && cPrev != '\r')
                )
                {
                    cleanedChars.Add('\r');
                    cleanedChars.Add('\n');
                }
                else if (cCurr != '\0')
                {
                    cleanedChars.Add(cCurr);
                }

                cPrev = cCurr;
            }

            if (cleanedChars.Count > 0)
            {
                return String.Concat(cleanedChars);
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region Join

        public static string Join(this IEnumerable<string> strings, string separator = null)
        {
            if (strings == null || !strings.Any()) return string.Empty;
            if (string.IsNullOrEmpty(separator)) separator = string.Empty;

            return string.Join(separator, strings);
        }

        public static string JoinNonEmpty(this IEnumerable<string> strings, string separator = null)
        {
            if (strings == null || !strings.Any()) return string.Empty;
            if (string.IsNullOrEmpty(separator)) separator = string.Empty;

            var stringsNonEmpty = strings.Where(item => !string.IsNullOrEmpty(item));
            if (stringsNonEmpty == null || !stringsNonEmpty.Any()) return string.Empty;

            return string.Join(separator, stringsNonEmpty);
        }

        public static string JoinNonWhiteSpace(this IEnumerable<string> strings, string separator = null)
        {
            if (strings == null || !strings.Any()) return string.Empty;
            if (string.IsNullOrEmpty(separator)) separator = string.Empty;

            var stringsNonWhitespace = strings.Where(item => !string.IsNullOrWhiteSpace(item));
            if (stringsNonWhitespace == null || !stringsNonWhitespace.Any()) return string.Empty;

            return string.Join(separator, stringsNonWhitespace);
        }

        #endregion

        #region Contains
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Returns a value indicating whether the specified System.String object occurs within this string
        /// </summary>
        ///
        /// <remarks>	Jörg Pasch, 06.03.2011. </remarks>
        ///
        /// <param name="value">	The value. </param>
        ///
        /// <returns>	true if contains, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool Contains(this string text, string value, StringComparison comparisonType)
        {
            return text.IndexOf(value, comparisonType) != -1;
        }

        #endregion

        #region ContainsEtAl

        const RegexOptions _etAltRegexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
        public static Regex _etAlRegex = new Regex(@"et\s*al", _etAltRegexOptions);

        public static bool ContainsEtAl(this string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            return _etAlRegex.IsMatch(text);
        }

        #endregion

        #region ContainsInPrintInformation

        static Lazy<IEnumerable<string>> _inPrintPlaceholderLanguageVersions = new Lazy<IEnumerable<string>>(() =>
        {
            return from tag in SwissAcademic.Properties.Resources.InPrintPlaceholderLanguageVersions.Split('|')
                   select tag.IsInBrackets() ? tag : "[{0}]".FormatString(tag);
        });

        public static bool ContainsInPrintInformation(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            foreach (string inPrintTag in _inPrintPlaceholderLanguageVersions.Value)
            {
                if (value.Contains(inPrintTag, StringComparison.CurrentCultureIgnoreCase)) return true;
            }
            return false;
        }

        #endregion ContainsInPrintInformation

        #region ContainsNoDateInformation

        const RegexOptions _noDateRegexOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant;
        static Regex _noDateRegex = new Regex(@"(?<nd>\bn\.{0,1} *d\.{0,1} *\b|\bno date\b)", _noDateRegexOptions);

        /// <summary>
        /// ND or n.d. or n. d. is required in APA citation style to indicate that a reference has no date.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsNoDateInformation(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            return _noDateRegex.IsMatch(value);
        }

        #endregion

        #region ConvertIntegerToLetter

        public static string ConvertIntegerToLetter(int number)
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
            return letter;

        }

        #endregion //ConvertIntegerToLetter

        #region EncodeToBase64

        public static string EncodeToBase64(this string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        #endregion

        #region DecodeFromBase64

        static public string DecodeFrom64(this string text)
        {
            var bytes = Convert.FromBase64String(text);
            return Encoding.UTF8.GetString(bytes);
        }

        #endregion

        #region EndsWithClosePunctuation

        public static bool EndsWithClosePunctuation(this string value)
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.unicodecategory?view=netcore-3.1
            //Here is the list of all chars in this category: https://www.fileformat.info/info/unicode/category/Pe/list.htm

            if (value == null || !value.Any()) return false;

            return System.Globalization.CharUnicodeInfo.GetUnicodeCategory(value.LastChar()) == UnicodeCategory.ClosePunctuation;
        }

        #endregion

        #region EndsWithFinalQuotePunctuation

        public static bool EndsWithFinalQuotePunctuation(this string value)
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.unicodecategory?view=netcore-3.1
            //Here is the list of all chars in this category: https://www.fileformat.info/info/unicode/category/Pf/list.htm

            if (value == null || !value.Any()) return false;

            //IMPORTANT: We need to cound the simple non-typographical quotation mark also as final quote punctuation
            return System.Globalization.CharUnicodeInfo.GetUnicodeCategory(value.LastChar()) == UnicodeCategory.OpenPunctuation ||
                value.EndsWith(CharUnicodeInfo.DoubleQuotationMark.ToString()) ||
                value.EndsWith(CharUnicodeInfo.LeftPointingAngleBracket.ToString()) ||
                value.EndsWith(CharUnicodeInfo.LeftPointingDoubleAngleQuotationMark.ToString());
        }

        #endregion

        #region EnsureTrailingSlash
        public static string EnsureTrailingSlash(this string url)
        {
            if (url != null && !url.EndsWith("/"))
            {
                return url + "/";
            }

            return url;
        }

		#endregion

		#region EscapeCurlyBraces

		public static string EscapeCurlyBraces(this string text, bool nonFormatItemsOnly = true)
        {
            //The following format string "{ foo:'{0}', bar:'{1}' }" becomes: "{{ foo:'{0}', bar: '{1}' }}".
            //Only those curly braces that are NOT format items must be escaped by means of 'doubling' .

            var curlyBracketExpressions = BracketExpression.FindSortedBracketExpressions(text, BracketType.CurlyBrackets);
            if (!curlyBracketExpressions.Any() || (curlyBracketExpressions.All(expression => expression.IsFormatItem) && nonFormatItemsOnly))
            {
                //no bracket expressions found or all found bracket expressions are format items > nothing to escape
                return text;
            }
            else
            {
                List<char> outputList = new List<char>();
                for (int i = 0; i < text.Length; i++)
                {
                    var c = text[i];
                    if
                    (
                        (c == CharUnicodeInfo.LeftCurlyBracket || c == CharUnicodeInfo.RightCurlyBracket) &&
                        curlyBracketExpressions.Any(expression => (expression.LeftBracketPosition == i || expression.RightBracketPosition == i) &&
                        (!expression.IsFormatItem || !nonFormatItemsOnly))
                    )
                    {
                        outputList.Add(c);
                    }

                    outputList.Add(c);
                }

                return new String(outputList.ToArray());
            }
        }

        #endregion EscapeCurlyBraces

        #region Equals

        public static bool Equals(this string input, char character)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.Length > 1) return false;
            return input[0].Equals(character);
        }

        #endregion

        #region EqualsAny

        public static bool EqualsAny(this string input, char[] characters)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.Length > 1) return false;

            return characters.Any(character => character.Equals(input[0]));
        }

        #endregion

        #region ExceptionDataToString

        public static string ExceptionDataToString(this Exception exception, string separator = ": ")
        {
            if (exception == null || exception.Data == null) return string.Empty;

            if (exception.Data.Count == 0) return string.Empty;
            StringBuilder sb = new StringBuilder();

            foreach (DictionaryEntry entry in exception.Data)
            {
                sb.Append(entry.Key.ToStringSafe());
                sb.Append(separator);
                if (entry.Value is IEnumerable<object>)
                {
                    sb.Append(((IEnumerable<object>)entry.Value).ToString("\r\n"));
                }
                else
                {
                    sb.Append(entry.Value.ToStringSafe());
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion 

        #region FirstChar

        public static char FirstChar(this string value)
        {
            if (string.IsNullOrEmpty(value)) return Char.MinValue;
            return value[0];
        }

        #endregion FirstChar

        #region FoldToAscii

        public static string FoldToAscii(this string input)
        {
            var output = new char[0];
            var outputPos = 4;

            FoldToAscii(input.ToCharArray(), input.Length, ref output, ref outputPos);
            return new string(output, 0, outputPos);
        }

        /// <summary> Converts characters above ASCII to their ASCII equivalents.  For example,
        /// accents are removed from accented characters.
        /// </summary>
        /// <param name="input">The string to fold
        /// </param>
        /// <param name="length">The number of characters in the input string
        /// </param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void FoldToAscii(char[] input, int length, ref char[] output, ref int outputPos)
        {
            // Worst-case length required:
            int maxSizeNeeded = 4 * length;
            if (output.Length < maxSizeNeeded)
            {
                output = new char[GetNextSize(maxSizeNeeded)];
            }

            outputPos = 0;

            #region Character folding

            for (var pos = 0; pos < length; ++pos)
            {
                char c = input[pos];

                // Quick test: if it's not in range then just keep current character
                if (c < '\u0080')
                {
                    output[outputPos++] = c;
                }
                else
                {
                    switch (c)
                    {
                        case '…':
                            output[outputPos++] = '.';
                            output[outputPos++] = '.';
                            output[outputPos++] = '.';
                            break;

                        case '\u00C0':
                        // Ãƒâ‚¬  [LATIN CAPITAL LETTER A WITH GRAVE]
                        case '\u00C1':
                        // Ãƒï¿½  [LATIN CAPITAL LETTER A WITH ACUTE]
                        case '\u00C2':
                        // Ãƒâ€š  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX]
                        case '\u00C3':
                        // ÃƒÆ’  [LATIN CAPITAL LETTER A WITH TILDE]
                        case '\u00C5':
                        // Ãƒâ€¦  [LATIN CAPITAL LETTER A WITH RING ABOVE]
                        case '\u0100':
                        // Ã„â‚¬  [LATIN CAPITAL LETTER A WITH MACRON]
                        case '\u0102':
                        // Ã„â€š  [LATIN CAPITAL LETTER A WITH BREVE]
                        case '\u0104':
                        // Ã„â€ž  [LATIN CAPITAL LETTER A WITH OGONEK]
                        case '\u018F':
                        // Ã†ï¿½  http://en.wikipedia.org/wiki/Schwa  [LATIN CAPITAL LETTER SCHWA]
                        case '\u01CD':
                        // Ã‡ï¿½  [LATIN CAPITAL LETTER A WITH CARON]
                        case '\u01DE':
                        // Ã‡Å¾  [LATIN CAPITAL LETTER A WITH DIAERESIS AND MACRON]
                        case '\u01E0':
                        // Ã‡Â   [LATIN CAPITAL LETTER A WITH DOT ABOVE AND MACRON]
                        case '\u01FA':
                        // Ã‡Âº  [LATIN CAPITAL LETTER A WITH RING ABOVE AND ACUTE]
                        case '\u0200':
                        // Ãˆâ‚¬  [LATIN CAPITAL LETTER A WITH DOUBLE GRAVE]
                        case '\u0202':
                        // Ãˆâ€š  [LATIN CAPITAL LETTER A WITH INVERTED BREVE]
                        case '\u0226':
                        // ÃˆÂ¦  [LATIN CAPITAL LETTER A WITH DOT ABOVE]
                        case '\u023A':
                        // ÃˆÂº  [LATIN CAPITAL LETTER A WITH STROKE]
                        case '\u1D00':
                        // Ã¡Â´â‚¬  [LATIN LETTER SMALL CAPITAL A]
                        case '\u1E00':
                        // Ã¡Â¸â‚¬  [LATIN CAPITAL LETTER A WITH RING BELOW]
                        case '\u1EA0':
                        // Ã¡ÂºÂ   [LATIN CAPITAL LETTER A WITH DOT BELOW]
                        case '\u1EA2':
                        // Ã¡ÂºÂ¢  [LATIN CAPITAL LETTER A WITH HOOK ABOVE]
                        case '\u1EA4':
                        // Ã¡ÂºÂ¤  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND ACUTE]
                        case '\u1EA6':
                        // Ã¡ÂºÂ¦  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND GRAVE]
                        case '\u1EA8':
                        // Ã¡ÂºÂ¨  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND HOOK ABOVE]
                        case '\u1EAA':
                        // Ã¡ÂºÂª  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND TILDE]
                        case '\u1EAC':
                        // Ã¡ÂºÂ¬  [LATIN CAPITAL LETTER A WITH CIRCUMFLEX AND DOT BELOW]
                        case '\u1EAE':
                        // Ã¡ÂºÂ®  [LATIN CAPITAL LETTER A WITH BREVE AND ACUTE]
                        case '\u1EB0':
                        // Ã¡ÂºÂ°  [LATIN CAPITAL LETTER A WITH BREVE AND GRAVE]
                        case '\u1EB2':
                        // Ã¡ÂºÂ²  [LATIN CAPITAL LETTER A WITH BREVE AND HOOK ABOVE]
                        case '\u1EB4':
                        // Ã¡ÂºÂ´  [LATIN CAPITAL LETTER A WITH BREVE AND TILDE]
                        case '\u1EB6':
                        // Ã¡ÂºÂ¶  [LATIN CAPITAL LETTER A WITH BREVE AND DOT BELOW]
                        case '\u24B6':
                        // Ã¢â€™Â¶  [CIRCLED LATIN CAPITAL LETTER A]
                        case '\uFF21':  // Ã¯Â¼Â¡  [FULLWIDTH LATIN CAPITAL LETTER A]
                            output[outputPos++] = 'A';
                            break;

                        case '\u00C4':
                            // Ãƒâ€ž  [LATIN CAPITAL LETTER A WITH DIAERESIS]
                            output[outputPos++] = 'A';
                            output[outputPos++] = 'e';
                            break;

                        case '\u00E0':
                        // ÃƒÂ   [LATIN SMALL LETTER A WITH GRAVE]
                        case '\u00E1':
                        // ÃƒÂ¡  [LATIN SMALL LETTER A WITH ACUTE]
                        case '\u00E2':
                        // ÃƒÂ¢  [LATIN SMALL LETTER A WITH CIRCUMFLEX]
                        case '\u00E3':
                        // ÃƒÂ£  [LATIN SMALL LETTER A WITH TILDE]
                        case '\u00E5':
                        // ÃƒÂ¥  [LATIN SMALL LETTER A WITH RING ABOVE]
                        case '\u0101':
                        // Ã„ï¿½  [LATIN SMALL LETTER A WITH MACRON]
                        case '\u0103':
                        // Ã„Æ’  [LATIN SMALL LETTER A WITH BREVE]
                        case '\u0105':
                        // Ã„â€¦  [LATIN SMALL LETTER A WITH OGONEK]
                        case '\u01CE':
                        // Ã‡Å½  [LATIN SMALL LETTER A WITH CARON]
                        case '\u01DF':
                        // Ã‡Å¸  [LATIN SMALL LETTER A WITH DIAERESIS AND MACRON]
                        case '\u01E1':
                        // Ã‡Â¡  [LATIN SMALL LETTER A WITH DOT ABOVE AND MACRON]
                        case '\u01FB':
                        // Ã‡Â»  [LATIN SMALL LETTER A WITH RING ABOVE AND ACUTE]
                        case '\u0201':
                        // Ãˆï¿½  [LATIN SMALL LETTER A WITH DOUBLE GRAVE]
                        case '\u0203':
                        // ÃˆÆ’  [LATIN SMALL LETTER A WITH INVERTED BREVE]
                        case '\u0227':
                        // ÃˆÂ§  [LATIN SMALL LETTER A WITH DOT ABOVE]
                        case '\u0250':
                        // Ã‰ï¿½  [LATIN SMALL LETTER TURNED A]
                        case '\u0259':
                        // Ã‰â„¢  [LATIN SMALL LETTER SCHWA]
                        case '\u025A':
                        // Ã‰Å¡  [LATIN SMALL LETTER SCHWA WITH HOOK]
                        case '\u1D8F':
                        // Ã¡Â¶ï¿½  [LATIN SMALL LETTER A WITH RETROFLEX HOOK]
                        case '\u1D95':
                        // Ã¡Â¶â€¢  [LATIN SMALL LETTER SCHWA WITH RETROFLEX HOOK]
                        case '\u1E01':
                        // Ã¡ÂºÂ¡  [LATIN SMALL LETTER A WITH RING BELOW]
                        case '\u1E9A':
                        // Ã¡ÂºÂ£  [LATIN SMALL LETTER A WITH RIGHT HALF RING]
                        case '\u1EA1':
                        // Ã¡ÂºÂ¡  [LATIN SMALL LETTER A WITH DOT BELOW]
                        case '\u1EA3':
                        // Ã¡ÂºÂ£  [LATIN SMALL LETTER A WITH HOOK ABOVE]
                        case '\u1EA5':
                        // Ã¡ÂºÂ¥  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND ACUTE]
                        case '\u1EA7':
                        // Ã¡ÂºÂ§  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND GRAVE]
                        case '\u1EA9':
                        // Ã¡ÂºÂ©  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND HOOK ABOVE]
                        case '\u1EAB':
                        // Ã¡ÂºÂ«  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND TILDE]
                        case '\u1EAD':
                        // Ã¡ÂºÂ­  [LATIN SMALL LETTER A WITH CIRCUMFLEX AND DOT BELOW]
                        case '\u1EAF':
                        // Ã¡ÂºÂ¯  [LATIN SMALL LETTER A WITH BREVE AND ACUTE]
                        case '\u1EB1':
                        // Ã¡ÂºÂ±  [LATIN SMALL LETTER A WITH BREVE AND GRAVE]
                        case '\u1EB3':
                        // Ã¡ÂºÂ³  [LATIN SMALL LETTER A WITH BREVE AND HOOK ABOVE]
                        case '\u1EB5':
                        // Ã¡ÂºÂµ  [LATIN SMALL LETTER A WITH BREVE AND TILDE]
                        case '\u1EB7':
                        // Ã¡ÂºÂ·  [LATIN SMALL LETTER A WITH BREVE AND DOT BELOW]
                        case '\u2090':
                        // Ã¢â€šï¿½  [LATIN SUBSCRIPT SMALL LETTER A]
                        case '\u2094':
                        // Ã¢â€šï¿½?  [LATIN SUBSCRIPT SMALL LETTER SCHWA]
                        case '\u24D0':
                        // Ã¢â€œï¿½  [CIRCLED LATIN SMALL LETTER A]
                        case '\u2C65':
                        // Ã¢Â±Â¥  [LATIN SMALL LETTER A WITH STROKE]
                        case '\u2C6F':
                        // Ã¢Â±Â¯  [LATIN CAPITAL LETTER TURNED A]
                        case '\uFF41':  // Ã¯Â½ï¿½  [FULLWIDTH LATIN SMALL LETTER A]
                            output[outputPos++] = 'a';
                            break;

                        case '\u00E4':
                            // ÃƒÂ¤  [LATIN SMALL LETTER A WITH DIAERESIS]
                            output[outputPos++] = 'a';
                            output[outputPos++] = 'e';
                            break;

                        case '\uA732':  // ÃªÅ“Â²  [LATIN CAPITAL LETTER AA]
                            output[outputPos++] = 'A';
                            output[outputPos++] = 'A';
                            break;

                        case '\u00C6':
                        // Ãƒâ€   [LATIN CAPITAL LETTER AE]
                        case '\u01E2':
                        // Ã‡Â¢  [LATIN CAPITAL LETTER AE WITH MACRON]
                        case '\u01FC':
                        // Ã‡Â¼  [LATIN CAPITAL LETTER AE WITH ACUTE]
                        case '\u1D01':  // Ã¡Â´ï¿½  [LATIN LETTER SMALL CAPITAL AE]
                            output[outputPos++] = 'A';
                            output[outputPos++] = 'E';
                            break;

                        case '\uA734':  // ÃªÅ“Â´  [LATIN CAPITAL LETTER AO]
                            output[outputPos++] = 'A';
                            output[outputPos++] = 'O';
                            break;

                        case '\uA736':  // ÃªÅ“Â¶  [LATIN CAPITAL LETTER AU]
                            output[outputPos++] = 'A';
                            output[outputPos++] = 'U';
                            break;

                        case '\uA738':
                        // ÃªÅ“Â¸  [LATIN CAPITAL LETTER AV]
                        case '\uA73A':  // ÃªÅ“Âº  [LATIN CAPITAL LETTER AV WITH HORIZONTAL BAR]
                            output[outputPos++] = 'A';
                            output[outputPos++] = 'V';
                            break;

                        case '\uA73C':  // ÃªÅ“Â¼  [LATIN CAPITAL LETTER AY]
                            output[outputPos++] = 'A';
                            output[outputPos++] = 'Y';
                            break;

                        case '\u249C':  // Ã¢â€™Å“  [PARENTHESIZED LATIN SMALL LETTER A]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'a';
                            output[outputPos++] = ')';
                            break;

                        case '\uA733':  // ÃªÅ“Â³  [LATIN SMALL LETTER AA]
                            output[outputPos++] = 'a';
                            output[outputPos++] = 'a';
                            break;

                        case '\u00E6':
                        // ÃƒÂ¦  [LATIN SMALL LETTER AE]
                        case '\u01E3':
                        // Ã‡Â£  [LATIN SMALL LETTER AE WITH MACRON]
                        case '\u01FD':
                        // Ã‡Â½  [LATIN SMALL LETTER AE WITH ACUTE]
                        case '\u1D02':  // Ã¡Â´â€š  [LATIN SMALL LETTER TURNED AE]
                            output[outputPos++] = 'a';
                            output[outputPos++] = 'e';
                            break;

                        case '\uA735':  // ÃªÅ“Âµ  [LATIN SMALL LETTER AO]
                            output[outputPos++] = 'a';
                            output[outputPos++] = 'o';
                            break;

                        case '\uA737':  // ÃªÅ“Â·  [LATIN SMALL LETTER AU]
                            output[outputPos++] = 'a';
                            output[outputPos++] = 'u';
                            break;

                        case '\uA739':
                        // ÃªÅ“Â¹  [LATIN SMALL LETTER AV]
                        case '\uA73B':  // ÃªÅ“Â»  [LATIN SMALL LETTER AV WITH HORIZONTAL BAR]
                            output[outputPos++] = 'a';
                            output[outputPos++] = 'v';
                            break;

                        case '\uA73D':  // ÃªÅ“Â½  [LATIN SMALL LETTER AY]
                            output[outputPos++] = 'a';
                            output[outputPos++] = 'y';
                            break;

                        case '\u0181':
                        // Ã†ï¿½  [LATIN CAPITAL LETTER B WITH HOOK]
                        case '\u0182':
                        // Ã†â€š  [LATIN CAPITAL LETTER B WITH TOPBAR]
                        case '\u0243':
                        // Ã‰Æ’  [LATIN CAPITAL LETTER B WITH STROKE]
                        case '\u0299':
                        // ÃŠâ„¢  [LATIN LETTER SMALL CAPITAL B]
                        case '\u1D03':
                        // Ã¡Â´Æ’  [LATIN LETTER SMALL CAPITAL BARRED B]
                        case '\u1E02':
                        // Ã¡Â¸â€š  [LATIN CAPITAL LETTER B WITH DOT ABOVE]
                        case '\u1E04':
                        // Ã¡Â¸â€ž  [LATIN CAPITAL LETTER B WITH DOT BELOW]
                        case '\u1E06':
                        // Ã¡Â¸â€   [LATIN CAPITAL LETTER B WITH LINE BELOW]
                        case '\u24B7':
                        // Ã¢â€™Â·  [CIRCLED LATIN CAPITAL LETTER B]
                        case '\uFF22':  // Ã¯Â¼Â¢  [FULLWIDTH LATIN CAPITAL LETTER B]
                            output[outputPos++] = 'B';
                            break;

                        case '\u0180':
                        // Ã†â‚¬  [LATIN SMALL LETTER B WITH STROKE]
                        case '\u0183':
                        // Ã†Æ’  [LATIN SMALL LETTER B WITH TOPBAR]
                        case '\u0253':
                        // Ã‰â€œ  [LATIN SMALL LETTER B WITH HOOK]
                        case '\u1D6C':
                        // Ã¡ÂµÂ¬  [LATIN SMALL LETTER B WITH MIDDLE TILDE]
                        case '\u1D80':
                        // Ã¡Â¶â‚¬  [LATIN SMALL LETTER B WITH PALATAL HOOK]
                        case '\u1E03':
                        // Ã¡Â¸Æ’  [LATIN SMALL LETTER B WITH DOT ABOVE]
                        case '\u1E05':
                        // Ã¡Â¸â€¦  [LATIN SMALL LETTER B WITH DOT BELOW]
                        case '\u1E07':
                        // Ã¡Â¸â€¡  [LATIN SMALL LETTER B WITH LINE BELOW]
                        case '\u24D1':
                        // Ã¢â€œâ€˜  [CIRCLED LATIN SMALL LETTER B]
                        case '\uFF42':  // Ã¯Â½â€š  [FULLWIDTH LATIN SMALL LETTER B]
                            output[outputPos++] = 'b';
                            break;

                        case '\u249D':  // Ã¢â€™ï¿½  [PARENTHESIZED LATIN SMALL LETTER B]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'b';
                            output[outputPos++] = ')';
                            break;

                        case '\u00C7':
                        // Ãƒâ€¡  [LATIN CAPITAL LETTER C WITH CEDILLA]
                        case '\u0106':
                        // Ã„â€   [LATIN CAPITAL LETTER C WITH ACUTE]
                        case '\u0108':
                        // Ã„Ë†  [LATIN CAPITAL LETTER C WITH CIRCUMFLEX]
                        case '\u010A':
                        // Ã„Å   [LATIN CAPITAL LETTER C WITH DOT ABOVE]
                        case '\u010C':
                        // Ã„Å’  [LATIN CAPITAL LETTER C WITH CARON]
                        case '\u0187':
                        // Ã†â€¡  [LATIN CAPITAL LETTER C WITH HOOK]
                        case '\u023B':
                        // ÃˆÂ»  [LATIN CAPITAL LETTER C WITH STROKE]
                        case '\u0297':
                        // ÃŠâ€”  [LATIN LETTER STRETCHED C]
                        case '\u1D04':
                        // Ã¡Â´â€ž  [LATIN LETTER SMALL CAPITAL C]
                        case '\u1E08':
                        // Ã¡Â¸Ë†  [LATIN CAPITAL LETTER C WITH CEDILLA AND ACUTE]
                        case '\u24B8':
                        // Ã¢â€™Â¸  [CIRCLED LATIN CAPITAL LETTER C]
                        case '\uFF23':  // Ã¯Â¼Â£  [FULLWIDTH LATIN CAPITAL LETTER C]
                            output[outputPos++] = 'C';
                            break;

                        case '\u00E7':
                        // ÃƒÂ§  [LATIN SMALL LETTER C WITH CEDILLA]
                        case '\u0107':
                        // Ã„â€¡  [LATIN SMALL LETTER C WITH ACUTE]
                        case '\u0109':
                        // Ã„â€°  [LATIN SMALL LETTER C WITH CIRCUMFLEX]
                        case '\u010B':
                        // Ã„â€¹  [LATIN SMALL LETTER C WITH DOT ABOVE]
                        case '\u010D':
                        // Ã„ï¿½  [LATIN SMALL LETTER C WITH CARON]
                        case '\u0188':
                        // Ã†Ë†  [LATIN SMALL LETTER C WITH HOOK]
                        case '\u023C':
                        // ÃˆÂ¼  [LATIN SMALL LETTER C WITH STROKE]
                        case '\u0255':
                        // Ã‰â€¢  [LATIN SMALL LETTER C WITH CURL]
                        case '\u1E09':
                        // Ã¡Â¸â€°  [LATIN SMALL LETTER C WITH CEDILLA AND ACUTE]
                        case '\u2184':
                        // Ã¢â€ â€ž  [LATIN SMALL LETTER REVERSED C]
                        case '\u24D2':
                        // Ã¢â€œâ€™  [CIRCLED LATIN SMALL LETTER C]
                        case '\uA73E':
                        // ÃªÅ“Â¾  [LATIN CAPITAL LETTER REVERSED C WITH DOT]
                        case '\uA73F':
                        // ÃªÅ“Â¿  [LATIN SMALL LETTER REVERSED C WITH DOT]
                        case '\uFF43':  // Ã¯Â½Æ’  [FULLWIDTH LATIN SMALL LETTER C]
                            output[outputPos++] = 'c';
                            break;

                        case '\u249E':  // Ã¢â€™Å¾  [PARENTHESIZED LATIN SMALL LETTER C]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'c';
                            output[outputPos++] = ')';
                            break;

                        case '\u00D0':
                        // Ãƒï¿½  [LATIN CAPITAL LETTER ETH]
                        case '\u010E':
                        // Ã„Å½  [LATIN CAPITAL LETTER D WITH CARON]
                        case '\u0110':
                        // Ã„ï¿½  [LATIN CAPITAL LETTER D WITH STROKE]
                        case '\u0189':
                        // Ã†â€°  [LATIN CAPITAL LETTER AFRICAN D]
                        case '\u018A':
                        // Ã†Å   [LATIN CAPITAL LETTER D WITH HOOK]
                        case '\u018B':
                        // Ã†â€¹  [LATIN CAPITAL LETTER D WITH TOPBAR]
                        case '\u1D05':
                        // Ã¡Â´â€¦  [LATIN LETTER SMALL CAPITAL D]
                        case '\u1D06':
                        // Ã¡Â´â€   [LATIN LETTER SMALL CAPITAL ETH]
                        case '\u1E0A':
                        // Ã¡Â¸Å   [LATIN CAPITAL LETTER D WITH DOT ABOVE]
                        case '\u1E0C':
                        // Ã¡Â¸Å’  [LATIN CAPITAL LETTER D WITH DOT BELOW]
                        case '\u1E0E':
                        // Ã¡Â¸Å½  [LATIN CAPITAL LETTER D WITH LINE BELOW]
                        case '\u1E10':
                        // Ã¡Â¸ï¿½  [LATIN CAPITAL LETTER D WITH CEDILLA]
                        case '\u1E12':
                        // Ã¡Â¸â€™  [LATIN CAPITAL LETTER D WITH CIRCUMFLEX BELOW]
                        case '\u24B9':
                        // Ã¢â€™Â¹  [CIRCLED LATIN CAPITAL LETTER D]
                        case '\uA779':
                        // Ãªï¿½Â¹  [LATIN CAPITAL LETTER INSULAR D]
                        case '\uFF24':  // Ã¯Â¼Â¤  [FULLWIDTH LATIN CAPITAL LETTER D]
                            output[outputPos++] = 'D';
                            break;

                        case '\u00F0':
                        // ÃƒÂ°  [LATIN SMALL LETTER ETH]
                        case '\u010F':
                        // Ã„ï¿½  [LATIN SMALL LETTER D WITH CARON]
                        case '\u0111':
                        // Ã„â€˜  [LATIN SMALL LETTER D WITH STROKE]
                        case '\u018C':
                        // Ã†Å’  [LATIN SMALL LETTER D WITH TOPBAR]
                        case '\u0221':
                        // ÃˆÂ¡  [LATIN SMALL LETTER D WITH CURL]
                        case '\u0256':
                        // Ã‰â€“  [LATIN SMALL LETTER D WITH TAIL]
                        case '\u0257':
                        // Ã‰â€”  [LATIN SMALL LETTER D WITH HOOK]
                        case '\u1D6D':
                        // Ã¡ÂµÂ­  [LATIN SMALL LETTER D WITH MIDDLE TILDE]
                        case '\u1D81':
                        // Ã¡Â¶ï¿½  [LATIN SMALL LETTER D WITH PALATAL HOOK]
                        case '\u1D91':
                        // Ã¡Â¶â€˜  [LATIN SMALL LETTER D WITH HOOK AND TAIL]
                        case '\u1E0B':
                        // Ã¡Â¸â€¹  [LATIN SMALL LETTER D WITH DOT ABOVE]
                        case '\u1E0D':
                        // Ã¡Â¸ï¿½  [LATIN SMALL LETTER D WITH DOT BELOW]
                        case '\u1E0F':
                        // Ã¡Â¸ï¿½  [LATIN SMALL LETTER D WITH LINE BELOW]
                        case '\u1E11':
                        // Ã¡Â¸â€˜  [LATIN SMALL LETTER D WITH CEDILLA]
                        case '\u1E13':
                        // Ã¡Â¸â€œ  [LATIN SMALL LETTER D WITH CIRCUMFLEX BELOW]
                        case '\u24D3':
                        // Ã¢â€œâ€œ  [CIRCLED LATIN SMALL LETTER D]
                        case '\uA77A':
                        // Ãªï¿½Âº  [LATIN SMALL LETTER INSULAR D]
                        case '\uFF44':  // Ã¯Â½â€ž  [FULLWIDTH LATIN SMALL LETTER D]
                            output[outputPos++] = 'd';
                            break;

                        case '\u01C4':
                        // Ã‡â€ž  [LATIN CAPITAL LETTER DZ WITH CARON]
                        case '\u01F1':  // Ã‡Â±  [LATIN CAPITAL LETTER DZ]
                            output[outputPos++] = 'D';
                            output[outputPos++] = 'Z';
                            break;

                        case '\u01C5':
                        // Ã‡â€¦  [LATIN CAPITAL LETTER D WITH SMALL LETTER Z WITH CARON]
                        case '\u01F2':  // Ã‡Â²  [LATIN CAPITAL LETTER D WITH SMALL LETTER Z]
                            output[outputPos++] = 'D';
                            output[outputPos++] = 'z';
                            break;

                        case '\u249F':  // Ã¢â€™Å¸  [PARENTHESIZED LATIN SMALL LETTER D]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'd';
                            output[outputPos++] = ')';
                            break;

                        case '\u0238':  // ÃˆÂ¸  [LATIN SMALL LETTER DB DIGRAPH]
                            output[outputPos++] = 'd';
                            output[outputPos++] = 'b';
                            break;

                        case '\u01C6':
                        // Ã‡â€   [LATIN SMALL LETTER DZ WITH CARON]
                        case '\u01F3':
                        // Ã‡Â³  [LATIN SMALL LETTER DZ]
                        case '\u02A3':
                        // ÃŠÂ£  [LATIN SMALL LETTER DZ DIGRAPH]
                        case '\u02A5':  // ÃŠÂ¥  [LATIN SMALL LETTER DZ DIGRAPH WITH CURL]
                            output[outputPos++] = 'd';
                            output[outputPos++] = 'z';
                            break;

                        case '\u00C8':
                        // ÃƒË†  [LATIN CAPITAL LETTER E WITH GRAVE]
                        case '\u00C9':
                        // Ãƒâ€°  [LATIN CAPITAL LETTER E WITH ACUTE]
                        case '\u00CA':
                        // ÃƒÅ   [LATIN CAPITAL LETTER E WITH CIRCUMFLEX]
                        case '\u00CB':
                        // Ãƒâ€¹  [LATIN CAPITAL LETTER E WITH DIAERESIS]
                        case '\u0112':
                        // Ã„â€™  [LATIN CAPITAL LETTER E WITH MACRON]
                        case '\u0114':
                        // Ã„ï¿½?  [LATIN CAPITAL LETTER E WITH BREVE]
                        case '\u0116':
                        // Ã„â€“  [LATIN CAPITAL LETTER E WITH DOT ABOVE]
                        case '\u0118':
                        // Ã„Ëœ  [LATIN CAPITAL LETTER E WITH OGONEK]
                        case '\u011A':
                        // Ã„Å¡  [LATIN CAPITAL LETTER E WITH CARON]
                        case '\u018E':
                        // Ã†Å½  [LATIN CAPITAL LETTER REVERSED E]
                        case '\u0190':
                        // Ã†ï¿½  [LATIN CAPITAL LETTER OPEN E]
                        case '\u0204':
                        // Ãˆâ€ž  [LATIN CAPITAL LETTER E WITH DOUBLE GRAVE]
                        case '\u0206':
                        // Ãˆâ€   [LATIN CAPITAL LETTER E WITH INVERTED BREVE]
                        case '\u0228':
                        // ÃˆÂ¨  [LATIN CAPITAL LETTER E WITH CEDILLA]
                        case '\u0246':
                        // Ã‰â€   [LATIN CAPITAL LETTER E WITH STROKE]
                        case '\u1D07':
                        // Ã¡Â´â€¡  [LATIN LETTER SMALL CAPITAL E]
                        case '\u1E14':
                        // Ã¡Â¸ï¿½?  [LATIN CAPITAL LETTER E WITH MACRON AND GRAVE]
                        case '\u1E16':
                        // Ã¡Â¸â€“  [LATIN CAPITAL LETTER E WITH MACRON AND ACUTE]
                        case '\u1E18':
                        // Ã¡Â¸Ëœ  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX BELOW]
                        case '\u1E1A':
                        // Ã¡Â¸Å¡  [LATIN CAPITAL LETTER E WITH TILDE BELOW]
                        case '\u1E1C':
                        // Ã¡Â¸Å“  [LATIN CAPITAL LETTER E WITH CEDILLA AND BREVE]
                        case '\u1EB8':
                        // Ã¡ÂºÂ¸  [LATIN CAPITAL LETTER E WITH DOT BELOW]
                        case '\u1EBA':
                        // Ã¡ÂºÂº  [LATIN CAPITAL LETTER E WITH HOOK ABOVE]
                        case '\u1EBC':
                        // Ã¡ÂºÂ¼  [LATIN CAPITAL LETTER E WITH TILDE]
                        case '\u1EBE':
                        // Ã¡ÂºÂ¾  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND ACUTE]
                        case '\u1EC0':
                        // Ã¡Â»â‚¬  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND GRAVE]
                        case '\u1EC2':
                        // Ã¡Â»â€š  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND HOOK ABOVE]
                        case '\u1EC4':
                        // Ã¡Â»â€ž  [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND TILDE]
                        case '\u1EC6':
                        // Ã¡Â»â€   [LATIN CAPITAL LETTER E WITH CIRCUMFLEX AND DOT BELOW]
                        case '\u24BA':
                        // Ã¢â€™Âº  [CIRCLED LATIN CAPITAL LETTER E]
                        case '\u2C7B':
                        // Ã¢Â±Â»  [LATIN LETTER SMALL CAPITAL TURNED E]
                        case '\uFF25':  // Ã¯Â¼Â¥  [FULLWIDTH LATIN CAPITAL LETTER E]
                            output[outputPos++] = 'E';
                            break;

                        case '\u00E8':
                        // ÃƒÂ¨  [LATIN SMALL LETTER E WITH GRAVE]
                        case '\u00E9':
                        // ÃƒÂ©  [LATIN SMALL LETTER E WITH ACUTE]
                        case '\u00EA':
                        // ÃƒÂª  [LATIN SMALL LETTER E WITH CIRCUMFLEX]
                        case '\u00EB':
                        // ÃƒÂ«  [LATIN SMALL LETTER E WITH DIAERESIS]
                        case '\u0113':
                        // Ã„â€œ  [LATIN SMALL LETTER E WITH MACRON]
                        case '\u0115':
                        // Ã„â€¢  [LATIN SMALL LETTER E WITH BREVE]
                        case '\u0117':
                        // Ã„â€”  [LATIN SMALL LETTER E WITH DOT ABOVE]
                        case '\u0119':
                        // Ã„â„¢  [LATIN SMALL LETTER E WITH OGONEK]
                        case '\u011B':
                        // Ã„â€º  [LATIN SMALL LETTER E WITH CARON]
                        case '\u01DD':
                        // Ã‡ï¿½  [LATIN SMALL LETTER TURNED E]
                        case '\u0205':
                        // Ãˆâ€¦  [LATIN SMALL LETTER E WITH DOUBLE GRAVE]
                        case '\u0207':
                        // Ãˆâ€¡  [LATIN SMALL LETTER E WITH INVERTED BREVE]
                        case '\u0229':
                        // ÃˆÂ©  [LATIN SMALL LETTER E WITH CEDILLA]
                        case '\u0247':
                        // Ã‰â€¡  [LATIN SMALL LETTER E WITH STROKE]
                        case '\u0258':
                        // Ã‰Ëœ  [LATIN SMALL LETTER REVERSED E]
                        case '\u025B':
                        // Ã‰â€º  [LATIN SMALL LETTER OPEN E]
                        case '\u025C':
                        // Ã‰Å“  [LATIN SMALL LETTER REVERSED OPEN E]
                        case '\u025D':
                        // Ã‰ï¿½  [LATIN SMALL LETTER REVERSED OPEN E WITH HOOK]
                        case '\u025E':
                        // Ã‰Å¾  [LATIN SMALL LETTER CLOSED REVERSED OPEN E]
                        case '\u029A':
                        // ÃŠÅ¡  [LATIN SMALL LETTER CLOSED OPEN E]
                        case '\u1D08':
                        // Ã¡Â´Ë†  [LATIN SMALL LETTER TURNED OPEN E]
                        case '\u1D92':
                        // Ã¡Â¶â€™  [LATIN SMALL LETTER E WITH RETROFLEX HOOK]
                        case '\u1D93':
                        // Ã¡Â¶â€œ  [LATIN SMALL LETTER OPEN E WITH RETROFLEX HOOK]
                        case '\u1D94':
                        // Ã¡Â¶ï¿½?  [LATIN SMALL LETTER REVERSED OPEN E WITH RETROFLEX HOOK]
                        case '\u1E15':
                        // Ã¡Â¸â€¢  [LATIN SMALL LETTER E WITH MACRON AND GRAVE]
                        case '\u1E17':
                        // Ã¡Â¸â€”  [LATIN SMALL LETTER E WITH MACRON AND ACUTE]
                        case '\u1E19':
                        // Ã¡Â¸â„¢  [LATIN SMALL LETTER E WITH CIRCUMFLEX BELOW]
                        case '\u1E1B':
                        // Ã¡Â¸â€º  [LATIN SMALL LETTER E WITH TILDE BELOW]
                        case '\u1E1D':
                        // Ã¡Â¸ï¿½  [LATIN SMALL LETTER E WITH CEDILLA AND BREVE]
                        case '\u1EB9':
                        // Ã¡ÂºÂ¹  [LATIN SMALL LETTER E WITH DOT BELOW]
                        case '\u1EBB':
                        // Ã¡ÂºÂ»  [LATIN SMALL LETTER E WITH HOOK ABOVE]
                        case '\u1EBD':
                        // Ã¡ÂºÂ½  [LATIN SMALL LETTER E WITH TILDE]
                        case '\u1EBF':
                        // Ã¡ÂºÂ¿  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND ACUTE]
                        case '\u1EC1':
                        // Ã¡Â»ï¿½  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND GRAVE]
                        case '\u1EC3':
                        // Ã¡Â»Æ’  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND HOOK ABOVE]
                        case '\u1EC5':
                        // Ã¡Â»â€¦  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND TILDE]
                        case '\u1EC7':
                        // Ã¡Â»â€¡  [LATIN SMALL LETTER E WITH CIRCUMFLEX AND DOT BELOW]
                        case '\u2091':
                        // Ã¢â€šâ€˜  [LATIN SUBSCRIPT SMALL LETTER E]
                        case '\u24D4':
                        // Ã¢â€œï¿½?  [CIRCLED LATIN SMALL LETTER E]
                        case '\u2C78':
                        // Ã¢Â±Â¸  [LATIN SMALL LETTER E WITH NOTCH]
                        case '\uFF45':  // Ã¯Â½â€¦  [FULLWIDTH LATIN SMALL LETTER E]
                            output[outputPos++] = 'e';
                            break;

                        case '\u24A0':  // Ã¢â€™Â   [PARENTHESIZED LATIN SMALL LETTER E]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'e';
                            output[outputPos++] = ')';
                            break;

                        case '\u0191':
                        // Ã†â€˜  [LATIN CAPITAL LETTER F WITH HOOK]
                        case '\u1E1E':
                        // Ã¡Â¸Å¾  [LATIN CAPITAL LETTER F WITH DOT ABOVE]
                        case '\u24BB':
                        // Ã¢â€™Â»  [CIRCLED LATIN CAPITAL LETTER F]
                        case '\uA730':
                        // ÃªÅ“Â°  [LATIN LETTER SMALL CAPITAL F]
                        case '\uA77B':
                        // Ãªï¿½Â»  [LATIN CAPITAL LETTER INSULAR F]
                        case '\uA7FB':
                        // ÃªÅ¸Â»  [LATIN EPIGRAPHIC LETTER REVERSED F]
                        case '\uFF26':  // Ã¯Â¼Â¦  [FULLWIDTH LATIN CAPITAL LETTER F]
                            output[outputPos++] = 'F';
                            break;

                        case '\u0192':
                        // Ã†â€™  [LATIN SMALL LETTER F WITH HOOK]
                        case '\u1D6E':
                        // Ã¡ÂµÂ®  [LATIN SMALL LETTER F WITH MIDDLE TILDE]
                        case '\u1D82':
                        // Ã¡Â¶â€š  [LATIN SMALL LETTER F WITH PALATAL HOOK]
                        case '\u1E1F':
                        // Ã¡Â¸Å¸  [LATIN SMALL LETTER F WITH DOT ABOVE]
                        case '\u1E9B':
                        // Ã¡Âºâ€º  [LATIN SMALL LETTER LONG S WITH DOT ABOVE]
                        case '\u24D5':
                        // Ã¢â€œâ€¢  [CIRCLED LATIN SMALL LETTER F]
                        case '\uA77C':
                        // Ãªï¿½Â¼  [LATIN SMALL LETTER INSULAR F]
                        case '\uFF46':  // Ã¯Â½â€   [FULLWIDTH LATIN SMALL LETTER F]
                            output[outputPos++] = 'f';
                            break;

                        case '\u24A1':  // Ã¢â€™Â¡  [PARENTHESIZED LATIN SMALL LETTER F]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'f';
                            output[outputPos++] = ')';
                            break;

                        case '\uFB00':  // Ã¯Â¬â‚¬  [LATIN SMALL LIGATURE FF]
                            output[outputPos++] = 'f';
                            output[outputPos++] = 'f';
                            break;

                        case '\uFB03':  // Ã¯Â¬Æ’  [LATIN SMALL LIGATURE FFI]
                            output[outputPos++] = 'f';
                            output[outputPos++] = 'f';
                            output[outputPos++] = 'i';
                            break;

                        case '\uFB04':  // Ã¯Â¬â€ž  [LATIN SMALL LIGATURE FFL]
                            output[outputPos++] = 'f';
                            output[outputPos++] = 'f';
                            output[outputPos++] = 'l';
                            break;

                        case '\uFB01':  // Ã¯Â¬ï¿½  [LATIN SMALL LIGATURE FI]
                            output[outputPos++] = 'f';
                            output[outputPos++] = 'i';
                            break;

                        case '\uFB02':  // Ã¯Â¬â€š  [LATIN SMALL LIGATURE FL]
                            output[outputPos++] = 'f';
                            output[outputPos++] = 'l';
                            break;

                        case '\u011C':
                        // Ã„Å“  [LATIN CAPITAL LETTER G WITH CIRCUMFLEX]
                        case '\u011E':
                        // Ã„Å¾  [LATIN CAPITAL LETTER G WITH BREVE]
                        case '\u0120':
                        // Ã„Â   [LATIN CAPITAL LETTER G WITH DOT ABOVE]
                        case '\u0122':
                        // Ã„Â¢  [LATIN CAPITAL LETTER G WITH CEDILLA]
                        case '\u0193':
                        // Ã†â€œ  [LATIN CAPITAL LETTER G WITH HOOK]
                        case '\u01E4':
                        // Ã‡Â¤  [LATIN CAPITAL LETTER G WITH STROKE]
                        case '\u01E5':
                        // Ã‡Â¥  [LATIN SMALL LETTER G WITH STROKE]
                        case '\u01E6':
                        // Ã‡Â¦  [LATIN CAPITAL LETTER G WITH CARON]
                        case '\u01E7':
                        // Ã‡Â§  [LATIN SMALL LETTER G WITH CARON]
                        case '\u01F4':
                        // Ã‡Â´  [LATIN CAPITAL LETTER G WITH ACUTE]
                        case '\u0262':
                        // Ã‰Â¢  [LATIN LETTER SMALL CAPITAL G]
                        case '\u029B':
                        // ÃŠâ€º  [LATIN LETTER SMALL CAPITAL G WITH HOOK]
                        case '\u1E20':
                        // Ã¡Â¸Â   [LATIN CAPITAL LETTER G WITH MACRON]
                        case '\u24BC':
                        // Ã¢â€™Â¼  [CIRCLED LATIN CAPITAL LETTER G]
                        case '\uA77D':
                        // Ãªï¿½Â½  [LATIN CAPITAL LETTER INSULAR G]
                        case '\uA77E':
                        // Ãªï¿½Â¾  [LATIN CAPITAL LETTER TURNED INSULAR G]
                        case '\uFF27':  // Ã¯Â¼Â§  [FULLWIDTH LATIN CAPITAL LETTER G]
                            output[outputPos++] = 'G';
                            break;

                        case '\u011D':
                        // Ã„ï¿½  [LATIN SMALL LETTER G WITH CIRCUMFLEX]
                        case '\u011F':
                        // Ã„Å¸  [LATIN SMALL LETTER G WITH BREVE]
                        case '\u0121':
                        // Ã„Â¡  [LATIN SMALL LETTER G WITH DOT ABOVE]
                        case '\u0123':
                        // Ã„Â£  [LATIN SMALL LETTER G WITH CEDILLA]
                        case '\u01F5':
                        // Ã‡Âµ  [LATIN SMALL LETTER G WITH ACUTE]
                        case '\u0260':
                        // Ã‰Â   [LATIN SMALL LETTER G WITH HOOK]
                        case '\u0261':
                        // Ã‰Â¡  [LATIN SMALL LETTER SCRIPT G]
                        case '\u1D77':
                        // Ã¡ÂµÂ·  [LATIN SMALL LETTER TURNED G]
                        case '\u1D79':
                        // Ã¡ÂµÂ¹  [LATIN SMALL LETTER INSULAR G]
                        case '\u1D83':
                        // Ã¡Â¶Æ’  [LATIN SMALL LETTER G WITH PALATAL HOOK]
                        case '\u1E21':
                        // Ã¡Â¸Â¡  [LATIN SMALL LETTER G WITH MACRON]
                        case '\u24D6':
                        // Ã¢â€œâ€“  [CIRCLED LATIN SMALL LETTER G]
                        case '\uA77F':
                        // Ãªï¿½Â¿  [LATIN SMALL LETTER TURNED INSULAR G]
                        case '\uFF47':  // Ã¯Â½â€¡  [FULLWIDTH LATIN SMALL LETTER G]
                            output[outputPos++] = 'g';
                            break;

                        case '\u24A2':  // Ã¢â€™Â¢  [PARENTHESIZED LATIN SMALL LETTER G]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'g';
                            output[outputPos++] = ')';
                            break;

                        case '\u0124':
                        // Ã„Â¤  [LATIN CAPITAL LETTER H WITH CIRCUMFLEX]
                        case '\u0126':
                        // Ã„Â¦  [LATIN CAPITAL LETTER H WITH STROKE]
                        case '\u021E':
                        // ÃˆÅ¾  [LATIN CAPITAL LETTER H WITH CARON]
                        case '\u029C':
                        // ÃŠÅ“  [LATIN LETTER SMALL CAPITAL H]
                        case '\u1E22':
                        // Ã¡Â¸Â¢  [LATIN CAPITAL LETTER H WITH DOT ABOVE]
                        case '\u1E24':
                        // Ã¡Â¸Â¤  [LATIN CAPITAL LETTER H WITH DOT BELOW]
                        case '\u1E26':
                        // Ã¡Â¸Â¦  [LATIN CAPITAL LETTER H WITH DIAERESIS]
                        case '\u1E28':
                        // Ã¡Â¸Â¨  [LATIN CAPITAL LETTER H WITH CEDILLA]
                        case '\u1E2A':
                        // Ã¡Â¸Âª  [LATIN CAPITAL LETTER H WITH BREVE BELOW]
                        case '\u24BD':
                        // Ã¢â€™Â½  [CIRCLED LATIN CAPITAL LETTER H]
                        case '\u2C67':
                        // Ã¢Â±Â§  [LATIN CAPITAL LETTER H WITH DESCENDER]
                        case '\u2C75':
                        // Ã¢Â±Âµ  [LATIN CAPITAL LETTER HALF H]
                        case '\uFF28':  // Ã¯Â¼Â¨  [FULLWIDTH LATIN CAPITAL LETTER H]
                            output[outputPos++] = 'H';
                            break;

                        case '\u0125':
                        // Ã„Â¥  [LATIN SMALL LETTER H WITH CIRCUMFLEX]
                        case '\u0127':
                        // Ã„Â§  [LATIN SMALL LETTER H WITH STROKE]
                        case '\u021F':
                        // ÃˆÅ¸  [LATIN SMALL LETTER H WITH CARON]
                        case '\u0265':
                        // Ã‰Â¥  [LATIN SMALL LETTER TURNED H]
                        case '\u0266':
                        // Ã‰Â¦  [LATIN SMALL LETTER H WITH HOOK]
                        case '\u02AE':
                        // ÃŠÂ®  [LATIN SMALL LETTER TURNED H WITH FISHHOOK]
                        case '\u02AF':
                        // ÃŠÂ¯  [LATIN SMALL LETTER TURNED H WITH FISHHOOK AND TAIL]
                        case '\u1E23':
                        // Ã¡Â¸Â£  [LATIN SMALL LETTER H WITH DOT ABOVE]
                        case '\u1E25':
                        // Ã¡Â¸Â¥  [LATIN SMALL LETTER H WITH DOT BELOW]
                        case '\u1E27':
                        // Ã¡Â¸Â§  [LATIN SMALL LETTER H WITH DIAERESIS]
                        case '\u1E29':
                        // Ã¡Â¸Â©  [LATIN SMALL LETTER H WITH CEDILLA]
                        case '\u1E2B':
                        // Ã¡Â¸Â«  [LATIN SMALL LETTER H WITH BREVE BELOW]
                        case '\u1E96':
                        // Ã¡Âºâ€“  [LATIN SMALL LETTER H WITH LINE BELOW]
                        case '\u24D7':
                        // Ã¢â€œâ€”  [CIRCLED LATIN SMALL LETTER H]
                        case '\u2C68':
                        // Ã¢Â±Â¨  [LATIN SMALL LETTER H WITH DESCENDER]
                        case '\u2C76':
                        // Ã¢Â±Â¶  [LATIN SMALL LETTER HALF H]
                        case '\uFF48':  // Ã¯Â½Ë†  [FULLWIDTH LATIN SMALL LETTER H]
                            output[outputPos++] = 'h';
                            break;

                        case '\u01F6':  // Ã‡Â¶  http://en.wikipedia.org/wiki/Hwair  [LATIN CAPITAL LETTER HWAIR]
                            output[outputPos++] = 'H';
                            output[outputPos++] = 'V';
                            break;

                        case '\u24A3':  // Ã¢â€™Â£  [PARENTHESIZED LATIN SMALL LETTER H]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'h';
                            output[outputPos++] = ')';
                            break;

                        case '\u0195':  // Ã†â€¢  [LATIN SMALL LETTER HV]
                            output[outputPos++] = 'h';
                            output[outputPos++] = 'v';
                            break;

                        case '\u00CC':
                        // ÃƒÅ’  [LATIN CAPITAL LETTER I WITH GRAVE]
                        case '\u00CD':
                        // Ãƒï¿½  [LATIN CAPITAL LETTER I WITH ACUTE]
                        case '\u00CE':
                        // ÃƒÅ½  [LATIN CAPITAL LETTER I WITH CIRCUMFLEX]
                        case '\u00CF':
                        // Ãƒï¿½  [LATIN CAPITAL LETTER I WITH DIAERESIS]
                        case '\u0128':
                        // Ã„Â¨  [LATIN CAPITAL LETTER I WITH TILDE]
                        case '\u012A':
                        // Ã„Âª  [LATIN CAPITAL LETTER I WITH MACRON]
                        case '\u012C':
                        // Ã„Â¬  [LATIN CAPITAL LETTER I WITH BREVE]
                        case '\u012E':
                        // Ã„Â®  [LATIN CAPITAL LETTER I WITH OGONEK]
                        case '\u0130':
                        // Ã„Â°  [LATIN CAPITAL LETTER I WITH DOT ABOVE]
                        case '\u0196':
                        // Ã†â€“  [LATIN CAPITAL LETTER IOTA]
                        case '\u0197':
                        // Ã†â€”  [LATIN CAPITAL LETTER I WITH STROKE]
                        case '\u01CF':
                        // Ã‡ï¿½  [LATIN CAPITAL LETTER I WITH CARON]
                        case '\u0208':
                        // ÃˆË†  [LATIN CAPITAL LETTER I WITH DOUBLE GRAVE]
                        case '\u020A':
                        // ÃˆÅ   [LATIN CAPITAL LETTER I WITH INVERTED BREVE]
                        case '\u026A':
                        // Ã‰Âª  [LATIN LETTER SMALL CAPITAL I]
                        case '\u1D7B':
                        // Ã¡ÂµÂ»  [LATIN SMALL CAPITAL LETTER I WITH STROKE]
                        case '\u1E2C':
                        // Ã¡Â¸Â¬  [LATIN CAPITAL LETTER I WITH TILDE BELOW]
                        case '\u1E2E':
                        // Ã¡Â¸Â®  [LATIN CAPITAL LETTER I WITH DIAERESIS AND ACUTE]
                        case '\u1EC8':
                        // Ã¡Â»Ë†  [LATIN CAPITAL LETTER I WITH HOOK ABOVE]
                        case '\u1ECA':
                        // Ã¡Â»Å   [LATIN CAPITAL LETTER I WITH DOT BELOW]
                        case '\u24BE':
                        // Ã¢â€™Â¾  [CIRCLED LATIN CAPITAL LETTER I]
                        case '\uA7FE':
                        // ÃªÅ¸Â¾  [LATIN EPIGRAPHIC LETTER I LONGA]
                        case '\uFF29':  // Ã¯Â¼Â©  [FULLWIDTH LATIN CAPITAL LETTER I]
                            output[outputPos++] = 'I';
                            break;

                        case '\u00EC':
                        // ÃƒÂ¬  [LATIN SMALL LETTER I WITH GRAVE]
                        case '\u00ED':
                        // ÃƒÂ­  [LATIN SMALL LETTER I WITH ACUTE]
                        case '\u00EE':
                        // ÃƒÂ®  [LATIN SMALL LETTER I WITH CIRCUMFLEX]
                        case '\u00EF':
                        // ÃƒÂ¯  [LATIN SMALL LETTER I WITH DIAERESIS]
                        case '\u0129':
                        // Ã„Â©  [LATIN SMALL LETTER I WITH TILDE]
                        case '\u012B':
                        // Ã„Â«  [LATIN SMALL LETTER I WITH MACRON]
                        case '\u012D':
                        // Ã„Â­  [LATIN SMALL LETTER I WITH BREVE]
                        case '\u012F':
                        // Ã„Â¯  [LATIN SMALL LETTER I WITH OGONEK]
                        case '\u0131':
                        // Ã„Â±  [LATIN SMALL LETTER DOTLESS I]
                        case '\u01D0':
                        // Ã‡ï¿½  [LATIN SMALL LETTER I WITH CARON]
                        case '\u0209':
                        // Ãˆâ€°  [LATIN SMALL LETTER I WITH DOUBLE GRAVE]
                        case '\u020B':
                        // Ãˆâ€¹  [LATIN SMALL LETTER I WITH INVERTED BREVE]
                        case '\u0268':
                        // Ã‰Â¨  [LATIN SMALL LETTER I WITH STROKE]
                        case '\u1D09':
                        // Ã¡Â´â€°  [LATIN SMALL LETTER TURNED I]
                        case '\u1D62':
                        // Ã¡ÂµÂ¢  [LATIN SUBSCRIPT SMALL LETTER I]
                        case '\u1D7C':
                        // Ã¡ÂµÂ¼  [LATIN SMALL LETTER IOTA WITH STROKE]
                        case '\u1D96':
                        // Ã¡Â¶â€“  [LATIN SMALL LETTER I WITH RETROFLEX HOOK]
                        case '\u1E2D':
                        // Ã¡Â¸Â­  [LATIN SMALL LETTER I WITH TILDE BELOW]
                        case '\u1E2F':
                        // Ã¡Â¸Â¯  [LATIN SMALL LETTER I WITH DIAERESIS AND ACUTE]
                        case '\u1EC9':
                        // Ã¡Â»â€°  [LATIN SMALL LETTER I WITH HOOK ABOVE]
                        case '\u1ECB':
                        // Ã¡Â»â€¹  [LATIN SMALL LETTER I WITH DOT BELOW]
                        case '\u2071':
                        // Ã¢ï¿½Â±  [SUPERSCRIPT LATIN SMALL LETTER I]
                        case '\u24D8':
                        // Ã¢â€œËœ  [CIRCLED LATIN SMALL LETTER I]
                        case '\uFF49':  // Ã¯Â½â€°  [FULLWIDTH LATIN SMALL LETTER I]
                            output[outputPos++] = 'i';
                            break;

                        case '\u0132':  // Ã„Â²  [LATIN CAPITAL LIGATURE IJ]
                            output[outputPos++] = 'I';
                            output[outputPos++] = 'J';
                            break;

                        case '\u24A4':  // Ã¢â€™Â¤  [PARENTHESIZED LATIN SMALL LETTER I]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'i';
                            output[outputPos++] = ')';
                            break;

                        case '\u0133':  // Ã„Â³  [LATIN SMALL LIGATURE IJ]
                            output[outputPos++] = 'i';
                            output[outputPos++] = 'j';
                            break;

                        case '\u0134':
                        // Ã„Â´  [LATIN CAPITAL LETTER J WITH CIRCUMFLEX]
                        case '\u0248':
                        // Ã‰Ë†  [LATIN CAPITAL LETTER J WITH STROKE]
                        case '\u1D0A':
                        // Ã¡Â´Å   [LATIN LETTER SMALL CAPITAL J]
                        case '\u24BF':
                        // Ã¢â€™Â¿  [CIRCLED LATIN CAPITAL LETTER J]
                        case '\uFF2A':  // Ã¯Â¼Âª  [FULLWIDTH LATIN CAPITAL LETTER J]
                            output[outputPos++] = 'J';
                            break;

                        case '\u0135':
                        // Ã„Âµ  [LATIN SMALL LETTER J WITH CIRCUMFLEX]
                        case '\u01F0':
                        // Ã‡Â°  [LATIN SMALL LETTER J WITH CARON]
                        case '\u0237':
                        // ÃˆÂ·  [LATIN SMALL LETTER DOTLESS J]
                        case '\u0249':
                        // Ã‰â€°  [LATIN SMALL LETTER J WITH STROKE]
                        case '\u025F':
                        // Ã‰Å¸  [LATIN SMALL LETTER DOTLESS J WITH STROKE]
                        case '\u0284':
                        // ÃŠâ€ž  [LATIN SMALL LETTER DOTLESS J WITH STROKE AND HOOK]
                        case '\u029D':
                        // ÃŠï¿½  [LATIN SMALL LETTER J WITH CROSSED-TAIL]
                        case '\u24D9':
                        // Ã¢â€œâ„¢  [CIRCLED LATIN SMALL LETTER J]
                        case '\u2C7C':
                        // Ã¢Â±Â¼  [LATIN SUBSCRIPT SMALL LETTER J]
                        case '\uFF4A':  // Ã¯Â½Å   [FULLWIDTH LATIN SMALL LETTER J]
                            output[outputPos++] = 'j';
                            break;

                        case '\u24A5':  // Ã¢â€™Â¥  [PARENTHESIZED LATIN SMALL LETTER J]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'j';
                            output[outputPos++] = ')';
                            break;

                        case '\u0136':
                        // Ã„Â¶  [LATIN CAPITAL LETTER K WITH CEDILLA]
                        case '\u0198':
                        // Ã†Ëœ  [LATIN CAPITAL LETTER K WITH HOOK]
                        case '\u01E8':
                        // Ã‡Â¨  [LATIN CAPITAL LETTER K WITH CARON]
                        case '\u1D0B':
                        // Ã¡Â´â€¹  [LATIN LETTER SMALL CAPITAL K]
                        case '\u1E30':
                        // Ã¡Â¸Â°  [LATIN CAPITAL LETTER K WITH ACUTE]
                        case '\u1E32':
                        // Ã¡Â¸Â²  [LATIN CAPITAL LETTER K WITH DOT BELOW]
                        case '\u1E34':
                        // Ã¡Â¸Â´  [LATIN CAPITAL LETTER K WITH LINE BELOW]
                        case '\u24C0':
                        // Ã¢â€œâ‚¬  [CIRCLED LATIN CAPITAL LETTER K]
                        case '\u2C69':
                        // Ã¢Â±Â©  [LATIN CAPITAL LETTER K WITH DESCENDER]
                        case '\uA740':
                        // Ãªï¿½â‚¬  [LATIN CAPITAL LETTER K WITH STROKE]
                        case '\uA742':
                        // Ãªï¿½â€š  [LATIN CAPITAL LETTER K WITH DIAGONAL STROKE]
                        case '\uA744':
                        // Ãªï¿½â€ž  [LATIN CAPITAL LETTER K WITH STROKE AND DIAGONAL STROKE]
                        case '\uFF2B':  // Ã¯Â¼Â«  [FULLWIDTH LATIN CAPITAL LETTER K]
                            output[outputPos++] = 'K';
                            break;

                        case '\u0137':
                        // Ã„Â·  [LATIN SMALL LETTER K WITH CEDILLA]
                        case '\u0199':
                        // Ã†â„¢  [LATIN SMALL LETTER K WITH HOOK]
                        case '\u01E9':
                        // Ã‡Â©  [LATIN SMALL LETTER K WITH CARON]
                        case '\u029E':
                        // ÃŠÅ¾  [LATIN SMALL LETTER TURNED K]
                        case '\u1D84':
                        // Ã¡Â¶â€ž  [LATIN SMALL LETTER K WITH PALATAL HOOK]
                        case '\u1E31':
                        // Ã¡Â¸Â±  [LATIN SMALL LETTER K WITH ACUTE]
                        case '\u1E33':
                        // Ã¡Â¸Â³  [LATIN SMALL LETTER K WITH DOT BELOW]
                        case '\u1E35':
                        // Ã¡Â¸Âµ  [LATIN SMALL LETTER K WITH LINE BELOW]
                        case '\u24DA':
                        // Ã¢â€œÅ¡  [CIRCLED LATIN SMALL LETTER K]
                        case '\u2C6A':
                        // Ã¢Â±Âª  [LATIN SMALL LETTER K WITH DESCENDER]
                        case '\uA741':
                        // Ãªï¿½ï¿½  [LATIN SMALL LETTER K WITH STROKE]
                        case '\uA743':
                        // Ãªï¿½Æ’  [LATIN SMALL LETTER K WITH DIAGONAL STROKE]
                        case '\uA745':
                        // Ãªï¿½â€¦  [LATIN SMALL LETTER K WITH STROKE AND DIAGONAL STROKE]
                        case '\uFF4B':  // Ã¯Â½â€¹  [FULLWIDTH LATIN SMALL LETTER K]
                            output[outputPos++] = 'k';
                            break;

                        case '\u24A6':  // Ã¢â€™Â¦  [PARENTHESIZED LATIN SMALL LETTER K]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'k';
                            output[outputPos++] = ')';
                            break;

                        case '\u0139':
                        // Ã„Â¹  [LATIN CAPITAL LETTER L WITH ACUTE]
                        case '\u013B':
                        // Ã„Â»  [LATIN CAPITAL LETTER L WITH CEDILLA]
                        case '\u013D':
                        // Ã„Â½  [LATIN CAPITAL LETTER L WITH CARON]
                        case '\u013F':
                        // Ã„Â¿  [LATIN CAPITAL LETTER L WITH MIDDLE DOT]
                        case '\u0141':
                        // Ã…ï¿½  [LATIN CAPITAL LETTER L WITH STROKE]
                        case '\u023D':
                        // ÃˆÂ½  [LATIN CAPITAL LETTER L WITH BAR]
                        case '\u029F':
                        // ÃŠÅ¸  [LATIN LETTER SMALL CAPITAL L]
                        case '\u1D0C':
                        // Ã¡Â´Å’  [LATIN LETTER SMALL CAPITAL L WITH STROKE]
                        case '\u1E36':
                        // Ã¡Â¸Â¶  [LATIN CAPITAL LETTER L WITH DOT BELOW]
                        case '\u1E38':
                        // Ã¡Â¸Â¸  [LATIN CAPITAL LETTER L WITH DOT BELOW AND MACRON]
                        case '\u1E3A':
                        // Ã¡Â¸Âº  [LATIN CAPITAL LETTER L WITH LINE BELOW]
                        case '\u1E3C':
                        // Ã¡Â¸Â¼  [LATIN CAPITAL LETTER L WITH CIRCUMFLEX BELOW]
                        case '\u24C1':
                        // Ã¢â€œï¿½  [CIRCLED LATIN CAPITAL LETTER L]
                        case '\u2C60':
                        // Ã¢Â±Â   [LATIN CAPITAL LETTER L WITH DOUBLE BAR]
                        case '\u2C62':
                        // Ã¢Â±Â¢  [LATIN CAPITAL LETTER L WITH MIDDLE TILDE]
                        case '\uA746':
                        // Ãªï¿½â€   [LATIN CAPITAL LETTER BROKEN L]
                        case '\uA748':
                        // Ãªï¿½Ë†  [LATIN CAPITAL LETTER L WITH HIGH STROKE]
                        case '\uA780':
                        // ÃªÅ¾â‚¬  [LATIN CAPITAL LETTER TURNED L]
                        case '\uFF2C':  // Ã¯Â¼Â¬  [FULLWIDTH LATIN CAPITAL LETTER L]
                            output[outputPos++] = 'L';
                            break;

                        case '\u013A':
                        // Ã„Âº  [LATIN SMALL LETTER L WITH ACUTE]
                        case '\u013C':
                        // Ã„Â¼  [LATIN SMALL LETTER L WITH CEDILLA]
                        case '\u013E':
                        // Ã„Â¾  [LATIN SMALL LETTER L WITH CARON]
                        case '\u0140':
                        // Ã…â‚¬  [LATIN SMALL LETTER L WITH MIDDLE DOT]
                        case '\u0142':
                        // Ã…â€š  [LATIN SMALL LETTER L WITH STROKE]
                        case '\u019A':
                        // Ã†Å¡  [LATIN SMALL LETTER L WITH BAR]
                        case '\u0234':
                        // ÃˆÂ´  [LATIN SMALL LETTER L WITH CURL]
                        case '\u026B':
                        // Ã‰Â«  [LATIN SMALL LETTER L WITH MIDDLE TILDE]
                        case '\u026C':
                        // Ã‰Â¬  [LATIN SMALL LETTER L WITH BELT]
                        case '\u026D':
                        // Ã‰Â­  [LATIN SMALL LETTER L WITH RETROFLEX HOOK]
                        case '\u1D85':
                        // Ã¡Â¶â€¦  [LATIN SMALL LETTER L WITH PALATAL HOOK]
                        case '\u1E37':
                        // Ã¡Â¸Â·  [LATIN SMALL LETTER L WITH DOT BELOW]
                        case '\u1E39':
                        // Ã¡Â¸Â¹  [LATIN SMALL LETTER L WITH DOT BELOW AND MACRON]
                        case '\u1E3B':
                        // Ã¡Â¸Â»  [LATIN SMALL LETTER L WITH LINE BELOW]
                        case '\u1E3D':
                        // Ã¡Â¸Â½  [LATIN SMALL LETTER L WITH CIRCUMFLEX BELOW]
                        case '\u24DB':
                        // Ã¢â€œâ€º  [CIRCLED LATIN SMALL LETTER L]
                        case '\u2C61':
                        // Ã¢Â±Â¡  [LATIN SMALL LETTER L WITH DOUBLE BAR]
                        case '\uA747':
                        // Ãªï¿½â€¡  [LATIN SMALL LETTER BROKEN L]
                        case '\uA749':
                        // Ãªï¿½â€°  [LATIN SMALL LETTER L WITH HIGH STROKE]
                        case '\uA781':
                        // ÃªÅ¾ï¿½  [LATIN SMALL LETTER TURNED L]
                        case '\uFF4C':  // Ã¯Â½Å’  [FULLWIDTH LATIN SMALL LETTER L]
                            output[outputPos++] = 'l';
                            break;

                        case '\u01C7':  // Ã‡â€¡  [LATIN CAPITAL LETTER LJ]
                            output[outputPos++] = 'L';
                            output[outputPos++] = 'J';
                            break;

                        case '\u1EFA':  // Ã¡Â»Âº  [LATIN CAPITAL LETTER MIDDLE-WELSH LL]
                            output[outputPos++] = 'L';
                            output[outputPos++] = 'L';
                            break;

                        case '\u01C8':  // Ã‡Ë†  [LATIN CAPITAL LETTER L WITH SMALL LETTER J]
                            output[outputPos++] = 'L';
                            output[outputPos++] = 'j';
                            break;

                        case '\u24A7':  // Ã¢â€™Â§  [PARENTHESIZED LATIN SMALL LETTER L]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'l';
                            output[outputPos++] = ')';
                            break;

                        case '\u01C9':  // Ã‡â€°  [LATIN SMALL LETTER LJ]
                            output[outputPos++] = 'l';
                            output[outputPos++] = 'j';
                            break;

                        case '\u1EFB':  // Ã¡Â»Â»  [LATIN SMALL LETTER MIDDLE-WELSH LL]
                            output[outputPos++] = 'l';
                            output[outputPos++] = 'l';
                            break;

                        case '\u02AA':  // ÃŠÂª  [LATIN SMALL LETTER LS DIGRAPH]
                            output[outputPos++] = 'l';
                            output[outputPos++] = 's';
                            break;

                        case '\u02AB':  // ÃŠÂ«  [LATIN SMALL LETTER LZ DIGRAPH]
                            output[outputPos++] = 'l';
                            output[outputPos++] = 'z';
                            break;

                        case '\u019C':
                        // Ã†Å“  [LATIN CAPITAL LETTER TURNED M]
                        case '\u1D0D':
                        // Ã¡Â´ï¿½  [LATIN LETTER SMALL CAPITAL M]
                        case '\u1E3E':
                        // Ã¡Â¸Â¾  [LATIN CAPITAL LETTER M WITH ACUTE]
                        case '\u1E40':
                        // Ã¡Â¹â‚¬  [LATIN CAPITAL LETTER M WITH DOT ABOVE]
                        case '\u1E42':
                        // Ã¡Â¹â€š  [LATIN CAPITAL LETTER M WITH DOT BELOW]
                        case '\u24C2':
                        // Ã¢â€œâ€š  [CIRCLED LATIN CAPITAL LETTER M]
                        case '\u2C6E':
                        // Ã¢Â±Â®  [LATIN CAPITAL LETTER M WITH HOOK]
                        case '\uA7FD':
                        // ÃªÅ¸Â½  [LATIN EPIGRAPHIC LETTER INVERTED M]
                        case '\uA7FF':
                        // ÃªÅ¸Â¿  [LATIN EPIGRAPHIC LETTER ARCHAIC M]
                        case '\uFF2D':  // Ã¯Â¼Â­  [FULLWIDTH LATIN CAPITAL LETTER M]
                            output[outputPos++] = 'M';
                            break;

                        case '\u026F':
                        // Ã‰Â¯  [LATIN SMALL LETTER TURNED M]
                        case '\u0270':
                        // Ã‰Â°  [LATIN SMALL LETTER TURNED M WITH LONG LEG]
                        case '\u0271':
                        // Ã‰Â±  [LATIN SMALL LETTER M WITH HOOK]
                        case '\u1D6F':
                        // Ã¡ÂµÂ¯  [LATIN SMALL LETTER M WITH MIDDLE TILDE]
                        case '\u1D86':
                        // Ã¡Â¶â€   [LATIN SMALL LETTER M WITH PALATAL HOOK]
                        case '\u1E3F':
                        // Ã¡Â¸Â¿  [LATIN SMALL LETTER M WITH ACUTE]
                        case '\u1E41':
                        // Ã¡Â¹ï¿½  [LATIN SMALL LETTER M WITH DOT ABOVE]
                        case '\u1E43':
                        // Ã¡Â¹Æ’  [LATIN SMALL LETTER M WITH DOT BELOW]
                        case '\u24DC':
                        // Ã¢â€œÅ“  [CIRCLED LATIN SMALL LETTER M]
                        case '\uFF4D':  // Ã¯Â½ï¿½  [FULLWIDTH LATIN SMALL LETTER M]
                            output[outputPos++] = 'm';
                            break;

                        case '\u24A8':  // Ã¢â€™Â¨  [PARENTHESIZED LATIN SMALL LETTER M]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'm';
                            output[outputPos++] = ')';
                            break;

                        case '\u00D1':
                        // Ãƒâ€˜  [LATIN CAPITAL LETTER N WITH TILDE]
                        case '\u0143':
                        // Ã…Æ’  [LATIN CAPITAL LETTER N WITH ACUTE]
                        case '\u0145':
                        // Ã…â€¦  [LATIN CAPITAL LETTER N WITH CEDILLA]
                        case '\u0147':
                        // Ã…â€¡  [LATIN CAPITAL LETTER N WITH CARON]
                        case '\u014A':
                        // Ã…Å   http://en.wikipedia.org/wiki/Eng_(letter)  [LATIN CAPITAL LETTER ENG]
                        case '\u019D':
                        // Ã†ï¿½  [LATIN CAPITAL LETTER N WITH LEFT HOOK]
                        case '\u01F8':
                        // Ã‡Â¸  [LATIN CAPITAL LETTER N WITH GRAVE]
                        case '\u0220':
                        // ÃˆÂ   [LATIN CAPITAL LETTER N WITH LONG RIGHT LEG]
                        case '\u0274':
                        // Ã‰Â´  [LATIN LETTER SMALL CAPITAL N]
                        case '\u1D0E':
                        // Ã¡Â´Å½  [LATIN LETTER SMALL CAPITAL REVERSED N]
                        case '\u1E44':
                        // Ã¡Â¹â€ž  [LATIN CAPITAL LETTER N WITH DOT ABOVE]
                        case '\u1E46':
                        // Ã¡Â¹â€   [LATIN CAPITAL LETTER N WITH DOT BELOW]
                        case '\u1E48':
                        // Ã¡Â¹Ë†  [LATIN CAPITAL LETTER N WITH LINE BELOW]
                        case '\u1E4A':
                        // Ã¡Â¹Å   [LATIN CAPITAL LETTER N WITH CIRCUMFLEX BELOW]
                        case '\u24C3':
                        // Ã¢â€œÆ’  [CIRCLED LATIN CAPITAL LETTER N]
                        case '\uFF2E':  // Ã¯Â¼Â®  [FULLWIDTH LATIN CAPITAL LETTER N]
                            output[outputPos++] = 'N';
                            break;

                        case '\u00F1':
                        // ÃƒÂ±  [LATIN SMALL LETTER N WITH TILDE]
                        case '\u0144':
                        // Ã…â€ž  [LATIN SMALL LETTER N WITH ACUTE]
                        case '\u0146':
                        // Ã…â€   [LATIN SMALL LETTER N WITH CEDILLA]
                        case '\u0148':
                        // Ã…Ë†  [LATIN SMALL LETTER N WITH CARON]
                        case '\u0149':
                        // Ã…â€°  [LATIN SMALL LETTER N PRECEDED BY APOSTROPHE]
                        case '\u014B':
                        // Ã…â€¹  http://en.wikipedia.org/wiki/Eng_(letter)  [LATIN SMALL LETTER ENG]
                        case '\u019E':
                        // Ã†Å¾  [LATIN SMALL LETTER N WITH LONG RIGHT LEG]
                        case '\u01F9':
                        // Ã‡Â¹  [LATIN SMALL LETTER N WITH GRAVE]
                        case '\u0235':
                        // ÃˆÂµ  [LATIN SMALL LETTER N WITH CURL]
                        case '\u0272':
                        // Ã‰Â²  [LATIN SMALL LETTER N WITH LEFT HOOK]
                        case '\u0273':
                        // Ã‰Â³  [LATIN SMALL LETTER N WITH RETROFLEX HOOK]
                        case '\u1D70':
                        // Ã¡ÂµÂ°  [LATIN SMALL LETTER N WITH MIDDLE TILDE]
                        case '\u1D87':
                        // Ã¡Â¶â€¡  [LATIN SMALL LETTER N WITH PALATAL HOOK]
                        case '\u1E45':
                        // Ã¡Â¹â€¦  [LATIN SMALL LETTER N WITH DOT ABOVE]
                        case '\u1E47':
                        // Ã¡Â¹â€¡  [LATIN SMALL LETTER N WITH DOT BELOW]
                        case '\u1E49':
                        // Ã¡Â¹â€°  [LATIN SMALL LETTER N WITH LINE BELOW]
                        case '\u1E4B':
                        // Ã¡Â¹â€¹  [LATIN SMALL LETTER N WITH CIRCUMFLEX BELOW]
                        case '\u207F':
                        // Ã¢ï¿½Â¿  [SUPERSCRIPT LATIN SMALL LETTER N]
                        case '\u24DD':
                        // Ã¢â€œï¿½  [CIRCLED LATIN SMALL LETTER N]
                        case '\uFF4E':  // Ã¯Â½Å½  [FULLWIDTH LATIN SMALL LETTER N]
                            output[outputPos++] = 'n';
                            break;

                        case '\u01CA':  // Ã‡Å   [LATIN CAPITAL LETTER NJ]
                            output[outputPos++] = 'N';
                            output[outputPos++] = 'J';
                            break;

                        case '\u01CB':  // Ã‡â€¹  [LATIN CAPITAL LETTER N WITH SMALL LETTER J]
                            output[outputPos++] = 'N';
                            output[outputPos++] = 'j';
                            break;

                        case '\u24A9':  // Ã¢â€™Â©  [PARENTHESIZED LATIN SMALL LETTER N]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'n';
                            output[outputPos++] = ')';
                            break;

                        case '\u01CC':  // Ã‡Å’  [LATIN SMALL LETTER NJ]
                            output[outputPos++] = 'n';
                            output[outputPos++] = 'j';
                            break;

                        case '\u00D2':
                        // Ãƒâ€™  [LATIN CAPITAL LETTER O WITH GRAVE]
                        case '\u00D3':
                        // Ãƒâ€œ  [LATIN CAPITAL LETTER O WITH ACUTE]
                        case '\u00D4':
                        // Ãƒï¿½?  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX]
                        case '\u00D5':
                        // Ãƒâ€¢  [LATIN CAPITAL LETTER O WITH TILDE]
                        case '\u00D8':
                        // ÃƒËœ  [LATIN CAPITAL LETTER O WITH STROKE]
                        case '\u014C':
                        // Ã…Å’  [LATIN CAPITAL LETTER O WITH MACRON]
                        case '\u014E':
                        // Ã…Å½  [LATIN CAPITAL LETTER O WITH BREVE]
                        case '\u0150':
                        // Ã…ï¿½  [LATIN CAPITAL LETTER O WITH DOUBLE ACUTE]
                        case '\u0186':
                        // Ã†â€   [LATIN CAPITAL LETTER OPEN O]
                        case '\u019F':
                        // Ã†Å¸  [LATIN CAPITAL LETTER O WITH MIDDLE TILDE]
                        case '\u01A0':
                        // Ã†Â   [LATIN CAPITAL LETTER O WITH HORN]
                        case '\u01D1':
                        // Ã‡â€˜  [LATIN CAPITAL LETTER O WITH CARON]
                        case '\u01EA':
                        // Ã‡Âª  [LATIN CAPITAL LETTER O WITH OGONEK]
                        case '\u01EC':
                        // Ã‡Â¬  [LATIN CAPITAL LETTER O WITH OGONEK AND MACRON]
                        case '\u01FE':
                        // Ã‡Â¾  [LATIN CAPITAL LETTER O WITH STROKE AND ACUTE]
                        case '\u020C':
                        // ÃˆÅ’  [LATIN CAPITAL LETTER O WITH DOUBLE GRAVE]
                        case '\u020E':
                        // ÃˆÅ½  [LATIN CAPITAL LETTER O WITH INVERTED BREVE]
                        case '\u022A':
                        // ÃˆÂª  [LATIN CAPITAL LETTER O WITH DIAERESIS AND MACRON]
                        case '\u022C':
                        // ÃˆÂ¬  [LATIN CAPITAL LETTER O WITH TILDE AND MACRON]
                        case '\u022E':
                        // ÃˆÂ®  [LATIN CAPITAL LETTER O WITH DOT ABOVE]
                        case '\u0230':
                        // ÃˆÂ°  [LATIN CAPITAL LETTER O WITH DOT ABOVE AND MACRON]
                        case '\u1D0F':
                        // Ã¡Â´ï¿½  [LATIN LETTER SMALL CAPITAL O]
                        case '\u1D10':
                        // Ã¡Â´ï¿½  [LATIN LETTER SMALL CAPITAL OPEN O]
                        case '\u1E4C':
                        // Ã¡Â¹Å’  [LATIN CAPITAL LETTER O WITH TILDE AND ACUTE]
                        case '\u1E4E':
                        // Ã¡Â¹Å½  [LATIN CAPITAL LETTER O WITH TILDE AND DIAERESIS]
                        case '\u1E50':
                        // Ã¡Â¹ï¿½  [LATIN CAPITAL LETTER O WITH MACRON AND GRAVE]
                        case '\u1E52':
                        // Ã¡Â¹â€™  [LATIN CAPITAL LETTER O WITH MACRON AND ACUTE]
                        case '\u1ECC':
                        // Ã¡Â»Å’  [LATIN CAPITAL LETTER O WITH DOT BELOW]
                        case '\u1ECE':
                        // Ã¡Â»Å½  [LATIN CAPITAL LETTER O WITH HOOK ABOVE]
                        case '\u1ED0':
                        // Ã¡Â»ï¿½  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND ACUTE]
                        case '\u1ED2':
                        // Ã¡Â»â€™  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND GRAVE]
                        case '\u1ED4':
                        // Ã¡Â»ï¿½?  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND HOOK ABOVE]
                        case '\u1ED6':
                        // Ã¡Â»â€“  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND TILDE]
                        case '\u1ED8':
                        // Ã¡Â»Ëœ  [LATIN CAPITAL LETTER O WITH CIRCUMFLEX AND DOT BELOW]
                        case '\u1EDA':
                        // Ã¡Â»Å¡  [LATIN CAPITAL LETTER O WITH HORN AND ACUTE]
                        case '\u1EDC':
                        // Ã¡Â»Å“  [LATIN CAPITAL LETTER O WITH HORN AND GRAVE]
                        case '\u1EDE':
                        // Ã¡Â»Å¾  [LATIN CAPITAL LETTER O WITH HORN AND HOOK ABOVE]
                        case '\u1EE0':
                        // Ã¡Â»Â   [LATIN CAPITAL LETTER O WITH HORN AND TILDE]
                        case '\u1EE2':
                        // Ã¡Â»Â¢  [LATIN CAPITAL LETTER O WITH HORN AND DOT BELOW]
                        case '\u24C4':
                        // Ã¢â€œâ€ž  [CIRCLED LATIN CAPITAL LETTER O]
                        case '\uA74A':
                        // Ãªï¿½Å   [LATIN CAPITAL LETTER O WITH LONG STROKE OVERLAY]
                        case '\uA74C':
                        // Ãªï¿½Å’  [LATIN CAPITAL LETTER O WITH LOOP]
                        case '\uFF2F':  // Ã¯Â¼Â¯  [FULLWIDTH LATIN CAPITAL LETTER O]
                            output[outputPos++] = 'O';
                            break;

                        case '\u00D6':
                            // Ãƒâ€“  [LATIN CAPITAL LETTER O WITH DIAERESIS]
                            output[outputPos++] = 'O';
                            output[outputPos++] = 'e';
                            break;

                        case '\u00F2':
                        // ÃƒÂ²  [LATIN SMALL LETTER O WITH GRAVE]
                        case '\u00F3':
                        // ÃƒÂ³  [LATIN SMALL LETTER O WITH ACUTE]
                        case '\u00F4':
                        // ÃƒÂ´  [LATIN SMALL LETTER O WITH CIRCUMFLEX]
                        case '\u00F5':
                        // ÃƒÂµ  [LATIN SMALL LETTER O WITH TILDE]
                        case '\u00F8':
                        // ÃƒÂ¸  [LATIN SMALL LETTER O WITH STROKE]
                        case '\u014D':
                        // Ã…ï¿½  [LATIN SMALL LETTER O WITH MACRON]
                        case '\u014F':
                        // Ã…ï¿½  [LATIN SMALL LETTER O WITH BREVE]
                        case '\u0151':
                        // Ã…â€˜  [LATIN SMALL LETTER O WITH DOUBLE ACUTE]
                        case '\u01A1':
                        // Ã†Â¡  [LATIN SMALL LETTER O WITH HORN]
                        case '\u01D2':
                        // Ã‡â€™  [LATIN SMALL LETTER O WITH CARON]
                        case '\u01EB':
                        // Ã‡Â«  [LATIN SMALL LETTER O WITH OGONEK]
                        case '\u01ED':
                        // Ã‡Â­  [LATIN SMALL LETTER O WITH OGONEK AND MACRON]
                        case '\u01FF':
                        // Ã‡Â¿  [LATIN SMALL LETTER O WITH STROKE AND ACUTE]
                        case '\u020D':
                        // Ãˆï¿½  [LATIN SMALL LETTER O WITH DOUBLE GRAVE]
                        case '\u020F':
                        // Ãˆï¿½  [LATIN SMALL LETTER O WITH INVERTED BREVE]
                        case '\u022B':
                        // ÃˆÂ«  [LATIN SMALL LETTER O WITH DIAERESIS AND MACRON]
                        case '\u022D':
                        // ÃˆÂ­  [LATIN SMALL LETTER O WITH TILDE AND MACRON]
                        case '\u022F':
                        // ÃˆÂ¯  [LATIN SMALL LETTER O WITH DOT ABOVE]
                        case '\u0231':
                        // ÃˆÂ±  [LATIN SMALL LETTER O WITH DOT ABOVE AND MACRON]
                        case '\u0254':
                        // Ã‰ï¿½?  [LATIN SMALL LETTER OPEN O]
                        case '\u0275':
                        // Ã‰Âµ  [LATIN SMALL LETTER BARRED O]
                        case '\u1D16':
                        // Ã¡Â´â€“  [LATIN SMALL LETTER TOP HALF O]
                        case '\u1D17':
                        // Ã¡Â´â€”  [LATIN SMALL LETTER BOTTOM HALF O]
                        case '\u1D97':
                        // Ã¡Â¶â€”  [LATIN SMALL LETTER OPEN O WITH RETROFLEX HOOK]
                        case '\u1E4D':
                        // Ã¡Â¹ï¿½  [LATIN SMALL LETTER O WITH TILDE AND ACUTE]
                        case '\u1E4F':
                        // Ã¡Â¹ï¿½  [LATIN SMALL LETTER O WITH TILDE AND DIAERESIS]
                        case '\u1E51':
                        // Ã¡Â¹â€˜  [LATIN SMALL LETTER O WITH MACRON AND GRAVE]
                        case '\u1E53':
                        // Ã¡Â¹â€œ  [LATIN SMALL LETTER O WITH MACRON AND ACUTE]
                        case '\u1ECD':
                        // Ã¡Â»ï¿½  [LATIN SMALL LETTER O WITH DOT BELOW]
                        case '\u1ECF':
                        // Ã¡Â»ï¿½  [LATIN SMALL LETTER O WITH HOOK ABOVE]
                        case '\u1ED1':
                        // Ã¡Â»â€˜  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND ACUTE]
                        case '\u1ED3':
                        // Ã¡Â»â€œ  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND GRAVE]
                        case '\u1ED5':
                        // Ã¡Â»â€¢  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND HOOK ABOVE]
                        case '\u1ED7':
                        // Ã¡Â»â€”  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND TILDE]
                        case '\u1ED9':
                        // Ã¡Â»â„¢  [LATIN SMALL LETTER O WITH CIRCUMFLEX AND DOT BELOW]
                        case '\u1EDB':
                        // Ã¡Â»â€º  [LATIN SMALL LETTER O WITH HORN AND ACUTE]
                        case '\u1EDD':
                        // Ã¡Â»ï¿½  [LATIN SMALL LETTER O WITH HORN AND GRAVE]
                        case '\u1EDF':
                        // Ã¡Â»Å¸  [LATIN SMALL LETTER O WITH HORN AND HOOK ABOVE]
                        case '\u1EE1':
                        // Ã¡Â»Â¡  [LATIN SMALL LETTER O WITH HORN AND TILDE]
                        case '\u1EE3':
                        // Ã¡Â»Â£  [LATIN SMALL LETTER O WITH HORN AND DOT BELOW]
                        case '\u2092':
                        // Ã¢â€šâ€™  [LATIN SUBSCRIPT SMALL LETTER O]
                        case '\u24DE':
                        // Ã¢â€œÅ¾  [CIRCLED LATIN SMALL LETTER O]
                        case '\u2C7A':
                        // Ã¢Â±Âº  [LATIN SMALL LETTER O WITH LOW RING INSIDE]
                        case '\uA74B':
                        // Ãªï¿½â€¹  [LATIN SMALL LETTER O WITH LONG STROKE OVERLAY]
                        case '\uA74D':
                        // Ãªï¿½ï¿½  [LATIN SMALL LETTER O WITH LOOP]
                        case '\uFF4F':  // Ã¯Â½ï¿½  [FULLWIDTH LATIN SMALL LETTER O]
                            output[outputPos++] = 'o';
                            break;

                        case '\u00F6':
                            // ÃƒÂ¶  [LATIN SMALL LETTER O WITH DIAERESIS]
                            output[outputPos++] = 'o';
                            output[outputPos++] = 'e';
                            break;

                        case '\u0152':
                        // Ã…â€™  [LATIN CAPITAL LIGATURE OE]
                        case '\u0276':  // Ã‰Â¶  [LATIN LETTER SMALL CAPITAL OE]
                            output[outputPos++] = 'O';
                            output[outputPos++] = 'E';
                            break;

                        case '\uA74E':  // Ãªï¿½Å½  [LATIN CAPITAL LETTER OO]
                            output[outputPos++] = 'O';
                            output[outputPos++] = 'O';
                            break;

                        case '\u0222':
                        // ÃˆÂ¢  http://en.wikipedia.org/wiki/OU  [LATIN CAPITAL LETTER OU]
                        case '\u1D15':  // Ã¡Â´â€¢  [LATIN LETTER SMALL CAPITAL OU]
                            output[outputPos++] = 'O';
                            output[outputPos++] = 'U';
                            break;

                        case '\u24AA':  // Ã¢â€™Âª  [PARENTHESIZED LATIN SMALL LETTER O]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'o';
                            output[outputPos++] = ')';
                            break;

                        case '\u0153':
                        // Ã…â€œ  [LATIN SMALL LIGATURE OE]
                        case '\u1D14':  // Ã¡Â´ï¿½?  [LATIN SMALL LETTER TURNED OE]
                            output[outputPos++] = 'o';
                            output[outputPos++] = 'e';
                            break;

                        case '\uA74F':  // Ãªï¿½ï¿½  [LATIN SMALL LETTER OO]
                            output[outputPos++] = 'o';
                            output[outputPos++] = 'o';
                            break;

                        case '\u0223':  // ÃˆÂ£  http://en.wikipedia.org/wiki/OU  [LATIN SMALL LETTER OU]
                            output[outputPos++] = 'o';
                            output[outputPos++] = 'u';
                            break;

                        case '\u01A4':
                        // Ã†Â¤  [LATIN CAPITAL LETTER P WITH HOOK]
                        case '\u1D18':
                        // Ã¡Â´Ëœ  [LATIN LETTER SMALL CAPITAL P]
                        case '\u1E54':
                        // Ã¡Â¹ï¿½?  [LATIN CAPITAL LETTER P WITH ACUTE]
                        case '\u1E56':
                        // Ã¡Â¹â€“  [LATIN CAPITAL LETTER P WITH DOT ABOVE]
                        case '\u24C5':
                        // Ã¢â€œâ€¦  [CIRCLED LATIN CAPITAL LETTER P]
                        case '\u2C63':
                        // Ã¢Â±Â£  [LATIN CAPITAL LETTER P WITH STROKE]
                        case '\uA750':
                        // Ãªï¿½ï¿½  [LATIN CAPITAL LETTER P WITH STROKE THROUGH DESCENDER]
                        case '\uA752':
                        // Ãªï¿½â€™  [LATIN CAPITAL LETTER P WITH FLOURISH]
                        case '\uA754':
                        // Ãªï¿½ï¿½?  [LATIN CAPITAL LETTER P WITH SQUIRREL TAIL]
                        case '\uFF30':  // Ã¯Â¼Â°  [FULLWIDTH LATIN CAPITAL LETTER P]
                            output[outputPos++] = 'P';
                            break;

                        case '\u01A5':
                        // Ã†Â¥  [LATIN SMALL LETTER P WITH HOOK]
                        case '\u1D71':
                        // Ã¡ÂµÂ±  [LATIN SMALL LETTER P WITH MIDDLE TILDE]
                        case '\u1D7D':
                        // Ã¡ÂµÂ½  [LATIN SMALL LETTER P WITH STROKE]
                        case '\u1D88':
                        // Ã¡Â¶Ë†  [LATIN SMALL LETTER P WITH PALATAL HOOK]
                        case '\u1E55':
                        // Ã¡Â¹â€¢  [LATIN SMALL LETTER P WITH ACUTE]
                        case '\u1E57':
                        // Ã¡Â¹â€”  [LATIN SMALL LETTER P WITH DOT ABOVE]
                        case '\u24DF':
                        // Ã¢â€œÅ¸  [CIRCLED LATIN SMALL LETTER P]
                        case '\uA751':
                        // Ãªï¿½â€˜  [LATIN SMALL LETTER P WITH STROKE THROUGH DESCENDER]
                        case '\uA753':
                        // Ãªï¿½â€œ  [LATIN SMALL LETTER P WITH FLOURISH]
                        case '\uA755':
                        // Ãªï¿½â€¢  [LATIN SMALL LETTER P WITH SQUIRREL TAIL]
                        case '\uA7FC':
                        // ÃªÅ¸Â¼  [LATIN EPIGRAPHIC LETTER REVERSED P]
                        case '\uFF50':  // Ã¯Â½ï¿½  [FULLWIDTH LATIN SMALL LETTER P]
                            output[outputPos++] = 'p';
                            break;

                        case '\u24AB':  // Ã¢â€™Â«  [PARENTHESIZED LATIN SMALL LETTER P]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'p';
                            output[outputPos++] = ')';
                            break;

                        case '\u024A':
                        // Ã‰Å   [LATIN CAPITAL LETTER SMALL Q WITH HOOK TAIL]
                        case '\u24C6':
                        // Ã¢â€œâ€   [CIRCLED LATIN CAPITAL LETTER Q]
                        case '\uA756':
                        // Ãªï¿½â€“  [LATIN CAPITAL LETTER Q WITH STROKE THROUGH DESCENDER]
                        case '\uA758':
                        // Ãªï¿½Ëœ  [LATIN CAPITAL LETTER Q WITH DIAGONAL STROKE]
                        case '\uFF31':  // Ã¯Â¼Â±  [FULLWIDTH LATIN CAPITAL LETTER Q]
                            output[outputPos++] = 'Q';
                            break;

                        case '\u0138':
                        // Ã„Â¸  http://en.wikipedia.org/wiki/Kra_(letter)  [LATIN SMALL LETTER KRA]
                        case '\u024B':
                        // Ã‰â€¹  [LATIN SMALL LETTER Q WITH HOOK TAIL]
                        case '\u02A0':
                        // ÃŠÂ   [LATIN SMALL LETTER Q WITH HOOK]
                        case '\u24E0':
                        // Ã¢â€œÂ   [CIRCLED LATIN SMALL LETTER Q]
                        case '\uA757':
                        // Ãªï¿½â€”  [LATIN SMALL LETTER Q WITH STROKE THROUGH DESCENDER]
                        case '\uA759':
                        // Ãªï¿½â„¢  [LATIN SMALL LETTER Q WITH DIAGONAL STROKE]
                        case '\uFF51':  // Ã¯Â½â€˜  [FULLWIDTH LATIN SMALL LETTER Q]
                            output[outputPos++] = 'q';
                            break;

                        case '\u24AC':  // Ã¢â€™Â¬  [PARENTHESIZED LATIN SMALL LETTER Q]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'q';
                            output[outputPos++] = ')';
                            break;

                        case '\u0239':  // ÃˆÂ¹  [LATIN SMALL LETTER QP DIGRAPH]
                            output[outputPos++] = 'q';
                            output[outputPos++] = 'p';
                            break;

                        case '\u0154':
                        // Ã…ï¿½?  [LATIN CAPITAL LETTER R WITH ACUTE]
                        case '\u0156':
                        // Ã…â€“  [LATIN CAPITAL LETTER R WITH CEDILLA]
                        case '\u0158':
                        // Ã…Ëœ  [LATIN CAPITAL LETTER R WITH CARON]
                        case '\u0210':
                        // Ãˆâ€™  [LATIN CAPITAL LETTER R WITH DOUBLE GRAVE]
                        case '\u0212':
                        // Ãˆâ€™  [LATIN CAPITAL LETTER R WITH INVERTED BREVE]
                        case '\u024C':
                        // Ã‰Å’  [LATIN CAPITAL LETTER R WITH STROKE]
                        case '\u0280':
                        // ÃŠâ‚¬  [LATIN LETTER SMALL CAPITAL R]
                        case '\u0281':
                        // ÃŠï¿½  [LATIN LETTER SMALL CAPITAL INVERTED R]
                        case '\u1D19':
                        // Ã¡Â´â„¢  [LATIN LETTER SMALL CAPITAL REVERSED R]
                        case '\u1D1A':
                        // Ã¡Â´Å¡  [LATIN LETTER SMALL CAPITAL TURNED R]
                        case '\u1E58':
                        // Ã¡Â¹Ëœ  [LATIN CAPITAL LETTER R WITH DOT ABOVE]
                        case '\u1E5A':
                        // Ã¡Â¹Å¡  [LATIN CAPITAL LETTER R WITH DOT BELOW]
                        case '\u1E5C':
                        // Ã¡Â¹Å“  [LATIN CAPITAL LETTER R WITH DOT BELOW AND MACRON]
                        case '\u1E5E':
                        // Ã¡Â¹Å¾  [LATIN CAPITAL LETTER R WITH LINE BELOW]
                        case '\u24C7':
                        // Ã¢â€œâ€¡  [CIRCLED LATIN CAPITAL LETTER R]
                        case '\u2C64':
                        // Ã¢Â±Â¤  [LATIN CAPITAL LETTER R WITH TAIL]
                        case '\uA75A':
                        // Ãªï¿½Å¡  [LATIN CAPITAL LETTER R ROTUNDA]
                        case '\uA782':
                        // ÃªÅ¾â€š  [LATIN CAPITAL LETTER INSULAR R]
                        case '\uFF32':  // Ã¯Â¼Â²  [FULLWIDTH LATIN CAPITAL LETTER R]
                            output[outputPos++] = 'R';
                            break;

                        case '\u0155':
                        // Ã…â€¢  [LATIN SMALL LETTER R WITH ACUTE]
                        case '\u0157':
                        // Ã…â€”  [LATIN SMALL LETTER R WITH CEDILLA]
                        case '\u0159':
                        // Ã…â„¢  [LATIN SMALL LETTER R WITH CARON]
                        case '\u0211':
                        // Ãˆâ€˜  [LATIN SMALL LETTER R WITH DOUBLE GRAVE]
                        case '\u0213':
                        // Ãˆâ€œ  [LATIN SMALL LETTER R WITH INVERTED BREVE]
                        case '\u024D':
                        // Ã‰ï¿½  [LATIN SMALL LETTER R WITH STROKE]
                        case '\u027C':
                        // Ã‰Â¼  [LATIN SMALL LETTER R WITH LONG LEG]
                        case '\u027D':
                        // Ã‰Â½  [LATIN SMALL LETTER R WITH TAIL]
                        case '\u027E':
                        // Ã‰Â¾  [LATIN SMALL LETTER R WITH FISHHOOK]
                        case '\u027F':
                        // Ã‰Â¿  [LATIN SMALL LETTER REVERSED R WITH FISHHOOK]
                        case '\u1D63':
                        // Ã¡ÂµÂ£  [LATIN SUBSCRIPT SMALL LETTER R]
                        case '\u1D72':
                        // Ã¡ÂµÂ²  [LATIN SMALL LETTER R WITH MIDDLE TILDE]
                        case '\u1D73':
                        // Ã¡ÂµÂ³  [LATIN SMALL LETTER R WITH FISHHOOK AND MIDDLE TILDE]
                        case '\u1D89':
                        // Ã¡Â¶â€°  [LATIN SMALL LETTER R WITH PALATAL HOOK]
                        case '\u1E59':
                        // Ã¡Â¹â„¢  [LATIN SMALL LETTER R WITH DOT ABOVE]
                        case '\u1E5B':
                        // Ã¡Â¹â€º  [LATIN SMALL LETTER R WITH DOT BELOW]
                        case '\u1E5D':
                        // Ã¡Â¹ï¿½  [LATIN SMALL LETTER R WITH DOT BELOW AND MACRON]
                        case '\u1E5F':
                        // Ã¡Â¹Å¸  [LATIN SMALL LETTER R WITH LINE BELOW]
                        case '\u24E1':
                        // Ã¢â€œÂ¡  [CIRCLED LATIN SMALL LETTER R]
                        case '\uA75B':
                        // Ãªï¿½â€º  [LATIN SMALL LETTER R ROTUNDA]
                        case '\uA783':
                        // ÃªÅ¾Æ’  [LATIN SMALL LETTER INSULAR R]
                        case '\uFF52':  // Ã¯Â½â€™  [FULLWIDTH LATIN SMALL LETTER R]
                            output[outputPos++] = 'r';
                            break;

                        case '\u24AD':  // Ã¢â€™Â­  [PARENTHESIZED LATIN SMALL LETTER R]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'r';
                            output[outputPos++] = ')';
                            break;

                        case '\u015A':
                        // Ã…Å¡  [LATIN CAPITAL LETTER S WITH ACUTE]
                        case '\u015C':
                        // Ã…Å“  [LATIN CAPITAL LETTER S WITH CIRCUMFLEX]
                        case '\u015E':
                        // Ã…Å¾  [LATIN CAPITAL LETTER S WITH CEDILLA]
                        case '\u0160':
                        // Ã…Â   [LATIN CAPITAL LETTER S WITH CARON]
                        case '\u0218':
                        // ÃˆËœ  [LATIN CAPITAL LETTER S WITH COMMA BELOW]
                        case '\u1E60':
                        // Ã¡Â¹Â   [LATIN CAPITAL LETTER S WITH DOT ABOVE]
                        case '\u1E62':
                        // Ã¡Â¹Â¢  [LATIN CAPITAL LETTER S WITH DOT BELOW]
                        case '\u1E64':
                        // Ã¡Â¹Â¤  [LATIN CAPITAL LETTER S WITH ACUTE AND DOT ABOVE]
                        case '\u1E66':
                        // Ã¡Â¹Â¦  [LATIN CAPITAL LETTER S WITH CARON AND DOT ABOVE]
                        case '\u1E68':
                        // Ã¡Â¹Â¨  [LATIN CAPITAL LETTER S WITH DOT BELOW AND DOT ABOVE]
                        case '\u24C8':
                        // Ã¢â€œË†  [CIRCLED LATIN CAPITAL LETTER S]
                        case '\uA731':
                        // ÃªÅ“Â±  [LATIN LETTER SMALL CAPITAL S]
                        case '\uA785':
                        // ÃªÅ¾â€¦  [LATIN SMALL LETTER INSULAR S]
                        case '\uFF33':  // Ã¯Â¼Â³  [FULLWIDTH LATIN CAPITAL LETTER S]
                            output[outputPos++] = 'S';
                            break;

                        case '\u015B':
                        // Ã…â€º  [LATIN SMALL LETTER S WITH ACUTE]
                        case '\u015D':
                        // Ã…ï¿½  [LATIN SMALL LETTER S WITH CIRCUMFLEX]
                        case '\u015F':
                        // Ã…Å¸  [LATIN SMALL LETTER S WITH CEDILLA]
                        case '\u0161':
                        // Ã…Â¡  [LATIN SMALL LETTER S WITH CARON]
                        case '\u017F':
                        // Ã…Â¿  http://en.wikipedia.org/wiki/Long_S  [LATIN SMALL LETTER LONG S]
                        case '\u0219':
                        // Ãˆâ„¢  [LATIN SMALL LETTER S WITH COMMA BELOW]
                        case '\u023F':
                        // ÃˆÂ¿  [LATIN SMALL LETTER S WITH SWASH TAIL]
                        case '\u0282':
                        // ÃŠâ€š  [LATIN SMALL LETTER S WITH HOOK]
                        case '\u1D74':
                        // Ã¡ÂµÂ´  [LATIN SMALL LETTER S WITH MIDDLE TILDE]
                        case '\u1D8A':
                        // Ã¡Â¶Å   [LATIN SMALL LETTER S WITH PALATAL HOOK]
                        case '\u1E61':
                        // Ã¡Â¹Â¡  [LATIN SMALL LETTER S WITH DOT ABOVE]
                        case '\u1E63':
                        // Ã¡Â¹Â£  [LATIN SMALL LETTER S WITH DOT BELOW]
                        case '\u1E65':
                        // Ã¡Â¹Â¥  [LATIN SMALL LETTER S WITH ACUTE AND DOT ABOVE]
                        case '\u1E67':
                        // Ã¡Â¹Â§  [LATIN SMALL LETTER S WITH CARON AND DOT ABOVE]
                        case '\u1E69':
                        // Ã¡Â¹Â©  [LATIN SMALL LETTER S WITH DOT BELOW AND DOT ABOVE]
                        case '\u1E9C':
                        // Ã¡ÂºÅ“  [LATIN SMALL LETTER LONG S WITH DIAGONAL STROKE]
                        case '\u1E9D':
                        // Ã¡Âºï¿½  [LATIN SMALL LETTER LONG S WITH HIGH STROKE]
                        case '\u24E2':
                        // Ã¢â€œÂ¢  [CIRCLED LATIN SMALL LETTER S]
                        case '\uA784':
                        // ÃªÅ¾â€ž  [LATIN CAPITAL LETTER INSULAR S]
                        case '\uFF53':  // Ã¯Â½â€œ  [FULLWIDTH LATIN SMALL LETTER S]
                            output[outputPos++] = 's';
                            break;

                        case '\u1E9E':  // Ã¡ÂºÅ¾  [LATIN CAPITAL LETTER SHARP S]
                            output[outputPos++] = 'S';
                            output[outputPos++] = 'S';
                            break;

                        case '\u24AE':  // Ã¢â€™Â®  [PARENTHESIZED LATIN SMALL LETTER S]
                            output[outputPos++] = '(';
                            output[outputPos++] = 's';
                            output[outputPos++] = ')';
                            break;

                        case '\u00DF':  // ÃƒÅ¸  [LATIN SMALL LETTER SHARP S]
                            output[outputPos++] = 's';
                            output[outputPos++] = 's';
                            break;

                        case '\uFB06':  // Ã¯Â¬â€   [LATIN SMALL LIGATURE ST]
                            output[outputPos++] = 's';
                            output[outputPos++] = 't';
                            break;

                        case '\u0162':
                        // Ã…Â¢  [LATIN CAPITAL LETTER T WITH CEDILLA]
                        case '\u0164':
                        // Ã…Â¤  [LATIN CAPITAL LETTER T WITH CARON]
                        case '\u0166':
                        // Ã…Â¦  [LATIN CAPITAL LETTER T WITH STROKE]
                        case '\u01AC':
                        // Ã†Â¬  [LATIN CAPITAL LETTER T WITH HOOK]
                        case '\u01AE':
                        // Ã†Â®  [LATIN CAPITAL LETTER T WITH RETROFLEX HOOK]
                        case '\u021A':
                        // ÃˆÅ¡  [LATIN CAPITAL LETTER T WITH COMMA BELOW]
                        case '\u023E':
                        // ÃˆÂ¾  [LATIN CAPITAL LETTER T WITH DIAGONAL STROKE]
                        case '\u1D1B':
                        // Ã¡Â´â€º  [LATIN LETTER SMALL CAPITAL T]
                        case '\u1E6A':
                        // Ã¡Â¹Âª  [LATIN CAPITAL LETTER T WITH DOT ABOVE]
                        case '\u1E6C':
                        // Ã¡Â¹Â¬  [LATIN CAPITAL LETTER T WITH DOT BELOW]
                        case '\u1E6E':
                        // Ã¡Â¹Â®  [LATIN CAPITAL LETTER T WITH LINE BELOW]
                        case '\u1E70':
                        // Ã¡Â¹Â°  [LATIN CAPITAL LETTER T WITH CIRCUMFLEX BELOW]
                        case '\u24C9':
                        // Ã¢â€œâ€°  [CIRCLED LATIN CAPITAL LETTER T]
                        case '\uA786':
                        // ÃªÅ¾â€   [LATIN CAPITAL LETTER INSULAR T]
                        case '\uFF34':  // Ã¯Â¼Â´  [FULLWIDTH LATIN CAPITAL LETTER T]
                            output[outputPos++] = 'T';
                            break;

                        case '\u0163':
                        // Ã…Â£  [LATIN SMALL LETTER T WITH CEDILLA]
                        case '\u0165':
                        // Ã…Â¥  [LATIN SMALL LETTER T WITH CARON]
                        case '\u0167':
                        // Ã…Â§  [LATIN SMALL LETTER T WITH STROKE]
                        case '\u01AB':
                        // Ã†Â«  [LATIN SMALL LETTER T WITH PALATAL HOOK]
                        case '\u01AD':
                        // Ã†Â­  [LATIN SMALL LETTER T WITH HOOK]
                        case '\u021B':
                        // Ãˆâ€º  [LATIN SMALL LETTER T WITH COMMA BELOW]
                        case '\u0236':
                        // ÃˆÂ¶  [LATIN SMALL LETTER T WITH CURL]
                        case '\u0287':
                        // ÃŠâ€¡  [LATIN SMALL LETTER TURNED T]
                        case '\u0288':
                        // ÃŠË†  [LATIN SMALL LETTER T WITH RETROFLEX HOOK]
                        case '\u1D75':
                        // Ã¡ÂµÂµ  [LATIN SMALL LETTER T WITH MIDDLE TILDE]
                        case '\u1E6B':
                        // Ã¡Â¹Â«  [LATIN SMALL LETTER T WITH DOT ABOVE]
                        case '\u1E6D':
                        // Ã¡Â¹Â­  [LATIN SMALL LETTER T WITH DOT BELOW]
                        case '\u1E6F':
                        // Ã¡Â¹Â¯  [LATIN SMALL LETTER T WITH LINE BELOW]
                        case '\u1E71':
                        // Ã¡Â¹Â±  [LATIN SMALL LETTER T WITH CIRCUMFLEX BELOW]
                        case '\u1E97':
                        // Ã¡Âºâ€”  [LATIN SMALL LETTER T WITH DIAERESIS]
                        case '\u24E3':
                        // Ã¢â€œÂ£  [CIRCLED LATIN SMALL LETTER T]
                        case '\u2C66':
                        // Ã¢Â±Â¦  [LATIN SMALL LETTER T WITH DIAGONAL STROKE]
                        case '\uFF54':  // Ã¯Â½ï¿½?  [FULLWIDTH LATIN SMALL LETTER T]
                            output[outputPos++] = 't';
                            break;

                        case '\u00DE':
                        // ÃƒÅ¾  [LATIN CAPITAL LETTER THORN]
                        case '\uA766':  // Ãªï¿½Â¦  [LATIN CAPITAL LETTER THORN WITH STROKE THROUGH DESCENDER]
                            output[outputPos++] = 'T';
                            output[outputPos++] = 'H';
                            break;

                        case '\uA728':  // ÃªÅ“Â¨  [LATIN CAPITAL LETTER TZ]
                            output[outputPos++] = 'T';
                            output[outputPos++] = 'Z';
                            break;

                        case '\u24AF':  // Ã¢â€™Â¯  [PARENTHESIZED LATIN SMALL LETTER T]
                            output[outputPos++] = '(';
                            output[outputPos++] = 't';
                            output[outputPos++] = ')';
                            break;

                        case '\u02A8':  // ÃŠÂ¨  [LATIN SMALL LETTER TC DIGRAPH WITH CURL]
                            output[outputPos++] = 't';
                            output[outputPos++] = 'c';
                            break;

                        case '\u00FE':
                        // ÃƒÂ¾  [LATIN SMALL LETTER THORN]
                        case '\u1D7A':
                        // Ã¡ÂµÂº  [LATIN SMALL LETTER TH WITH STRIKETHROUGH]
                        case '\uA767':  // Ãªï¿½Â§  [LATIN SMALL LETTER THORN WITH STROKE THROUGH DESCENDER]
                            output[outputPos++] = 't';
                            output[outputPos++] = 'h';
                            break;

                        case '\u02A6':  // ÃŠÂ¦  [LATIN SMALL LETTER TS DIGRAPH]
                            output[outputPos++] = 't';
                            output[outputPos++] = 's';
                            break;

                        case '\uA729':  // ÃªÅ“Â©  [LATIN SMALL LETTER TZ]
                            output[outputPos++] = 't';
                            output[outputPos++] = 'z';
                            break;

                        case '\u00D9':
                        // Ãƒâ„¢  [LATIN CAPITAL LETTER U WITH GRAVE]
                        case '\u00DA':
                        // ÃƒÅ¡  [LATIN CAPITAL LETTER U WITH ACUTE]
                        case '\u00DB':
                        // Ãƒâ€º  [LATIN CAPITAL LETTER U WITH CIRCUMFLEX]
                        case '\u0168':
                        // Ã…Â¨  [LATIN CAPITAL LETTER U WITH TILDE]
                        case '\u016A':
                        // Ã…Âª  [LATIN CAPITAL LETTER U WITH MACRON]
                        case '\u016C':
                        // Ã…Â¬  [LATIN CAPITAL LETTER U WITH BREVE]
                        case '\u016E':
                        // Ã…Â®  [LATIN CAPITAL LETTER U WITH RING ABOVE]
                        case '\u0170':
                        // Ã…Â°  [LATIN CAPITAL LETTER U WITH DOUBLE ACUTE]
                        case '\u0172':
                        // Ã…Â²  [LATIN CAPITAL LETTER U WITH OGONEK]
                        case '\u01AF':
                        // Ã†Â¯  [LATIN CAPITAL LETTER U WITH HORN]
                        case '\u01D3':
                        // Ã‡â€œ  [LATIN CAPITAL LETTER U WITH CARON]
                        case '\u01D5':
                        // Ã‡â€¢  [LATIN CAPITAL LETTER U WITH DIAERESIS AND MACRON]
                        case '\u01D7':
                        // Ã‡â€”  [LATIN CAPITAL LETTER U WITH DIAERESIS AND ACUTE]
                        case '\u01D9':
                        // Ã‡â„¢  [LATIN CAPITAL LETTER U WITH DIAERESIS AND CARON]
                        case '\u01DB':
                        // Ã‡â€º  [LATIN CAPITAL LETTER U WITH DIAERESIS AND GRAVE]
                        case '\u0214':
                        // Ãˆï¿½?  [LATIN CAPITAL LETTER U WITH DOUBLE GRAVE]
                        case '\u0216':
                        // Ãˆâ€“  [LATIN CAPITAL LETTER U WITH INVERTED BREVE]
                        case '\u0244':
                        // Ã‰â€ž  [LATIN CAPITAL LETTER U BAR]
                        case '\u1D1C':
                        // Ã¡Â´Å“  [LATIN LETTER SMALL CAPITAL U]
                        case '\u1D7E':
                        // Ã¡ÂµÂ¾  [LATIN SMALL CAPITAL LETTER U WITH STROKE]
                        case '\u1E72':
                        // Ã¡Â¹Â²  [LATIN CAPITAL LETTER U WITH DIAERESIS BELOW]
                        case '\u1E74':
                        // Ã¡Â¹Â´  [LATIN CAPITAL LETTER U WITH TILDE BELOW]
                        case '\u1E76':
                        // Ã¡Â¹Â¶  [LATIN CAPITAL LETTER U WITH CIRCUMFLEX BELOW]
                        case '\u1E78':
                        // Ã¡Â¹Â¸  [LATIN CAPITAL LETTER U WITH TILDE AND ACUTE]
                        case '\u1E7A':
                        // Ã¡Â¹Âº  [LATIN CAPITAL LETTER U WITH MACRON AND DIAERESIS]
                        case '\u1EE4':
                        // Ã¡Â»Â¤  [LATIN CAPITAL LETTER U WITH DOT BELOW]
                        case '\u1EE6':
                        // Ã¡Â»Â¦  [LATIN CAPITAL LETTER U WITH HOOK ABOVE]
                        case '\u1EE8':
                        // Ã¡Â»Â¨  [LATIN CAPITAL LETTER U WITH HORN AND ACUTE]
                        case '\u1EEA':
                        // Ã¡Â»Âª  [LATIN CAPITAL LETTER U WITH HORN AND GRAVE]
                        case '\u1EEC':
                        // Ã¡Â»Â¬  [LATIN CAPITAL LETTER U WITH HORN AND HOOK ABOVE]
                        case '\u1EEE':
                        // Ã¡Â»Â®  [LATIN CAPITAL LETTER U WITH HORN AND TILDE]
                        case '\u1EF0':
                        // Ã¡Â»Â°  [LATIN CAPITAL LETTER U WITH HORN AND DOT BELOW]
                        case '\u24CA':
                        // Ã¢â€œÅ   [CIRCLED LATIN CAPITAL LETTER U]
                        case '\uFF35':  // Ã¯Â¼Âµ  [FULLWIDTH LATIN CAPITAL LETTER U]
                            output[outputPos++] = 'U';
                            break;

                        case '\u00DC':
                            // ÃƒÅ“  [LATIN CAPITAL LETTER U WITH DIAERESIS]
                            output[outputPos++] = 'U';
                            output[outputPos++] = 'e';
                            break;

                        case '\u00F9':
                        // ÃƒÂ¹  [LATIN SMALL LETTER U WITH GRAVE]
                        case '\u00FA':
                        // ÃƒÂº  [LATIN SMALL LETTER U WITH ACUTE]
                        case '\u00FB':
                        // ÃƒÂ»  [LATIN SMALL LETTER U WITH CIRCUMFLEX]
                        case '\u0169':
                        // Ã…Â©  [LATIN SMALL LETTER U WITH TILDE]
                        case '\u016B':
                        // Ã…Â«  [LATIN SMALL LETTER U WITH MACRON]
                        case '\u016D':
                        // Ã…Â­  [LATIN SMALL LETTER U WITH BREVE]
                        case '\u016F':
                        // Ã…Â¯  [LATIN SMALL LETTER U WITH RING ABOVE]
                        case '\u0171':
                        // Ã…Â±  [LATIN SMALL LETTER U WITH DOUBLE ACUTE]
                        case '\u0173':
                        // Ã…Â³  [LATIN SMALL LETTER U WITH OGONEK]
                        case '\u01B0':
                        // Ã†Â°  [LATIN SMALL LETTER U WITH HORN]
                        case '\u01D4':
                        // Ã‡ï¿½?  [LATIN SMALL LETTER U WITH CARON]
                        case '\u01D6':
                        // Ã‡â€“  [LATIN SMALL LETTER U WITH DIAERESIS AND MACRON]
                        case '\u01D8':
                        // Ã‡Ëœ  [LATIN SMALL LETTER U WITH DIAERESIS AND ACUTE]
                        case '\u01DA':
                        // Ã‡Å¡  [LATIN SMALL LETTER U WITH DIAERESIS AND CARON]
                        case '\u01DC':
                        // Ã‡Å“  [LATIN SMALL LETTER U WITH DIAERESIS AND GRAVE]
                        case '\u0215':
                        // Ãˆâ€¢  [LATIN SMALL LETTER U WITH DOUBLE GRAVE]
                        case '\u0217':
                        // Ãˆâ€”  [LATIN SMALL LETTER U WITH INVERTED BREVE]
                        case '\u0289':
                        // ÃŠâ€°  [LATIN SMALL LETTER U BAR]
                        case '\u1D64':
                        // Ã¡ÂµÂ¤  [LATIN SUBSCRIPT SMALL LETTER U]
                        case '\u1D99':
                        // Ã¡Â¶â„¢  [LATIN SMALL LETTER U WITH RETROFLEX HOOK]
                        case '\u1E73':
                        // Ã¡Â¹Â³  [LATIN SMALL LETTER U WITH DIAERESIS BELOW]
                        case '\u1E75':
                        // Ã¡Â¹Âµ  [LATIN SMALL LETTER U WITH TILDE BELOW]
                        case '\u1E77':
                        // Ã¡Â¹Â·  [LATIN SMALL LETTER U WITH CIRCUMFLEX BELOW]
                        case '\u1E79':
                        // Ã¡Â¹Â¹  [LATIN SMALL LETTER U WITH TILDE AND ACUTE]
                        case '\u1E7B':
                        // Ã¡Â¹Â»  [LATIN SMALL LETTER U WITH MACRON AND DIAERESIS]
                        case '\u1EE5':
                        // Ã¡Â»Â¥  [LATIN SMALL LETTER U WITH DOT BELOW]
                        case '\u1EE7':
                        // Ã¡Â»Â§  [LATIN SMALL LETTER U WITH HOOK ABOVE]
                        case '\u1EE9':
                        // Ã¡Â»Â©  [LATIN SMALL LETTER U WITH HORN AND ACUTE]
                        case '\u1EEB':
                        // Ã¡Â»Â«  [LATIN SMALL LETTER U WITH HORN AND GRAVE]
                        case '\u1EED':
                        // Ã¡Â»Â­  [LATIN SMALL LETTER U WITH HORN AND HOOK ABOVE]
                        case '\u1EEF':
                        // Ã¡Â»Â¯  [LATIN SMALL LETTER U WITH HORN AND TILDE]
                        case '\u1EF1':
                        // Ã¡Â»Â±  [LATIN SMALL LETTER U WITH HORN AND DOT BELOW]
                        case '\u24E4':
                        // Ã¢â€œÂ¤  [CIRCLED LATIN SMALL LETTER U]
                        case '\uFF55':  // Ã¯Â½â€¢  [FULLWIDTH LATIN SMALL LETTER U]
                            output[outputPos++] = 'u';
                            break;

                        case '\u00FC':
                            // ÃƒÂ¼  [LATIN SMALL LETTER U WITH DIAERESIS]
                            output[outputPos++] = 'u';
                            output[outputPos++] = 'e';
                            break;

                        case '\u24B0':  // Ã¢â€™Â°  [PARENTHESIZED LATIN SMALL LETTER U]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'u';
                            output[outputPos++] = ')';
                            break;

                        case '\u1D6B':  // Ã¡ÂµÂ«  [LATIN SMALL LETTER UE]
                            output[outputPos++] = 'u';
                            output[outputPos++] = 'e';
                            break;

                        case '\u01B2':
                        // Ã†Â²  [LATIN CAPITAL LETTER V WITH HOOK]
                        case '\u0245':
                        // Ã‰â€¦  [LATIN CAPITAL LETTER TURNED V]
                        case '\u1D20':
                        // Ã¡Â´Â   [LATIN LETTER SMALL CAPITAL V]
                        case '\u1E7C':
                        // Ã¡Â¹Â¼  [LATIN CAPITAL LETTER V WITH TILDE]
                        case '\u1E7E':
                        // Ã¡Â¹Â¾  [LATIN CAPITAL LETTER V WITH DOT BELOW]
                        case '\u1EFC':
                        // Ã¡Â»Â¼  [LATIN CAPITAL LETTER MIDDLE-WELSH V]
                        case '\u24CB':
                        // Ã¢â€œâ€¹  [CIRCLED LATIN CAPITAL LETTER V]
                        case '\uA75E':
                        // Ãªï¿½Å¾  [LATIN CAPITAL LETTER V WITH DIAGONAL STROKE]
                        case '\uA768':
                        // Ãªï¿½Â¨  [LATIN CAPITAL LETTER VEND]
                        case '\uFF36':  // Ã¯Â¼Â¶  [FULLWIDTH LATIN CAPITAL LETTER V]
                            output[outputPos++] = 'V';
                            break;

                        case '\u028B':
                        // ÃŠâ€¹  [LATIN SMALL LETTER V WITH HOOK]
                        case '\u028C':
                        // ÃŠÅ’  [LATIN SMALL LETTER TURNED V]
                        case '\u1D65':
                        // Ã¡ÂµÂ¥  [LATIN SUBSCRIPT SMALL LETTER V]
                        case '\u1D8C':
                        // Ã¡Â¶Å’  [LATIN SMALL LETTER V WITH PALATAL HOOK]
                        case '\u1E7D':
                        // Ã¡Â¹Â½  [LATIN SMALL LETTER V WITH TILDE]
                        case '\u1E7F':
                        // Ã¡Â¹Â¿  [LATIN SMALL LETTER V WITH DOT BELOW]
                        case '\u24E5':
                        // Ã¢â€œÂ¥  [CIRCLED LATIN SMALL LETTER V]
                        case '\u2C71':
                        // Ã¢Â±Â±  [LATIN SMALL LETTER V WITH RIGHT HOOK]
                        case '\u2C74':
                        // Ã¢Â±Â´  [LATIN SMALL LETTER V WITH CURL]
                        case '\uA75F':
                        // Ãªï¿½Å¸  [LATIN SMALL LETTER V WITH DIAGONAL STROKE]
                        case '\uFF56':  // Ã¯Â½â€“  [FULLWIDTH LATIN SMALL LETTER V]
                            output[outputPos++] = 'v';
                            break;

                        case '\uA760':  // Ãªï¿½Â   [LATIN CAPITAL LETTER VY]
                            output[outputPos++] = 'V';
                            output[outputPos++] = 'Y';
                            break;

                        case '\u24B1':  // Ã¢â€™Â±  [PARENTHESIZED LATIN SMALL LETTER V]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'v';
                            output[outputPos++] = ')';
                            break;

                        case '\uA761':  // Ãªï¿½Â¡  [LATIN SMALL LETTER VY]
                            output[outputPos++] = 'v';
                            output[outputPos++] = 'y';
                            break;

                        case '\u0174':
                        // Ã…Â´  [LATIN CAPITAL LETTER W WITH CIRCUMFLEX]
                        case '\u01F7':
                        // Ã‡Â·  http://en.wikipedia.org/wiki/Wynn  [LATIN CAPITAL LETTER WYNN]
                        case '\u1D21':
                        // Ã¡Â´Â¡  [LATIN LETTER SMALL CAPITAL W]
                        case '\u1E80':
                        // Ã¡Âºâ‚¬  [LATIN CAPITAL LETTER W WITH GRAVE]
                        case '\u1E82':
                        // Ã¡Âºâ€š  [LATIN CAPITAL LETTER W WITH ACUTE]
                        case '\u1E84':
                        // Ã¡Âºâ€ž  [LATIN CAPITAL LETTER W WITH DIAERESIS]
                        case '\u1E86':
                        // Ã¡Âºâ€   [LATIN CAPITAL LETTER W WITH DOT ABOVE]
                        case '\u1E88':
                        // Ã¡ÂºË†  [LATIN CAPITAL LETTER W WITH DOT BELOW]
                        case '\u24CC':
                        // Ã¢â€œÅ’  [CIRCLED LATIN CAPITAL LETTER W]
                        case '\u2C72':
                        // Ã¢Â±Â²  [LATIN CAPITAL LETTER W WITH HOOK]
                        case '\uFF37':  // Ã¯Â¼Â·  [FULLWIDTH LATIN CAPITAL LETTER W]
                            output[outputPos++] = 'W';
                            break;

                        case '\u0175':
                        // Ã…Âµ  [LATIN SMALL LETTER W WITH CIRCUMFLEX]
                        case '\u01BF':
                        // Ã†Â¿  http://en.wikipedia.org/wiki/Wynn  [LATIN LETTER WYNN]
                        case '\u028D':
                        // ÃŠï¿½  [LATIN SMALL LETTER TURNED W]
                        case '\u1E81':
                        // Ã¡Âºï¿½  [LATIN SMALL LETTER W WITH GRAVE]
                        case '\u1E83':
                        // Ã¡ÂºÆ’  [LATIN SMALL LETTER W WITH ACUTE]
                        case '\u1E85':
                        // Ã¡Âºâ€¦  [LATIN SMALL LETTER W WITH DIAERESIS]
                        case '\u1E87':
                        // Ã¡Âºâ€¡  [LATIN SMALL LETTER W WITH DOT ABOVE]
                        case '\u1E89':
                        // Ã¡Âºâ€°  [LATIN SMALL LETTER W WITH DOT BELOW]
                        case '\u1E98':
                        // Ã¡ÂºËœ  [LATIN SMALL LETTER W WITH RING ABOVE]
                        case '\u24E6':
                        // Ã¢â€œÂ¦  [CIRCLED LATIN SMALL LETTER W]
                        case '\u2C73':
                        // Ã¢Â±Â³  [LATIN SMALL LETTER W WITH HOOK]
                        case '\uFF57':  // Ã¯Â½â€”  [FULLWIDTH LATIN SMALL LETTER W]
                            output[outputPos++] = 'w';
                            break;

                        case '\u24B2':  // Ã¢â€™Â²  [PARENTHESIZED LATIN SMALL LETTER W]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'w';
                            output[outputPos++] = ')';
                            break;

                        case '\u1E8A':
                        // Ã¡ÂºÅ   [LATIN CAPITAL LETTER X WITH DOT ABOVE]
                        case '\u1E8C':
                        // Ã¡ÂºÅ’  [LATIN CAPITAL LETTER X WITH DIAERESIS]
                        case '\u24CD':
                        // Ã¢â€œï¿½  [CIRCLED LATIN CAPITAL LETTER X]
                        case '\uFF38':  // Ã¯Â¼Â¸  [FULLWIDTH LATIN CAPITAL LETTER X]
                            output[outputPos++] = 'X';
                            break;

                        case '\u1D8D':
                        // Ã¡Â¶ï¿½  [LATIN SMALL LETTER X WITH PALATAL HOOK]
                        case '\u1E8B':
                        // Ã¡Âºâ€¹  [LATIN SMALL LETTER X WITH DOT ABOVE]
                        case '\u1E8D':
                        // Ã¡Âºï¿½  [LATIN SMALL LETTER X WITH DIAERESIS]
                        case '\u2093':
                        // Ã¢â€šâ€œ  [LATIN SUBSCRIPT SMALL LETTER X]
                        case '\u24E7':
                        // Ã¢â€œÂ§  [CIRCLED LATIN SMALL LETTER X]
                        case '\uFF58':  // Ã¯Â½Ëœ  [FULLWIDTH LATIN SMALL LETTER X]
                            output[outputPos++] = 'x';
                            break;

                        case '\u24B3':  // Ã¢â€™Â³  [PARENTHESIZED LATIN SMALL LETTER X]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'x';
                            output[outputPos++] = ')';
                            break;

                        case '\u00DD':
                        // Ãƒï¿½  [LATIN CAPITAL LETTER Y WITH ACUTE]
                        case '\u0176':
                        // Ã…Â¶  [LATIN CAPITAL LETTER Y WITH CIRCUMFLEX]
                        case '\u0178':
                        // Ã…Â¸  [LATIN CAPITAL LETTER Y WITH DIAERESIS]
                        case '\u01B3':
                        // Ã†Â³  [LATIN CAPITAL LETTER Y WITH HOOK]
                        case '\u0232':
                        // ÃˆÂ²  [LATIN CAPITAL LETTER Y WITH MACRON]
                        case '\u024E':
                        // Ã‰Å½  [LATIN CAPITAL LETTER Y WITH STROKE]
                        case '\u028F':
                        // ÃŠï¿½  [LATIN LETTER SMALL CAPITAL Y]
                        case '\u1E8E':
                        // Ã¡ÂºÅ½  [LATIN CAPITAL LETTER Y WITH DOT ABOVE]
                        case '\u1EF2':
                        // Ã¡Â»Â²  [LATIN CAPITAL LETTER Y WITH GRAVE]
                        case '\u1EF4':
                        // Ã¡Â»Â´  [LATIN CAPITAL LETTER Y WITH DOT BELOW]
                        case '\u1EF6':
                        // Ã¡Â»Â¶  [LATIN CAPITAL LETTER Y WITH HOOK ABOVE]
                        case '\u1EF8':
                        // Ã¡Â»Â¸  [LATIN CAPITAL LETTER Y WITH TILDE]
                        case '\u1EFE':
                        // Ã¡Â»Â¾  [LATIN CAPITAL LETTER Y WITH LOOP]
                        case '\u24CE':
                        // Ã¢â€œÅ½  [CIRCLED LATIN CAPITAL LETTER Y]
                        case '\uFF39':  // Ã¯Â¼Â¹  [FULLWIDTH LATIN CAPITAL LETTER Y]
                            output[outputPos++] = 'Y';
                            break;

                        case '\u00FD':
                        // ÃƒÂ½  [LATIN SMALL LETTER Y WITH ACUTE]
                        case '\u00FF':
                        // ÃƒÂ¿  [LATIN SMALL LETTER Y WITH DIAERESIS]
                        case '\u0177':
                        // Ã…Â·  [LATIN SMALL LETTER Y WITH CIRCUMFLEX]
                        case '\u01B4':
                        // Ã†Â´  [LATIN SMALL LETTER Y WITH HOOK]
                        case '\u0233':
                        // ÃˆÂ³  [LATIN SMALL LETTER Y WITH MACRON]
                        case '\u024F':
                        // Ã‰ï¿½  [LATIN SMALL LETTER Y WITH STROKE]
                        case '\u028E':
                        // ÃŠÅ½  [LATIN SMALL LETTER TURNED Y]
                        case '\u1E8F':
                        // Ã¡Âºï¿½  [LATIN SMALL LETTER Y WITH DOT ABOVE]
                        case '\u1E99':
                        // Ã¡Âºâ„¢  [LATIN SMALL LETTER Y WITH RING ABOVE]
                        case '\u1EF3':
                        // Ã¡Â»Â³  [LATIN SMALL LETTER Y WITH GRAVE]
                        case '\u1EF5':
                        // Ã¡Â»Âµ  [LATIN SMALL LETTER Y WITH DOT BELOW]
                        case '\u1EF7':
                        // Ã¡Â»Â·  [LATIN SMALL LETTER Y WITH HOOK ABOVE]
                        case '\u1EF9':
                        // Ã¡Â»Â¹  [LATIN SMALL LETTER Y WITH TILDE]
                        case '\u1EFF':
                        // Ã¡Â»Â¿  [LATIN SMALL LETTER Y WITH LOOP]
                        case '\u24E8':
                        // Ã¢â€œÂ¨  [CIRCLED LATIN SMALL LETTER Y]
                        case '\uFF59':  // Ã¯Â½â„¢  [FULLWIDTH LATIN SMALL LETTER Y]
                            output[outputPos++] = 'y';
                            break;

                        case '\u24B4':  // Ã¢â€™Â´  [PARENTHESIZED LATIN SMALL LETTER Y]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'y';
                            output[outputPos++] = ')';
                            break;

                        case '\u0179':
                        // Ã…Â¹  [LATIN CAPITAL LETTER Z WITH ACUTE]
                        case '\u017B':
                        // Ã…Â»  [LATIN CAPITAL LETTER Z WITH DOT ABOVE]
                        case '\u017D':
                        // Ã…Â½  [LATIN CAPITAL LETTER Z WITH CARON]
                        case '\u01B5':
                        // Ã†Âµ  [LATIN CAPITAL LETTER Z WITH STROKE]
                        case '\u021C':
                        // ÃˆÅ“  http://en.wikipedia.org/wiki/Yogh  [LATIN CAPITAL LETTER YOGH]
                        case '\u0224':
                        // ÃˆÂ¤  [LATIN CAPITAL LETTER Z WITH HOOK]
                        case '\u1D22':
                        // Ã¡Â´Â¢  [LATIN LETTER SMALL CAPITAL Z]
                        case '\u1E90':
                        // Ã¡Âºï¿½  [LATIN CAPITAL LETTER Z WITH CIRCUMFLEX]
                        case '\u1E92':
                        // Ã¡Âºâ€™  [LATIN CAPITAL LETTER Z WITH DOT BELOW]
                        case '\u1E94':
                        // Ã¡Âºï¿½?  [LATIN CAPITAL LETTER Z WITH LINE BELOW]
                        case '\u24CF':
                        // Ã¢â€œï¿½  [CIRCLED LATIN CAPITAL LETTER Z]
                        case '\u2C6B':
                        // Ã¢Â±Â«  [LATIN CAPITAL LETTER Z WITH DESCENDER]
                        case '\uA762':
                        // Ãªï¿½Â¢  [LATIN CAPITAL LETTER VISIGOTHIC Z]
                        case '\uFF3A':  // Ã¯Â¼Âº  [FULLWIDTH LATIN CAPITAL LETTER Z]
                            output[outputPos++] = 'Z';
                            break;

                        case '\u017A':
                        // Ã…Âº  [LATIN SMALL LETTER Z WITH ACUTE]
                        case '\u017C':
                        // Ã…Â¼  [LATIN SMALL LETTER Z WITH DOT ABOVE]
                        case '\u017E':
                        // Ã…Â¾  [LATIN SMALL LETTER Z WITH CARON]
                        case '\u01B6':
                        // Ã†Â¶  [LATIN SMALL LETTER Z WITH STROKE]
                        case '\u021D':
                        // Ãˆï¿½  http://en.wikipedia.org/wiki/Yogh  [LATIN SMALL LETTER YOGH]
                        case '\u0225':
                        // ÃˆÂ¥  [LATIN SMALL LETTER Z WITH HOOK]
                        case '\u0240':
                        // Ã‰â‚¬  [LATIN SMALL LETTER Z WITH SWASH TAIL]
                        case '\u0290':
                        // ÃŠï¿½  [LATIN SMALL LETTER Z WITH RETROFLEX HOOK]
                        case '\u0291':
                        // ÃŠâ€˜  [LATIN SMALL LETTER Z WITH CURL]
                        case '\u1D76':
                        // Ã¡ÂµÂ¶  [LATIN SMALL LETTER Z WITH MIDDLE TILDE]
                        case '\u1D8E':
                        // Ã¡Â¶Å½  [LATIN SMALL LETTER Z WITH PALATAL HOOK]
                        case '\u1E91':
                        // Ã¡Âºâ€˜  [LATIN SMALL LETTER Z WITH CIRCUMFLEX]
                        case '\u1E93':
                        // Ã¡Âºâ€œ  [LATIN SMALL LETTER Z WITH DOT BELOW]
                        case '\u1E95':
                        // Ã¡Âºâ€¢  [LATIN SMALL LETTER Z WITH LINE BELOW]
                        case '\u24E9':
                        // Ã¢â€œÂ©  [CIRCLED LATIN SMALL LETTER Z]
                        case '\u2C6C':
                        // Ã¢Â±Â¬  [LATIN SMALL LETTER Z WITH DESCENDER]
                        case '\uA763':
                        // Ãªï¿½Â£  [LATIN SMALL LETTER VISIGOTHIC Z]
                        case '\uFF5A':  // Ã¯Â½Å¡  [FULLWIDTH LATIN SMALL LETTER Z]
                            output[outputPos++] = 'z';
                            break;

                        case '\u24B5':  // Ã¢â€™Âµ  [PARENTHESIZED LATIN SMALL LETTER Z]
                            output[outputPos++] = '(';
                            output[outputPos++] = 'z';
                            output[outputPos++] = ')';
                            break;

                        case '\u2070':
                        // Ã¢ï¿½Â°  [SUPERSCRIPT ZERO]
                        case '\u2080':
                        // Ã¢â€šâ‚¬  [SUBSCRIPT ZERO]
                        case '\u24EA':
                        // Ã¢â€œÂª  [CIRCLED DIGIT ZERO]
                        case '\u24FF':
                        // Ã¢â€œÂ¿  [NEGATIVE CIRCLED DIGIT ZERO]
                        case '\uFF10':  // Ã¯Â¼ï¿½  [FULLWIDTH DIGIT ZERO]
                            output[outputPos++] = '0';
                            break;

                        case '\u00B9':
                        // Ã‚Â¹  [SUPERSCRIPT ONE]
                        case '\u2081':
                        // Ã¢â€šï¿½  [SUBSCRIPT ONE]
                        case '\u2460':
                        // Ã¢â€˜Â   [CIRCLED DIGIT ONE]
                        case '\u24F5':
                        // Ã¢â€œÂµ  [DOUBLE CIRCLED DIGIT ONE]
                        case '\u2776':
                        // Ã¢ï¿½Â¶  [DINGBAT NEGATIVE CIRCLED DIGIT ONE]
                        case '\u2780':
                        // Ã¢Å¾â‚¬  [DINGBAT CIRCLED SANS-SERIF DIGIT ONE]
                        case '\u278A':
                        // Ã¢Å¾Å   [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT ONE]
                        case '\uFF11':  // Ã¯Â¼â€˜  [FULLWIDTH DIGIT ONE]
                            output[outputPos++] = '1';
                            break;

                        case '\u2488':  // Ã¢â€™Ë†  [DIGIT ONE FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '.';
                            break;

                        case '\u2474':  // Ã¢â€˜Â´  [PARENTHESIZED DIGIT ONE]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = ')';
                            break;

                        case '\u00B2':
                        // Ã‚Â²  [SUPERSCRIPT TWO]
                        case '\u2082':
                        // Ã¢â€šâ€š  [SUBSCRIPT TWO]
                        case '\u2461':
                        // Ã¢â€˜Â¡  [CIRCLED DIGIT TWO]
                        case '\u24F6':
                        // Ã¢â€œÂ¶  [DOUBLE CIRCLED DIGIT TWO]
                        case '\u2777':
                        // Ã¢ï¿½Â·  [DINGBAT NEGATIVE CIRCLED DIGIT TWO]
                        case '\u2781':
                        // Ã¢Å¾ï¿½  [DINGBAT CIRCLED SANS-SERIF DIGIT TWO]
                        case '\u278B':
                        // Ã¢Å¾â€¹  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT TWO]
                        case '\uFF12':  // Ã¯Â¼â€™  [FULLWIDTH DIGIT TWO]
                            output[outputPos++] = '2';
                            break;

                        case '\u2489':  // Ã¢â€™â€°  [DIGIT TWO FULL STOP]
                            output[outputPos++] = '2';
                            output[outputPos++] = '.';
                            break;

                        case '\u2475':  // Ã¢â€˜Âµ  [PARENTHESIZED DIGIT TWO]
                            output[outputPos++] = '(';
                            output[outputPos++] = '2';
                            output[outputPos++] = ')';
                            break;

                        case '\u00B3':
                        // Ã‚Â³  [SUPERSCRIPT THREE]
                        case '\u2083':
                        // Ã¢â€šÆ’  [SUBSCRIPT THREE]
                        case '\u2462':
                        // Ã¢â€˜Â¢  [CIRCLED DIGIT THREE]
                        case '\u24F7':
                        // Ã¢â€œÂ·  [DOUBLE CIRCLED DIGIT THREE]
                        case '\u2778':
                        // Ã¢ï¿½Â¸  [DINGBAT NEGATIVE CIRCLED DIGIT THREE]
                        case '\u2782':
                        // Ã¢Å¾â€š  [DINGBAT CIRCLED SANS-SERIF DIGIT THREE]
                        case '\u278C':
                        // Ã¢Å¾Å’  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT THREE]
                        case '\uFF13':  // Ã¯Â¼â€œ  [FULLWIDTH DIGIT THREE]
                            output[outputPos++] = '3';
                            break;

                        case '\u248A':  // Ã¢â€™Å   [DIGIT THREE FULL STOP]
                            output[outputPos++] = '3';
                            output[outputPos++] = '.';
                            break;

                        case '\u2476':  // Ã¢â€˜Â¶  [PARENTHESIZED DIGIT THREE]
                            output[outputPos++] = '(';
                            output[outputPos++] = '3';
                            output[outputPos++] = ')';
                            break;

                        case '\u2074':
                        // Ã¢ï¿½Â´  [SUPERSCRIPT FOUR]
                        case '\u2084':
                        // Ã¢â€šâ€ž  [SUBSCRIPT FOUR]
                        case '\u2463':
                        // Ã¢â€˜Â£  [CIRCLED DIGIT FOUR]
                        case '\u24F8':
                        // Ã¢â€œÂ¸  [DOUBLE CIRCLED DIGIT FOUR]
                        case '\u2779':
                        // Ã¢ï¿½Â¹  [DINGBAT NEGATIVE CIRCLED DIGIT FOUR]
                        case '\u2783':
                        // Ã¢Å¾Æ’  [DINGBAT CIRCLED SANS-SERIF DIGIT FOUR]
                        case '\u278D':
                        // Ã¢Å¾ï¿½  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT FOUR]
                        case '\uFF14':  // Ã¯Â¼ï¿½?  [FULLWIDTH DIGIT FOUR]
                            output[outputPos++] = '4';
                            break;

                        case '\u248B':  // Ã¢â€™â€¹  [DIGIT FOUR FULL STOP]
                            output[outputPos++] = '4';
                            output[outputPos++] = '.';
                            break;

                        case '\u2477':  // Ã¢â€˜Â·  [PARENTHESIZED DIGIT FOUR]
                            output[outputPos++] = '(';
                            output[outputPos++] = '4';
                            output[outputPos++] = ')';
                            break;

                        case '\u2075':
                        // Ã¢ï¿½Âµ  [SUPERSCRIPT FIVE]
                        case '\u2085':
                        // Ã¢â€šâ€¦  [SUBSCRIPT FIVE]
                        case '\u2464':
                        // Ã¢â€˜Â¤  [CIRCLED DIGIT FIVE]
                        case '\u24F9':
                        // Ã¢â€œÂ¹  [DOUBLE CIRCLED DIGIT FIVE]
                        case '\u277A':
                        // Ã¢ï¿½Âº  [DINGBAT NEGATIVE CIRCLED DIGIT FIVE]
                        case '\u2784':
                        // Ã¢Å¾â€ž  [DINGBAT CIRCLED SANS-SERIF DIGIT FIVE]
                        case '\u278E':
                        // Ã¢Å¾Å½  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT FIVE]
                        case '\uFF15':  // Ã¯Â¼â€¢  [FULLWIDTH DIGIT FIVE]
                            output[outputPos++] = '5';
                            break;

                        case '\u248C':  // Ã¢â€™Å’  [DIGIT FIVE FULL STOP]
                            output[outputPos++] = '5';
                            output[outputPos++] = '.';
                            break;

                        case '\u2478':  // Ã¢â€˜Â¸  [PARENTHESIZED DIGIT FIVE]
                            output[outputPos++] = '(';
                            output[outputPos++] = '5';
                            output[outputPos++] = ')';
                            break;

                        case '\u2076':
                        // Ã¢ï¿½Â¶  [SUPERSCRIPT SIX]
                        case '\u2086':
                        // Ã¢â€šâ€   [SUBSCRIPT SIX]
                        case '\u2465':
                        // Ã¢â€˜Â¥  [CIRCLED DIGIT SIX]
                        case '\u24FA':
                        // Ã¢â€œÂº  [DOUBLE CIRCLED DIGIT SIX]
                        case '\u277B':
                        // Ã¢ï¿½Â»  [DINGBAT NEGATIVE CIRCLED DIGIT SIX]
                        case '\u2785':
                        // Ã¢Å¾â€¦  [DINGBAT CIRCLED SANS-SERIF DIGIT SIX]
                        case '\u278F':
                        // Ã¢Å¾ï¿½  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT SIX]
                        case '\uFF16':  // Ã¯Â¼â€“  [FULLWIDTH DIGIT SIX]
                            output[outputPos++] = '6';
                            break;

                        case '\u248D':  // Ã¢â€™ï¿½  [DIGIT SIX FULL STOP]
                            output[outputPos++] = '6';
                            output[outputPos++] = '.';
                            break;

                        case '\u2479':  // Ã¢â€˜Â¹  [PARENTHESIZED DIGIT SIX]
                            output[outputPos++] = '(';
                            output[outputPos++] = '6';
                            output[outputPos++] = ')';
                            break;

                        case '\u2077':
                        // Ã¢ï¿½Â·  [SUPERSCRIPT SEVEN]
                        case '\u2087':
                        // Ã¢â€šâ€¡  [SUBSCRIPT SEVEN]
                        case '\u2466':
                        // Ã¢â€˜Â¦  [CIRCLED DIGIT SEVEN]
                        case '\u24FB':
                        // Ã¢â€œÂ»  [DOUBLE CIRCLED DIGIT SEVEN]
                        case '\u277C':
                        // Ã¢ï¿½Â¼  [DINGBAT NEGATIVE CIRCLED DIGIT SEVEN]
                        case '\u2786':
                        // Ã¢Å¾â€   [DINGBAT CIRCLED SANS-SERIF DIGIT SEVEN]
                        case '\u2790':
                        // Ã¢Å¾ï¿½  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT SEVEN]
                        case '\uFF17':  // Ã¯Â¼â€”  [FULLWIDTH DIGIT SEVEN]
                            output[outputPos++] = '7';
                            break;

                        case '\u248E':  // Ã¢â€™Å½  [DIGIT SEVEN FULL STOP]
                            output[outputPos++] = '7';
                            output[outputPos++] = '.';
                            break;

                        case '\u247A':  // Ã¢â€˜Âº  [PARENTHESIZED DIGIT SEVEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '7';
                            output[outputPos++] = ')';
                            break;

                        case '\u2078':
                        // Ã¢ï¿½Â¸  [SUPERSCRIPT EIGHT]
                        case '\u2088':
                        // Ã¢â€šË†  [SUBSCRIPT EIGHT]
                        case '\u2467':
                        // Ã¢â€˜Â§  [CIRCLED DIGIT EIGHT]
                        case '\u24FC':
                        // Ã¢â€œÂ¼  [DOUBLE CIRCLED DIGIT EIGHT]
                        case '\u277D':
                        // Ã¢ï¿½Â½  [DINGBAT NEGATIVE CIRCLED DIGIT EIGHT]
                        case '\u2787':
                        // Ã¢Å¾â€¡  [DINGBAT CIRCLED SANS-SERIF DIGIT EIGHT]
                        case '\u2791':
                        // Ã¢Å¾â€˜  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT EIGHT]
                        case '\uFF18':  // Ã¯Â¼Ëœ  [FULLWIDTH DIGIT EIGHT]
                            output[outputPos++] = '8';
                            break;

                        case '\u248F':  // Ã¢â€™ï¿½  [DIGIT EIGHT FULL STOP]
                            output[outputPos++] = '8';
                            output[outputPos++] = '.';
                            break;

                        case '\u247B':  // Ã¢â€˜Â»  [PARENTHESIZED DIGIT EIGHT]
                            output[outputPos++] = '(';
                            output[outputPos++] = '8';
                            output[outputPos++] = ')';
                            break;

                        case '\u2079':
                        // Ã¢ï¿½Â¹  [SUPERSCRIPT NINE]
                        case '\u2089':
                        // Ã¢â€šâ€°  [SUBSCRIPT NINE]
                        case '\u2468':
                        // Ã¢â€˜Â¨  [CIRCLED DIGIT NINE]
                        case '\u24FD':
                        // Ã¢â€œÂ½  [DOUBLE CIRCLED DIGIT NINE]
                        case '\u277E':
                        // Ã¢ï¿½Â¾  [DINGBAT NEGATIVE CIRCLED DIGIT NINE]
                        case '\u2788':
                        // Ã¢Å¾Ë†  [DINGBAT CIRCLED SANS-SERIF DIGIT NINE]
                        case '\u2792':
                        // Ã¢Å¾â€™  [DINGBAT NEGATIVE CIRCLED SANS-SERIF DIGIT NINE]
                        case '\uFF19':  // Ã¯Â¼â„¢  [FULLWIDTH DIGIT NINE]
                            output[outputPos++] = '9';
                            break;

                        case '\u2490':  // Ã¢â€™ï¿½  [DIGIT NINE FULL STOP]
                            output[outputPos++] = '9';
                            output[outputPos++] = '.';
                            break;

                        case '\u247C':  // Ã¢â€˜Â¼  [PARENTHESIZED DIGIT NINE]
                            output[outputPos++] = '(';
                            output[outputPos++] = '9';
                            output[outputPos++] = ')';
                            break;

                        case '\u2469':
                        // Ã¢â€˜Â©  [CIRCLED NUMBER TEN]
                        case '\u24FE':
                        // Ã¢â€œÂ¾  [DOUBLE CIRCLED NUMBER TEN]
                        case '\u277F':
                        // Ã¢ï¿½Â¿  [DINGBAT NEGATIVE CIRCLED NUMBER TEN]
                        case '\u2789':
                        // Ã¢Å¾â€°  [DINGBAT CIRCLED SANS-SERIF NUMBER TEN]
                        case '\u2793':  // Ã¢Å¾â€œ  [DINGBAT NEGATIVE CIRCLED SANS-SERIF NUMBER TEN]
                            output[outputPos++] = '1';
                            output[outputPos++] = '0';
                            break;

                        case '\u2491':  // Ã¢â€™â€˜  [NUMBER TEN FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '0';
                            output[outputPos++] = '.';
                            break;

                        case '\u247D':  // Ã¢â€˜Â½  [PARENTHESIZED NUMBER TEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '0';
                            output[outputPos++] = ')';
                            break;

                        case '\u246A':
                        // Ã¢â€˜Âª  [CIRCLED NUMBER ELEVEN]
                        case '\u24EB':  // Ã¢â€œÂ«  [NEGATIVE CIRCLED NUMBER ELEVEN]
                            output[outputPos++] = '1';
                            output[outputPos++] = '1';
                            break;

                        case '\u2492':  // Ã¢â€™â€™  [NUMBER ELEVEN FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '1';
                            output[outputPos++] = '.';
                            break;

                        case '\u247E':  // Ã¢â€˜Â¾  [PARENTHESIZED NUMBER ELEVEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '1';
                            output[outputPos++] = ')';
                            break;

                        case '\u246B':
                        // Ã¢â€˜Â«  [CIRCLED NUMBER TWELVE]
                        case '\u24EC':  // Ã¢â€œÂ¬  [NEGATIVE CIRCLED NUMBER TWELVE]
                            output[outputPos++] = '1';
                            output[outputPos++] = '2';
                            break;

                        case '\u2493':  // Ã¢â€™â€œ  [NUMBER TWELVE FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '2';
                            output[outputPos++] = '.';
                            break;

                        case '\u247F':  // Ã¢â€˜Â¿  [PARENTHESIZED NUMBER TWELVE]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '2';
                            output[outputPos++] = ')';
                            break;

                        case '\u246C':
                        // Ã¢â€˜Â¬  [CIRCLED NUMBER THIRTEEN]
                        case '\u24ED':  // Ã¢â€œÂ­  [NEGATIVE CIRCLED NUMBER THIRTEEN]
                            output[outputPos++] = '1';
                            output[outputPos++] = '3';
                            break;

                        case '\u2494':  // Ã¢â€™ï¿½?  [NUMBER THIRTEEN FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '3';
                            output[outputPos++] = '.';
                            break;

                        case '\u2480':  // Ã¢â€™â‚¬  [PARENTHESIZED NUMBER THIRTEEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '3';
                            output[outputPos++] = ')';
                            break;

                        case '\u246D':
                        // Ã¢â€˜Â­  [CIRCLED NUMBER FOURTEEN]
                        case '\u24EE':  // Ã¢â€œÂ®  [NEGATIVE CIRCLED NUMBER FOURTEEN]
                            output[outputPos++] = '1';
                            output[outputPos++] = '4';
                            break;

                        case '\u2495':  // Ã¢â€™â€¢  [NUMBER FOURTEEN FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '4';
                            output[outputPos++] = '.';
                            break;

                        case '\u2481':  // Ã¢â€™ï¿½  [PARENTHESIZED NUMBER FOURTEEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '4';
                            output[outputPos++] = ')';
                            break;

                        case '\u246E':
                        // Ã¢â€˜Â®  [CIRCLED NUMBER FIFTEEN]
                        case '\u24EF':  // Ã¢â€œÂ¯  [NEGATIVE CIRCLED NUMBER FIFTEEN]
                            output[outputPos++] = '1';
                            output[outputPos++] = '5';
                            break;

                        case '\u2496':  // Ã¢â€™â€“  [NUMBER FIFTEEN FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '5';
                            output[outputPos++] = '.';
                            break;

                        case '\u2482':  // Ã¢â€™â€š  [PARENTHESIZED NUMBER FIFTEEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '5';
                            output[outputPos++] = ')';
                            break;

                        case '\u246F':
                        // Ã¢â€˜Â¯  [CIRCLED NUMBER SIXTEEN]
                        case '\u24F0':  // Ã¢â€œÂ°  [NEGATIVE CIRCLED NUMBER SIXTEEN]
                            output[outputPos++] = '1';
                            output[outputPos++] = '6';
                            break;

                        case '\u2497':  // Ã¢â€™â€”  [NUMBER SIXTEEN FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '6';
                            output[outputPos++] = '.';
                            break;

                        case '\u2483':  // Ã¢â€™Æ’  [PARENTHESIZED NUMBER SIXTEEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '6';
                            output[outputPos++] = ')';
                            break;

                        case '\u2470':
                        // Ã¢â€˜Â°  [CIRCLED NUMBER SEVENTEEN]
                        case '\u24F1':  // Ã¢â€œÂ±  [NEGATIVE CIRCLED NUMBER SEVENTEEN]
                            output[outputPos++] = '1';
                            output[outputPos++] = '7';
                            break;

                        case '\u2498':  // Ã¢â€™Ëœ  [NUMBER SEVENTEEN FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '7';
                            output[outputPos++] = '.';
                            break;

                        case '\u2484':  // Ã¢â€™â€ž  [PARENTHESIZED NUMBER SEVENTEEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '7';
                            output[outputPos++] = ')';
                            break;

                        case '\u2471':
                        // Ã¢â€˜Â±  [CIRCLED NUMBER EIGHTEEN]
                        case '\u24F2':  // Ã¢â€œÂ²  [NEGATIVE CIRCLED NUMBER EIGHTEEN]
                            output[outputPos++] = '1';
                            output[outputPos++] = '8';
                            break;

                        case '\u2499':  // Ã¢â€™â„¢  [NUMBER EIGHTEEN FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '8';
                            output[outputPos++] = '.';
                            break;

                        case '\u2485':  // Ã¢â€™â€¦  [PARENTHESIZED NUMBER EIGHTEEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '8';
                            output[outputPos++] = ')';
                            break;

                        case '\u2472':
                        // Ã¢â€˜Â²  [CIRCLED NUMBER NINETEEN]
                        case '\u24F3':  // Ã¢â€œÂ³  [NEGATIVE CIRCLED NUMBER NINETEEN]
                            output[outputPos++] = '1';
                            output[outputPos++] = '9';
                            break;

                        case '\u249A':  // Ã¢â€™Å¡  [NUMBER NINETEEN FULL STOP]
                            output[outputPos++] = '1';
                            output[outputPos++] = '9';
                            output[outputPos++] = '.';
                            break;

                        case '\u2486':  // Ã¢â€™â€   [PARENTHESIZED NUMBER NINETEEN]
                            output[outputPos++] = '(';
                            output[outputPos++] = '1';
                            output[outputPos++] = '9';
                            output[outputPos++] = ')';
                            break;

                        case '\u2473':
                        // Ã¢â€˜Â³  [CIRCLED NUMBER TWENTY]
                        case '\u24F4':  // Ã¢â€œÂ´  [NEGATIVE CIRCLED NUMBER TWENTY]
                            output[outputPos++] = '2';
                            output[outputPos++] = '0';
                            break;

                        case '\u249B':  // Ã¢â€™â€º  [NUMBER TWENTY FULL STOP]
                            output[outputPos++] = '2';
                            output[outputPos++] = '0';
                            output[outputPos++] = '.';
                            break;

                        case '\u2487':  // Ã¢â€™â€¡  [PARENTHESIZED NUMBER TWENTY]
                            output[outputPos++] = '(';
                            output[outputPos++] = '2';
                            output[outputPos++] = '0';
                            output[outputPos++] = ')';
                            break;

                        case '\u00AB':
                        // Ã‚Â«  [LEFT-POINTING DOUBLE ANGLE QUOTATION MARK]
                        case '\u00BB':
                        // Ã‚Â»  [RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK]
                        case '\u201C':
                        // Ã¢â‚¬Å“  [LEFT DOUBLE QUOTATION MARK]
                        case '\u201D':
                        // Ã¢â‚¬ï¿½  [RIGHT DOUBLE QUOTATION MARK]
                        case '\u201E':
                        // Ã¢â‚¬Å¾  [DOUBLE LOW-9 QUOTATION MARK]
                        case '\u2033':
                        // Ã¢â‚¬Â³  [DOUBLE PRIME]
                        case '\u2036':
                        // Ã¢â‚¬Â¶  [REVERSED DOUBLE PRIME]
                        case '\u275D':
                        // Ã¢ï¿½ï¿½  [HEAVY DOUBLE TURNED COMMA QUOTATION MARK ORNAMENT]
                        case '\u275E':
                        // Ã¢ï¿½Å¾  [HEAVY DOUBLE COMMA QUOTATION MARK ORNAMENT]
                        case '\u276E':
                        // Ã¢ï¿½Â®  [HEAVY LEFT-POINTING ANGLE QUOTATION MARK ORNAMENT]
                        case '\u276F':
                        // Ã¢ï¿½Â¯  [HEAVY RIGHT-POINTING ANGLE QUOTATION MARK ORNAMENT]
                        case '\uFF02':  // Ã¯Â¼â€š  [FULLWIDTH QUOTATION MARK]
                            output[outputPos++] = '"';
                            break;

                        case '\u2018':
                        // Ã¢â‚¬Ëœ  [LEFT SINGLE QUOTATION MARK]
                        case '\u2019':
                        // Ã¢â‚¬â„¢  [RIGHT SINGLE QUOTATION MARK]
                        case '\u201A':
                        // Ã¢â‚¬Å¡  [SINGLE LOW-9 QUOTATION MARK]
                        case '\u201B':
                        // Ã¢â‚¬â€º  [SINGLE HIGH-REVERSED-9 QUOTATION MARK]
                        case '\u2032':
                        // Ã¢â‚¬Â²  [PRIME]
                        case '\u2035':
                        // Ã¢â‚¬Âµ  [REVERSED PRIME]
                        case '\u2039':
                        // Ã¢â‚¬Â¹  [SINGLE LEFT-POINTING ANGLE QUOTATION MARK]
                        case '\u203A':
                        // Ã¢â‚¬Âº  [SINGLE RIGHT-POINTING ANGLE QUOTATION MARK]
                        case '\u275B':
                        // Ã¢ï¿½â€º  [HEAVY SINGLE TURNED COMMA QUOTATION MARK ORNAMENT]
                        case '\u275C':
                        // Ã¢ï¿½Å“  [HEAVY SINGLE COMMA QUOTATION MARK ORNAMENT]
                        case '\uFF07':  // Ã¯Â¼â€¡  [FULLWIDTH APOSTROPHE]
                            output[outputPos++] = '\'';
                            break;

                        case '\u2010':
                        // Ã¢â‚¬ï¿½  [HYPHEN]
                        case '\u2011':
                        // Ã¢â‚¬â€˜  [NON-BREAKING HYPHEN]
                        case '\u2012':
                        // Ã¢â‚¬â€™  [FIGURE DASH]
                        case '\u2013':
                        // Ã¢â‚¬â€œ  [EN DASH]
                        case '\u2014':
                        // Ã¢â‚¬ï¿½?  [EM DASH]
                        case '\u207B':
                        // Ã¢ï¿½Â»  [SUPERSCRIPT MINUS]
                        case '\u208B':
                        // Ã¢â€šâ€¹  [SUBSCRIPT MINUS]
                        case '\uFF0D':  // Ã¯Â¼ï¿½  [FULLWIDTH HYPHEN-MINUS]
                            output[outputPos++] = '-';
                            break;

                        case '\u2045':
                        // Ã¢ï¿½â€¦  [LEFT SQUARE BRACKET WITH QUILL]
                        case '\u2772':
                        // Ã¢ï¿½Â²  [LIGHT LEFT TORTOISE SHELL BRACKET ORNAMENT]
                        case '\uFF3B':  // Ã¯Â¼Â»  [FULLWIDTH LEFT SQUARE BRACKET]
                            output[outputPos++] = '[';
                            break;

                        case '\u2046':
                        // Ã¢ï¿½â€   [RIGHT SQUARE BRACKET WITH QUILL]
                        case '\u2773':
                        // Ã¢ï¿½Â³  [LIGHT RIGHT TORTOISE SHELL BRACKET ORNAMENT]
                        case '\uFF3D':  // Ã¯Â¼Â½  [FULLWIDTH RIGHT SQUARE BRACKET]
                            output[outputPos++] = ']';
                            break;

                        case '\u207D':
                        // Ã¢ï¿½Â½  [SUPERSCRIPT LEFT PARENTHESIS]
                        case '\u208D':
                        // Ã¢â€šï¿½  [SUBSCRIPT LEFT PARENTHESIS]
                        case '\u2768':
                        // Ã¢ï¿½Â¨  [MEDIUM LEFT PARENTHESIS ORNAMENT]
                        case '\u276A':
                        // Ã¢ï¿½Âª  [MEDIUM FLATTENED LEFT PARENTHESIS ORNAMENT]
                        case '\uFF08':  // Ã¯Â¼Ë†  [FULLWIDTH LEFT PARENTHESIS]
                            output[outputPos++] = '(';
                            break;

                        case '\u2E28':  // Ã¢Â¸Â¨  [LEFT DOUBLE PARENTHESIS]
                            output[outputPos++] = '(';
                            output[outputPos++] = '(';
                            break;

                        case '\u207E':
                        // Ã¢ï¿½Â¾  [SUPERSCRIPT RIGHT PARENTHESIS]
                        case '\u208E':
                        // Ã¢â€šÅ½  [SUBSCRIPT RIGHT PARENTHESIS]
                        case '\u2769':
                        // Ã¢ï¿½Â©  [MEDIUM RIGHT PARENTHESIS ORNAMENT]
                        case '\u276B':
                        // Ã¢ï¿½Â«  [MEDIUM FLATTENED RIGHT PARENTHESIS ORNAMENT]
                        case '\uFF09':  // Ã¯Â¼â€°  [FULLWIDTH RIGHT PARENTHESIS]
                            output[outputPos++] = ')';
                            break;

                        case '\u2E29':  // Ã¢Â¸Â©  [RIGHT DOUBLE PARENTHESIS]
                            output[outputPos++] = ')';
                            output[outputPos++] = ')';
                            break;

                        case '\u276C':
                        // Ã¢ï¿½Â¬  [MEDIUM LEFT-POINTING ANGLE BRACKET ORNAMENT]
                        case '\u2770':
                        // Ã¢ï¿½Â°  [HEAVY LEFT-POINTING ANGLE BRACKET ORNAMENT]
                        case '\uFF1C':  // Ã¯Â¼Å“  [FULLWIDTH LESS-THAN SIGN]
                            output[outputPos++] = '<';
                            break;

                        case '\u276D':
                        // Ã¢ï¿½Â­  [MEDIUM RIGHT-POINTING ANGLE BRACKET ORNAMENT]
                        case '\u2771':
                        // Ã¢ï¿½Â±  [HEAVY RIGHT-POINTING ANGLE BRACKET ORNAMENT]
                        case '\uFF1E':  // Ã¯Â¼Å¾  [FULLWIDTH GREATER-THAN SIGN]
                            output[outputPos++] = '>';
                            break;

                        case '\u2774':
                        // Ã¢ï¿½Â´  [MEDIUM LEFT CURLY BRACKET ORNAMENT]
                        case '\uFF5B':  // Ã¯Â½â€º  [FULLWIDTH LEFT CURLY BRACKET]
                            output[outputPos++] = '{';
                            break;

                        case '\u2775':
                        // Ã¢ï¿½Âµ  [MEDIUM RIGHT CURLY BRACKET ORNAMENT]
                        case '\uFF5D':  // Ã¯Â½ï¿½  [FULLWIDTH RIGHT CURLY BRACKET]
                            output[outputPos++] = '}';
                            break;

                        case '\u207A':
                        // Ã¢ï¿½Âº  [SUPERSCRIPT PLUS SIGN]
                        case '\u208A':
                        // Ã¢â€šÅ   [SUBSCRIPT PLUS SIGN]
                        case '\uFF0B':  // Ã¯Â¼â€¹  [FULLWIDTH PLUS SIGN]
                            output[outputPos++] = '+';
                            break;

                        case '\u207C':
                        // Ã¢ï¿½Â¼  [SUPERSCRIPT EQUALS SIGN]
                        case '\u208C':
                        // Ã¢â€šÅ’  [SUBSCRIPT EQUALS SIGN]
                        case '\uFF1D':  // Ã¯Â¼ï¿½  [FULLWIDTH EQUALS SIGN]
                            output[outputPos++] = '=';
                            break;

                        case '\uFF01':  // Ã¯Â¼ï¿½  [FULLWIDTH EXCLAMATION MARK]
                            output[outputPos++] = '!';
                            break;

                        case '\u203C':  // Ã¢â‚¬Â¼  [DOUBLE EXCLAMATION MARK]
                            output[outputPos++] = '!';
                            output[outputPos++] = '!';
                            break;

                        case '\u2049':  // Ã¢ï¿½â€°  [EXCLAMATION QUESTION MARK]
                            output[outputPos++] = '!';
                            output[outputPos++] = '?';
                            break;

                        case '\uFF03':  // Ã¯Â¼Æ’  [FULLWIDTH NUMBER SIGN]
                            output[outputPos++] = '#';
                            break;

                        case '\uFF04':  // Ã¯Â¼â€ž  [FULLWIDTH DOLLAR SIGN]
                            output[outputPos++] = '$';
                            break;

                        case '\u2052':
                        // Ã¢ï¿½â€™  [COMMERCIAL MINUS SIGN]
                        case '\uFF05':  // Ã¯Â¼â€¦  [FULLWIDTH PERCENT SIGN]
                            output[outputPos++] = '%';
                            break;

                        case '\uFF06':  // Ã¯Â¼â€   [FULLWIDTH AMPERSAND]
                            output[outputPos++] = '&';
                            break;

                        case '\u204E':
                        // Ã¢ï¿½Å½  [LOW ASTERISK]
                        case '\uFF0A':  // Ã¯Â¼Å   [FULLWIDTH ASTERISK]
                            output[outputPos++] = '*';
                            break;

                        case '\uFF0C':  // Ã¯Â¼Å’  [FULLWIDTH COMMA]
                            output[outputPos++] = ',';
                            break;

                        case '\uFF0E':  // Ã¯Â¼Å½  [FULLWIDTH FULL STOP]
                            output[outputPos++] = '.';
                            break;

                        case '\u2044':
                        // Ã¢ï¿½â€ž  [FRACTION SLASH]
                        case '\uFF0F':  // Ã¯Â¼ï¿½  [FULLWIDTH SOLIDUS]
                            output[outputPos++] = '/';
                            break;

                        case '\uFF1A':  // Ã¯Â¼Å¡  [FULLWIDTH COLON]
                            output[outputPos++] = ':';
                            break;

                        case '\u204F':
                        // Ã¢ï¿½ï¿½  [REVERSED SEMICOLON]
                        case '\uFF1B':  // Ã¯Â¼â€º  [FULLWIDTH SEMICOLON]
                            output[outputPos++] = ';';
                            break;

                        case '\uFF1F':  // Ã¯Â¼Å¸  [FULLWIDTH QUESTION MARK]
                            output[outputPos++] = '?';
                            break;

                        case '\u2047':  // Ã¢ï¿½â€¡  [DOUBLE QUESTION MARK]
                            output[outputPos++] = '?';
                            output[outputPos++] = '?';
                            break;

                        case '\u2048':  // Ã¢ï¿½Ë†  [QUESTION EXCLAMATION MARK]
                            output[outputPos++] = '?';
                            output[outputPos++] = '!';
                            break;

                        case '\uFF20':  // Ã¯Â¼Â   [FULLWIDTH COMMERCIAL AT]
                            output[outputPos++] = '@';
                            break;

                        case '\uFF3C':  // Ã¯Â¼Â¼  [FULLWIDTH REVERSE SOLIDUS]
                            output[outputPos++] = '\\';
                            break;

                        case '\u2038':
                        // Ã¢â‚¬Â¸  [CARET]
                        case '\uFF3E':  // Ã¯Â¼Â¾  [FULLWIDTH CIRCUMFLEX ACCENT]
                            output[outputPos++] = '^';
                            break;

                        case '\uFF3F':  // Ã¯Â¼Â¿  [FULLWIDTH LOW LINE]
                            output[outputPos++] = '_';
                            break;

                        case '\u2053':
                        // Ã¢ï¿½â€œ  [SWUNG DASH]
                        case '\uFF5E':  // Ã¯Â½Å¾  [FULLWIDTH TILDE]
                            output[outputPos++] = '~';
                            break;

                        default:
                            output[outputPos++] = c;
                            break;

                    }
                }
            }

            #endregion
        }

        static int GetNextSize(int targetSize)
        {
            /* This over-allocates proportional to the list size, making room
			* for additional growth.  The over-allocation is mild, but is
			* enough to give linear-time amortized behavior over a long
			* sequence of appends() in the presence of a poorly-performing
			* system realloc().
			* The growth pattern is:  0, 4, 8, 16, 25, 35, 46, 58, 72, 88, ...
			*/
            return (targetSize >> 3) + (targetSize < 9 ? 3 : 6) + targetSize;
        }

        #endregion

        #region Format

        public static string FormatSmart(this string value, params object[] args)
        {
            //the Default SmartFormatter is always set anew when changing UICulture/language
            return Smart.Default.Format(value, args);
        }

        public static string FormatSmart(this string value, CultureInfo culture, params object[] args)
        {
            return Smart.Format(culture, value, args);
        }

        public static string FormatString(this string value, params object[] args)
        {
            return string.Format(value, args);
        }

        public static string FormatStringSafe(this string value, params object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null && args[i] is string)
                {
                    args[i] = (object)((string)args[i]).EscapeCurlyBraces(false);
                }
            }

            return string.Format(value, args);
        }

        #endregion

        #region GetCompareValue

        /// <summary>
        /// Removes certain characters which should not be included in sorting (e.g. ", (, [, « etc.)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetCompareValue(this string value)
        {
            if (string.IsNullOrEmpty(value)) return "_";

            switch (value[0])
            {
                case '"':
                case '(':
                case '[':
                case '«':
                case '»':
                case '„':
                case '“':
                case '”':
                    value = value.TrimStart('"', '(', '[', '«', '»', '„', '“', '”');
                    break;
            }

            value = value.Clean(IllegalCharacters.NonStandardWhitespace, false);

            return string.IsNullOrEmpty(value) ? "_" : value;
        }

        #endregion

        #region GetDamerauLevenshteinDistance

        public static double GetDamerauLevenshteinDistance(string text1, string text2)
        {
            // http://en.wikipedia.org/wiki/Damerau%E2%80%93Levenshtein_distance

            if (string.IsNullOrEmpty(text1))
            {
                if (string.IsNullOrEmpty(text2)) return 0;
                return text2.Length;
            }
            else if (string.IsNullOrEmpty(text2))
            {
                return text1.Length;
            }

            var m = text1.Length;
            var n = text2.Length;
            int length = Math.Max(m, n);
            int[,] H = new int[m + 2, n + 2];

            int INF = m + n;
            H[0, 0] = INF;
            for (int i = 0; i <= m; i++) { H[i + 1, 1] = i; H[i + 1, 0] = INF; }
            for (int j = 0; j <= n; j++) { H[1, j + 1] = j; H[0, j + 1] = INF; }

            SortedDictionary<Char, int> sd = new SortedDictionary<Char, int>();
            foreach (Char Letter in (text1 + text2))
            {
                if (!sd.ContainsKey(Letter))
                    sd.Add(Letter, 0);
            }

            for (int i = 1; i <= m; i++)
            {
                int DB = 0;
                for (int j = 1; j <= n; j++)
                {
                    int i1 = sd[text2[j - 1]];
                    int j1 = DB;

                    if (text1[i - 1] == text2[j - 1])
                    {
                        H[i + 1, j + 1] = H[i, j];
                        DB = j;
                    }
                    else
                    {
                        H[i + 1, j + 1] = Math.Min(H[i, j], Math.Min(H[i + 1, j], H[i, j + 1])) + 1;
                    }

                    H[i + 1, j + 1] = Math.Min(H[i + 1, j + 1], H[i1, j1] + (i - i1 - 1) + 1 + (j - j1 - 1));
                }

                sd[text1[i - 1]] = i;
            }

            var x = H[m + 1, n + 1];
            var result = 1.0 - (double)x / length;

            return result;
        }

        #endregion

        #region GetLevenshteinDistance

        public static double GetLevenshteinDistance(string text1, string text2)
        {
            int n = text1.Length;
            int m = text2.Length;
            int length = Math.Max(text1.Length, text2.Length);

            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 1; i <= n; i++)
            {
                //#1150
                //https://stackoverflow.com/questions/39073599/levenshtein-compare-strings-without-changing-numbers
                if ('0' <= text1[i - 1] && text1[i - 1] <= '9')
                    d[i, 0] = d[i - 1, 0] + 1000;
                else
                    d[i, 0] = d[i - 1, 0] + 1;
            }

            for (int j = 1; j <= m; j++)
            {
                if ('0' <= text2[j - 1] && text2[j - 1] <= '9')
                    d[0, j] = d[0, j - 1] + 1000;
                else
                    d[0, j] = d[0, j - 1] + 1;
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    bool isIdentical = text2[j - 1] == text1[i - 1];
                    bool isNumber = ('0' <= text2[j - 1] && text2[j - 1] <= '9') || ('0' <= text1[i - 1] && text1[i - 1] <= '9');

                    int cost1 = isIdentical ? 0 : (isNumber ? 200 : 1);
                    int cost2 = isNumber ? 200 : 1;

                    // Step 6
                    d[i, j] = (ushort)(Math.Min(Math.Min(d[i - 1, j] + cost2, d[i, j - 1] + cost2), d[i - 1, j - 1] + cost1));
                }
            }
            var x = d[n, m];
            var result = 1.0 - (double)x / length;
            return result;
        }

        #endregion

        #region GetLines

        public static IEnumerable<string> GetLines(this string input)
        {
            if (string.IsNullOrEmpty(input)) yield break;

            using (StringReader reader = new StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        #endregion

        #region GetQuotationMarksMatches

        static Regex _quotationMarkRegex = new Regex("\"(.+?)\"", RegexOptions.CultureInvariant);

        public static MatchCollection GetQuotationMarksMatches(this string text)
        {
            return _quotationMarkRegex.Matches(text);
        }

        #endregion

        #region GetNoDateMatches

        #endregion

        #region GetInPrintMatches


        #endregion

        #region GetDuplicates

        public static IEnumerable<string> GetDuplicates(this IEnumerable<string> strings)
        {
            if (strings == null || !strings.Any() || strings.Count() == 1) return Enumerable.Empty<string>();

            return (from s in strings
                    group s by s into groupedStrings
                    where groupedStrings.Count() > 1
                    select groupedStrings.Key).AsEnumerable();
        }

        #endregion

        #region GetEtAlMatches


        #endregion

        #region GetUniqueName

        public static string GetUniqueName(this string name, Func<IEnumerable<string>> nameExtractor)
        {
            var existing = (from s in nameExtractor()
                            where s.Equals(name, StringComparison.CurrentCultureIgnoreCase)
                            select s).FirstOrDefault();

            var i = 1;
            while (existing != null)
            {
                if (name.EndsWith(" (" + i + ")"))
                {
                    name = name.Substring(0, name.Length - (3 + i.ToString().Length));
                }

                i++;
                name = name + " (" + i + ")";
                existing = (from s in nameExtractor()
                            where s.Equals(name, StringComparison.CurrentCultureIgnoreCase)
                            select s).FirstOrDefault();
            }

            return name;
        }

        #endregion

        #region GetSortFullName

        public static string GetSortFullName(string value)
        {
            return value.ToLowerInvariant().GetCompareValue().Truncate(100, false);
        }

        #endregion

        #region GetUrlMatches

        public static MatchCollection GetUrlMatches(this string text)
        {
            return UriUtility.UrlRegex.Matches(text);
        }

        #endregion

        #region GetUnicodeEscapeSquence

        public static string GetUnicodeEscapeSequence(this char c)
        {
            return "\\u" + ((int)c).ToString("X4");
        }

        public static string GetUnicodeEscapeSequence(this string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            var stringBuilder = new StringBuilder();
            
            foreach(var c in s)
            {
                stringBuilder.Append(c.GetUnicodeEscapeSequence());
            }
            return stringBuilder.ToString();
        }

        #endregion

        #region HasDuplicates

        public static bool HasDuplicates(this IEnumerable<string> strings)
        {
            if (strings == null || !strings.Any() || strings.Count() == 1) return false;

            var duplicates = strings.GetDuplicates();
            return duplicates != null && duplicates.Any();
        }

        #endregion

        #region HasLowercase

        public static bool HasLowercase(this string input)
        {
            return !string.IsNullOrEmpty(input) && input.Any(c => char.IsLower(c));
        }

        #endregion

        #region MakeReadable

        public static string MakeReadable(this string text)
        {
            return MakeReadable(text, null);
        }

        public static string MakeReadable(this string text, CultureInfo culture, bool doWhiteSpaceOnly = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (culture == null) return SpecialChars.EmptyStringReplacement;
                return SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.EmptyStringReplacement), culture);
            }

            var charArray = text.ToCharArray();
            var replacementStringArray = new string[charArray.Length];
            char nextChar = '\0';
            char secondnextChar = '\0';

            bool charIsWhiteSpace = false;

            for (int i = 0; i < charArray.Length; i++)
            {
                nextChar = (i < charArray.Length - 1) ? charArray[i + 1] : '\0';
                secondnextChar = (i < charArray.Length - 2) ? charArray[i + 2] : '\0';

                charIsWhiteSpace = char.IsWhiteSpace(charArray[i]);
                if (!charIsWhiteSpace && doWhiteSpaceOnly)
                {
                    replacementStringArray[i] = charArray[i].ToString();
                    continue;
                }

                switch (charArray[i])
                {

                    case CharUnicodeInfo.Null:
                        continue;

                    #region New Line

                    case CharUnicodeInfo.NewlineSign:
                        {
                            if (nextChar == CharUnicodeInfo.CarriageReturn && secondnextChar == CharUnicodeInfo.LineFeed)
                            {
                                if (culture == null) replacementStringArray[i] = SpecialChars.SoftReturnReplacement;
                                else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.SoftReturnReplacement), culture);

                                nextChar = CharUnicodeInfo.Null;
                                secondnextChar = CharUnicodeInfo.Null;
                                continue;
                            }

                            if (nextChar == CharUnicodeInfo.LineFeed || nextChar == CharUnicodeInfo.LineSeparator)
                            {
                                if (culture == null) replacementStringArray[i] = SpecialChars.SoftReturnReplacement;
                                else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.SoftReturnReplacement), culture);

                                nextChar = CharUnicodeInfo.Null;
                                continue;
                            }
                        }
                        break;

                    case CharUnicodeInfo.LineSeparator:
                        break;

                    #endregion New Line

                    #region Paragraph

                    case CharUnicodeInfo.ParagraphSign:
                        {
                            if (nextChar == CharUnicodeInfo.CarriageReturn && secondnextChar == CharUnicodeInfo.LineFeed)
                            {
                                if (culture == null) replacementStringArray[i] = SpecialChars.ReturnReplacement;
                                else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.ReturnReplacement), culture);

                                nextChar = CharUnicodeInfo.Null;
                                secondnextChar = CharUnicodeInfo.Null;
                                continue;
                            }

                            if (nextChar == CharUnicodeInfo.LineFeed || nextChar == CharUnicodeInfo.LineSeparator)
                            {
                                if (culture == null) replacementStringArray[i] = SpecialChars.ReturnReplacement;
                                else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.ReturnReplacement), culture);

                                nextChar = CharUnicodeInfo.Null;
                                continue;
                            }
                        }
                        break;

                    case CharUnicodeInfo.CarriageReturn:
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.SoftReturnReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.SoftReturnReplacement), culture);

                            if (nextChar == CharUnicodeInfo.LineFeed) nextChar = CharUnicodeInfo.Null;
                        }
                        break;

                    case CharUnicodeInfo.LineFeed:
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.SoftReturnReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.SoftReturnReplacement), culture);

                            if (nextChar == CharUnicodeInfo.LineFeed) nextChar = CharUnicodeInfo.Null;
                        }
                        break;

                    #endregion Paragraph

                    #region Space

                    case CharUnicodeInfo.Space:   //space
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.SpaceReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.SpaceReplacement), culture);
                        }
                        break;

                    #endregion Space

                    #region Dot

                    case CharUnicodeInfo.Dot:
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.DotReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.DotReplacement), culture);
                        }
                        break;


                    #endregion Dot

                    #region Non-breaking Space

                    case CharUnicodeInfo.NonBreakingSpace:
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.NonBreakingSpaceReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.NonBreakingSpaceReplacement), culture);
                        }
                        break;

                    #endregion Non-breaking Space

                    #region Non-breaking Hyphen

                    case CharUnicodeInfo.NonBreakingHyphen:
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.NonBreakingHyphenReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.NonBreakingHyphenReplacement), culture);
                        }
                        break;

                    #endregion Non-breaking Hyphen

                    #region Tab

                    case '\t':
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.TabReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.TabReplacement), culture);
                        }
                        break;

                    #endregion Tab

                    #region Colon

                    case ':':   //colon
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.ColonReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.ColonReplacement), culture);
                        }
                        break;

                    #endregion Colon

                    #region Comma

                    case ',': //comma
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.CommaReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.CommaReplacement), culture);
                        }
                        break;

                    #endregion Comma

                    #region Semicolon

                    case ';': //semicolon
                        {
                            if (culture == null) replacementStringArray[i] = SpecialChars.SemicolonReplacement;
                            else replacementStringArray[i] = SpecialChars.ResourceManager.GetString(ResourceHelper.GetNameOf(() => SpecialChars.SemicolonReplacement), culture);
                        }
                        break;

                    #endregion Semicolon

                    default:
                        {
                            replacementStringArray[i] = charArray[i].ToString();
                        }
                        break;
                }
            }

            return string.Join(string.Empty, replacementStringArray).Replace("  ", " ").Trim();

        }

        #endregion

        #region IsFormatItem

        public static bool IsFormatItem(this string input)
        {
            //https://msdn.microsoft.com/en-us/library/system.string.format(v=vs.110).aspx#FormatItem
            //{index[,alignment][:formatString]}
            var regex = new Regex(@"^\{(\d+)(,-?\d+)?(:[^\{\}]*)?\}$");
            return regex.IsMatch(input);
        }

        #endregion

        #region IsInDoubleQuotes

        public static bool IsInDoubleQuotes(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            string trimmedValue = value.Trim();
            if (string.IsNullOrWhiteSpace(trimmedValue)) return false;
            if (trimmedValue.Length < 2) return false;

            Char firstChar = trimmedValue.First();
            Char lastChar = trimmedValue.Last();

            //see https://de.wikipedia.org/wiki/Anf%C3%BChrungszeichen

            //"…" non-typographic
            if (firstChar.Equals(CharUnicodeInfo.DoubleQuotationMark) && lastChar.Equals(CharUnicodeInfo.DoubleQuotationMark)) return true;

            //“…” Afrikaans, Chinese, English, Hebrew, Korean, Dutch, etc.
            if (firstChar.Equals(CharUnicodeInfo.LeftDoubleQuotationMark) && lastChar.Equals(CharUnicodeInfo.RightDoubleQuotationMark)) return true;

            //„…“ Bulgarian, German, Danish, Georgian etc.
            if (firstChar.Equals(CharUnicodeInfo.DoubleLow9QuotationMark) && lastChar.Equals(CharUnicodeInfo.LeftDoubleQuotationMark)) return true;

            //„…” Estonian, Croation etc.
            if (firstChar.Equals(CharUnicodeInfo.DoubleLow9QuotationMark) && lastChar.Equals(CharUnicodeInfo.RightDoubleQuotationMark)) return true;

            //”…” Swedish 
            if (firstChar.Equals(CharUnicodeInfo.RightDoubleQuotationMark) && lastChar.Equals(CharUnicodeInfo.RightDoubleQuotationMark)) return true;

            //«…» French, Albanian, Arabian etc.
            if (firstChar.Equals(CharUnicodeInfo.LeftPointingDoubleAngleQuotationMark) && lastChar.Equals(CharUnicodeInfo.RightPointingDoubleAngleQuotationMark)) return true;

            //»…« Serbian, Slovakian, Slovenian (all secondary) etc. 
            if (firstChar.Equals(CharUnicodeInfo.RightPointingDoubleAngleQuotationMark) && lastChar.Equals(CharUnicodeInfo.LeftPointingDoubleAngleQuotationMark)) return true;

            //»…» Swedish (secondary)
            if (firstChar.Equals(CharUnicodeInfo.RightPointingDoubleAngleQuotationMark) && lastChar.Equals(CharUnicodeInfo.RightPointingDoubleAngleQuotationMark)) return true;

            //“…„ Hebrew
            if (firstChar.Equals(CharUnicodeInfo.LeftDoubleQuotationMark) && lastChar.Equals(CharUnicodeInfo.DoubleLow9QuotationMark)) return true;

            //「…」Japanese, Korean (secondary)
            if (firstChar.Equals(CharUnicodeInfo.LeftCornerBracket) && lastChar.Equals(CharUnicodeInfo.RightCornerBracket)) return true;

            //『…』Japanese (secondary)
            if (firstChar.Equals(CharUnicodeInfo.LeftWhiteCornerBracket) && lastChar.Equals(CharUnicodeInfo.RightWhiteCornerBracket)) return true;

            return false;
        }

        #endregion

        #region IsInSingleQuotes

        public static bool IsInSingleQuotes(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            string trimmedValue = value.Trim();
            if (string.IsNullOrWhiteSpace(trimmedValue)) return false;
            if (trimmedValue.Length < 2) return false;

            Char firstChar = trimmedValue.First();
            Char lastChar = trimmedValue.Last();

            //see https://de.wikipedia.org/wiki/Anf%C3%BChrungszeichen

            //'…' non-typographic
            if (firstChar.Equals(CharUnicodeInfo.Apostroph) && lastChar.Equals(CharUnicodeInfo.Apostroph)) return true;

            //‘…’ English, Afrikaans etc.
            if (firstChar.Equals(CharUnicodeInfo.LeftSingleQuotationMark) && lastChar.Equals(CharUnicodeInfo.RightSingleQuotationMark)) return true;

            //‚…‘ Bulgarian, German etc.
            if (firstChar.Equals(CharUnicodeInfo.SingleLow9QuotationMark) && lastChar.Equals(CharUnicodeInfo.LeftSingleQuotationMark)) return true;

            //’…’ Finnish etc.
            if (firstChar.Equals(CharUnicodeInfo.RightSingleQuotationMark) && lastChar.Equals(CharUnicodeInfo.RightSingleQuotationMark)) return true;

            //‹…› Spanish (secondary) etc.
            if ((firstChar.Equals(CharUnicodeInfo.LeftPointingAngleBracket) && lastChar.Equals(CharUnicodeInfo.RightPointingAngleBracket)) ||
                (firstChar.Equals(CharUnicodeInfo.SingleLeftPointingAngleQuotationMark) && lastChar.Equals(CharUnicodeInfo.SingleRightPointingAngleQuotationMark))) return true;

            //›…‹
            if ((firstChar.Equals(CharUnicodeInfo.RightPointingAngleBracket) && lastChar.Equals(CharUnicodeInfo.LeftPointingAngleBracket)) ||
                (firstChar.Equals(CharUnicodeInfo.SingleRightPointingAngleQuotationMark) && lastChar.Equals(CharUnicodeInfo.SingleLeftPointingAngleQuotationMark))) return true;

            return false;
        }

        #endregion

        #region IsInQuotes

        public static bool IsInQuotes(this string value)
        {
            return value.IsInDoubleQuotes() || value.IsInSingleQuotes();
        }

        #endregion

        #region IsInBrackets

        public static bool IsInBrackets(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            if (value.Length < 2) return false;

            return
                value.FirstChar() == '[' && value.LastChar() == ']';
        }

        #endregion IsInBrackets

        #region IsInCurlyBraces

        public static bool IsInCurlyBraces(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            if (value.Length < 2) return false;

            return
                value.FirstChar() == '{' && value.LastChar() == '}';
        }

        #endregion IsInCurlyBraces

        #region IsInParenthesis

        public static bool IsInParenthesis(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            if (value.Length < 2) return false;

            return
                value.FirstChar() == '(' && value.LastChar() == ')';
        }

        #endregion IsInParenthesis

        #region IsInSlashes

        public static bool IsInSlashes(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;
            if (value.Length < 2) return false;

            return
                value.FirstChar() == '/' && value.LastChar() == '/';
        }

        #endregion IsInSlashes

        #region IsNumeric

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Evaluates if all characters in the value are numbers. If the string contains decimal
        /// separators, the function will return false. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 27.09.2010. </remarks>
        ///
        /// <param name="value">	The value. </param>
        ///
        /// <returns>	true if numeric, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool IsNumeric(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            foreach (var c in value)
            {
                if (!char.IsNumber(c)) return false;
            }

            return true;
        }

        #endregion

        #region IsLastCharDigit

        public static bool IsLastCharDigit(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            char lastChar = value[value.Length - 1];

            return Char.IsDigit(lastChar);
        }

        #endregion IsLastCharDigit

        #region IsLastCharUpperCase

        public static bool IsLastCharUpperCase(this string value)
        {
            if (string.IsNullOrEmpty(value)) return false;

            char lastChar = value[value.Length - 1];

            return Char.IsUpper(lastChar);
        }

        #endregion IsLastCharUpperCase

        #region IsAllUppercase

        public static bool IsAllUppercase(this string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            foreach (var c in input)
            {
                if (!Char.IsUpper(c))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region IsPunctuation

        public static bool IsPunctuation(this string input, bool strictPerUnicodeDefinition = true)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.Length < 1 || input.Length > 1) return false;

            char c = input[0];
            return c.IsPunctuation(strictPerUnicodeDefinition);
        }

        public static bool IsPunctuationForTitleCasing(this string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.Length < 1 || input.Length > 1) return false;

            char c = input[0];
            return c.IsPunctuationForTitleCasing();
        }

        #endregion

        #region IsQuotationMark

        public static bool IsQuotationMark(this string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.Length < 1 || input.Length > 1) return false;

            char c = input[0];
            return c.IsQuotePunctuation(false);
        }

        #endregion

        #region IsWhitespace

        public static bool IsWhiteSpace(this string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.Length < 1 || input.Length > 1) return false;

            char c = input[0];
            return c.IsSpaceSeparator();
        }

        #endregion

        #region IsApostroph

        public static bool IsApostroph(this string input, bool strictPerUnicodeDefinition = true)
        {
            if (string.IsNullOrEmpty(input)) return false;
            if (input.Length < 1 || input.Length > 1) return false;

            char c = input[0];
            return c.IsApostroph(strictPerUnicodeDefinition);
        }

        #endregion

        #region LastChar

        public static char LastChar(this string value)
        {
            if (string.IsNullOrEmpty(value)) return Char.MinValue;
            return value[value.Length - 1];
        }

        #endregion LastChar

        #region ParseEnum

        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static bool TryParseEnum<T>(this string value, out T result, bool ignoreCase = false)
            where T : struct
        {
            var names = Enum.GetNames(typeof(T));
            var stringResult = ignoreCase ?
                names.FirstOrDefault(item => item.Equals(value, StringComparison.OrdinalIgnoreCase)) :
                names.FirstOrDefault(item => item.Equals(value, StringComparison.Ordinal));

            if (string.IsNullOrEmpty(stringResult))
            {
                if (Enum.TryParse(value, out result))
                {
                    return true;
                }
            }

            if (string.IsNullOrEmpty(stringResult))
            {
                result = default(T);
                return false;
            }

            result = ParseEnum<T>(stringResult);
            return true;
        }

        #endregion

        #region PlainTextNeedsHtmlConversion

        static char[] PlainTextNeedsHtmlConversionChars = new char[] { '\r', '\n' };

        public static bool PlainTextNeedsHtmlConversion(string text)
        {
            if (text == null) return false;
            return text.IndexOfAny(PlainTextNeedsHtmlConversionChars) != -1;
        }

        #endregion

        #region ReplaceInString

        public static string ReplaceStringFirstOccurrence(this string template, string oldValue, string newValue)
        {
            var index = template.IndexOf(oldValue);
            if (index > -1)
            {
                var finalArray = new char[template.Length + newValue.Length - oldValue.Length];
                template.CopyTo(0, finalArray, 0, index);
                newValue.CopyTo(0, finalArray, index, newValue.Length);
                template.CopyTo(index + oldValue.Length, finalArray, index + newValue.Length, template.Length - index - oldValue.Length);
                return new string(finalArray);
            }
            return template;
        }

        /// <summary>
        /// Simple routine to replace an existing character by another one. Not suitable to replace a character by \0 values or other complicated cases.
        /// </summary>
        /// <param name="charArray"></param>
        /// <param name="replacements"></param>
        public static void Replace(this char[] charArray, params (char Value, char Replacement)[] replacements)
        {
            for (int i = 0; i < charArray.Length; i++)
            {
                foreach (var replacement in replacements)
                {
                    if (charArray[i] == replacement.Value) charArray.SetValue(replacement.Replacement, i);
                }
            }
        }

        #endregion

        #region Reverse

        public static string Reverse(this string input, bool considerCombiningCharacters = false)
        {
            //combining characters are also known as surrogate pairs
            //"Les Mise\u0301rables".Reverse()      yields "selbaŕesiM seL" when printed
            //"Les Mise\u0301rables".Reverse(true)  yields "selbarésiM seL" when printed

            if (string.IsNullOrEmpty(input)) return input;

            if (considerCombiningCharacters)
            {
                return string.Join("", input.GraphemeClusters().Reverse().ToArray());
            }
            else
            {
                return string.Join("", input.ToCharArray().Reverse());
            }
        }

        private static IEnumerable<string> GraphemeClusters(this string input)
        {
            //see https://docs.microsoft.com/en-us/dotnet/api/system.globalization.stringinfo.gettextelementenumerato
            var enumerator = StringInfo.GetTextElementEnumerator(input);
            while (enumerator.MoveNext())
            {
                yield return (string)enumerator.Current;
            }
        }

        #endregion

        #region StartsWithOpenPunctuation

        public static bool StartsWithOpenPunctuation(this string value)
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.unicodecategory?view=netcore-3.1
            //Here is the list of all chars in this category: https://www.fileformat.info/info/unicode/category/Ps/list.htm

            if (value == null || !value.Any()) return false;

            return System.Globalization.CharUnicodeInfo.GetUnicodeCategory(value[0]) == UnicodeCategory.OpenPunctuation;
        }

        #endregion

        #region StartsWithInitialQuotePunctuation

        public static bool StartsWithInitialQuotePunctuation(this string value)
        {
            //https://docs.microsoft.com/en-us/dotnet/api/system.globalization.unicodecategory?view=netcore-3.1
            //Here is the list of all chars in this category: https://www.fileformat.info/info/unicode/category/Pi/list.htm

            if (value == null || !value.Any()) return false;

            //IMPORTANT: We need to cound the simple non-typographical quotation mark also as initial quote punctuation
            return System.Globalization.CharUnicodeInfo.GetUnicodeCategory(value[0]) == UnicodeCategory.OpenPunctuation || 
                value.StartsWith(CharUnicodeInfo.DoubleQuotationMark.ToString()) ||
                value.StartsWith(CharUnicodeInfo.RightPointingAngleBracket.ToString()) ||
                value.StartsWith(CharUnicodeInfo.RightPointingDoubleAngleQuotationMark.ToString());
        }

        #endregion

        #region SplitAndKeepWhitespace

        /// <summary>
        /// Similar to the Split() method, it splits a sentence at each whitespace character and returns the words PLUS the whitespace characters, so that the sentence can be re-joined later on.
        /// </summary>
        /// <param name="input">The full sentence.</param>
        /// <returns>An Enumerable of words and whitespace characters.</returns>
        public static IEnumerable<string> SplitAndKeepWhitespace(this string input)
        {
            if (input == null || !input.Any()) yield return null;

            int previousYieldPosition = -1;
            for (int i = 0; i < input.Length; i++)
            {
                var character = input.ElementAt(i);
                if (char.IsWhiteSpace(character))
                {
                    if (previousYieldPosition == -1) //nothing yielded yet
                    {
                        yield return input.Substring(0, i);
                    }
                    else if (i - previousYieldPosition > 1) //a new word, not just yet another whitespace
                    {
                        yield return input.Substring(previousYieldPosition + 1, i - previousYieldPosition - 1);
                    }
                    yield return character.ToString();
                    previousYieldPosition = i;
                }
                else if (i == input.Length - 1) //we reached the last char of the input string
                {
                    yield return input.Substring(previousYieldPosition + 1, i - previousYieldPosition);
                }
            }
        }

        #endregion

        #region SplitAndKeepWhiteSpaceAndPunctuation

        /// <summary>
        /// Splits a sentence into words and keeps both whitespace as well as punctuation characters, 
        /// so that the sentence could be reassembled again from the enumeration of strings.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitAndKeepWhitespaceAndPunctuationOld(this string input)
        {
            if (input == null || !input.Any()) yield return null;
            var punctuation = input.Where(char.IsPunctuation).Distinct().ToArray();

            var wordsAndWhitespaces = input.SplitAndKeepWhitespace();

            foreach (var word in wordsAndWhitespaces)
            {
                var word2 = word;

                var firstChar = word2.FirstChar();
                while (firstChar != char.MinValue && char.IsPunctuation(firstChar))
                {
                    yield return firstChar.ToString();
                    word2 = word2.Remove(0, 1);
                    firstChar = word2.FirstChar();
                }

                word2 = word2.Reverse();

                var lastChar = word2.FirstChar();
                var lastPunctuationChars = new List<char>();
                while (lastChar != char.MinValue && char.IsPunctuation(lastChar))
                {
                    lastPunctuationChars.Add(lastChar);
                    word2 = word2.Remove(0, 1);
                    lastChar = word2.FirstChar();
                }

                if (word2.Length > 0) yield return word2.Reverse();

                lastPunctuationChars.Reverse();
                foreach (var character in lastPunctuationChars)
                {
                    yield return character.ToString();
                }
            }
        }

        //https://docs.microsoft.com/en-us/dotnet/standard/base-types/character-classes-in-regular-expressions
        //Z is all of Zs (Separator, Space), Zl (Separator, Line) and Zp (Separator, Paragraph)
        static readonly Regex _splitWordsKeepPunctuationRegex = new Regex(@"(\p{Z})|(\p{P})", RegexOptions.IgnoreCase);

        /// <summary>
        /// Breaks the input text into a list of words at whitespaces, hyphens, brackets, parentheses, quotation marks and all kinds of punctuation.
        /// All punctuation is also returned inbetween words.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static IEnumerable<string> SplitAndKeepWhitespaceAndPunctuation(this string input, IEnumerable<string> keepAsStated = null)
        {
            if (string.IsNullOrEmpty(input)) return Enumerable.Empty<string>();
            if (keepAsStated == null || !keepAsStated.Any())
            {
                return _splitWordsKeepPunctuationRegex.Split(input).Where(word => word != string.Empty);
            }
            else
            {
                var splitPattern = string.Format(@"({0})", String.Join("|", keepAsStated.Select(x => string.Format(@"\b{0}\b", Regex.Escape(x))))) + "|" + @"(\p{Z})|(\p{P})";
                return Regex.Split(input, splitPattern, RegexOptions.IgnoreCase).Where(word => word != string.Empty);
            }
        }


        #endregion

        #region SplitAndKeepSeparators

        public static IEnumerable<string> SplitAndKeepSeparators(this string input, char[] separators, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
        {
            if (input == null || !input.Any()) yield return null;

            int previousYieldPosition = -1;

            for (int i = 0; i < input.Length; i++)
            {
                var character = input.ElementAt(i);
                if (separators.Any(separator => separator.Equals(character)))
                {
                    if (previousYieldPosition == -1 && i > 0) //nothing yielded yet
                    {
                        yield return input.Substring(0, i);
                    }
                    else if (previousYieldPosition == -1 && i == 0 && stringSplitOptions == StringSplitOptions.None) //starting with a separator
                    {
                        yield return string.Empty;
                    }
                    else if (previousYieldPosition == i - 1 && i > 0 && stringSplitOptions == StringSplitOptions.None) //two successive separators
                    {
                        yield return string.Empty;
                    }
                    else if (previousYieldPosition > -1 && previousYieldPosition < i - 1) //a new word, not just yet another separator
                    {
                        yield return input.Substring(previousYieldPosition + 1, i - previousYieldPosition - 1);
                    }
                    yield return character.ToString();
                    previousYieldPosition = i;

                    if (i == input.Length - 1 && stringSplitOptions == StringSplitOptions.None) //last char is a separator
                    {
                        yield return string.Empty;
                    }
                }
                else
                {
                    if (i == input.Length - 1) //we reached the last char of the input string
                    {
                        yield return input.Substring(previousYieldPosition + 1, i - previousYieldPosition);
                    }
                }
            }
        }

        #endregion

        #region SeparateWordsKeepSeparator

        public static string[] SeparateWordsKeepSeparator(this string value)
        {
            if (string.IsNullOrEmpty(value)) return new string[] { };
            var wordSeparatorChars = value.Where(c => char.IsPunctuation(c) || char.IsWhiteSpace(c) || char.IsSeparator(c)).ToArray();
            if (wordSeparatorChars == null || !wordSeparatorChars.Any()) return new string[] { value };

            var pattern = "(" + string.Join("|", wordSeparatorChars.Select(c => Regex.Escape(c.ToString()))) + ")";


            return Regex.Split(value, pattern).Where(s => s != String.Empty).ToArray();
        }

        #endregion

        #region UnciodeToLatin
        public static string UnciodeToLatin(this string text)
        {
            var s = new StringBuilder();
            foreach (var c in text)
            {
                if (c < 255)
                {
                    s.Append(c);
                }
                else
                {
                    s.Append(c.ToString().Normalize(NormalizationForm.FormKD).Where(i => i < 128).ToArray());
                }
            }
            return s.ToString();
        }

        #endregion

        #region ToInitialUppercase

        // DEFERRED: das ist noch völlig ungeprüft...
        public static string ToInitialUpper(this string text)
        {
            var regex = new Regex(@"\b.+?\b");
            var matches = regex.Matches(text);

            for (int i = 0; i < matches.Count; i++)
            {
                if (string.CompareOrdinal(matches[i].Value, matches[i].Value.ToUpper()) != 0)
                {
                    char c = text[matches[i].Index];

                    if (!char.IsUpper(c))
                    {
                        text = text.Substring(0, matches[i].Index) + char.ToUpper(c) + text.Substring(matches[i].Index + 1);
                    }

                    for (int j = 1; j < matches[i].Value.Length; j++)
                    {
                        c = text[matches[i].Index + j];

                        if (!char.IsLower(c))
                        {
                            text = text.Substring(0, matches[i].Index + j - 1) + char.ToUpper(c) + text.Substring(matches[i].Index + j + 1);
                        }
                    }
                }
            }

            return text;
        }

        #endregion

        #region ToUppercase

        public static string ToUppercase(this string text, int index)
        {
            if (string.IsNullOrEmpty(text)) return text;
            if (index < 0 || index >= text.Length) return text;

            if (index == 0)
            {
                return text.ToUpperFirstLetter();
            }
            if (index == text.Length - 1)
            {
                text = string.Concat(text.Substring(0, index), text.Substring(index, 1).ToUpper());
            }
            else
            {
                text = string.Concat(text.Substring(0, index), text.Substring(index, 1).ToUpper(), text.Substring(index + 1));
            }
            return text;
        }

        #endregion

        #region ToStringSafe

        public static string ToStringSafe(this object value)
        {
            //JHP Added handling of DBNulls for DatabaseTool.RepairDataInconsistency
            if (value == null || value == DBNull.Value) return string.Empty;
            return value.ToString();
        }

        #endregion

        #region ToTitleCase

        public static string ToTitleCaseCurrentCulture(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            CultureInfo currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            return currentCulture.TextInfo.ToTitleCase(text);

        }

        public static string ToTitleCaseInvariantCulture(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            CultureInfo invariantCulture = System.Globalization.CultureInfo.InvariantCulture;
            return invariantCulture.TextInfo.ToTitleCase(text);
        }

        public static string ToTitleCaseProperEnglish(this string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            string workingText = text;

            char[] space = new char[] { ' ' };

            List<string> artsAndPreps = new List<string>() { "a", "an", "and", "any", "at", "from", "into", "of", "on", "or", "some", "the", "to", };

            //Get the culture property of the thread.
            CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentCulture;

            //Create TextInfo object.
            TextInfo textInfo = cultureInfo.TextInfo;

            //Convert to title case.
            workingText = textInfo.ToTitleCase(text.ToLower());

            List<string> tokens = workingText.Split(space, StringSplitOptions.RemoveEmptyEntries).ToList();

            workingText = tokens[0];

            tokens.RemoveAt(0);

            workingText += tokens.Aggregate<String, String>(String.Empty, (String prev, String input)
                                    => prev +
                                        (artsAndPreps.Contains(input.ToLower()) // If True
                                            ? " " + input.ToLower()              // Return the prep/art lowercase
                                            : " " + input));                   // Otherwise return the valid word.

            // Handle an "Out Of" but not in the start of the sentance
            workingText = Regex.Replace(workingText, @"(?!^Out)(Out\s+Of)", "out of");

            return workingText;
        }

        #endregion ToTitleCase

        #region ToUpperFirstLetter

        /// <summary>
        /// Capitalizes the very first character in a string.
        /// </summary>
        /// <param name="source">The string whose first letter should be capitalized.</param>
        /// <param name="ensureAllButFirstIsLower">If true, the method will ensure, that all but the first character is lower case. Default is false.</param>
        /// <param name="culture">The culture for capitalizing and lower-casing. Default is null. 
        /// In Turkish e.g., "i" e.g. becomes "İ" when capitalizing and "I" becomes "ı" when lower-casing.</param>
        /// <returns>A string with a properly capitalized first letter.</returns>
        public static string ToUpperFirstLetter(this string input, bool ensureAllButFirstIsLower = false, CultureInfo culture = null)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // convert to char array of the string
            char[] letters = input.ToCharArray();

            // upper case the first char
            for (var i = 0; i < letters.Length; i++)
            {
                if (i == 0)
                {
                    if (culture == null)
                    {
                        letters[0] = char.ToUpper(letters[0]);
                    }
                    else if (culture == CultureInfo.InvariantCulture)
                    {
                        letters[0] = char.ToUpperInvariant(letters[0]);
                    }
                    else
                    {
                        letters[0] = char.ToUpper(letters[0], culture);
                    }
                    continue;
                }

                if (i > 0 && ensureAllButFirstIsLower == false) break;

                if(culture == null)
                {
                    letters[i] = char.ToLower(letters[i]);
                }
                else if (culture == CultureInfo.InvariantCulture)
                {
                    letters[i] = char.ToLowerInvariant(letters[i]);
                }
                else
                {
                    letters[i] = char.ToLower(letters[i], culture);
                }
            }

            // return the array made of the new char array
            return new string(letters);
        }

        #endregion ToUpperFirstLetter

        #region TrimBrackets

        /// <summary>
        /// Trims only one single pair of brackets from start AND end: "[...]" > "..."
        /// Unpaired brackets are ignored.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimBracketsPair(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (value.StartsWith("[") && value.EndsWith("]"))
            {
                value = value.Substring(0, value.Length - 1);
                value = value.Substring(1);
            }

            return value;
        }

        #endregion

        #region TrimParenthesis

        /// <summary>
        /// Trims only one single pair of parentheses from start AND end: "(...)" > "..."
        /// Unpaired parentheses are ignored.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TrimParenthesesPair(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            if (value.StartsWith("(") && value.EndsWith(")"))
            {
                value = value.Substring(0, value.Length - 1);
                value = value.Substring(1);
            }

            return value;
        }

        #endregion

        #region TrimEndWithChecks

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Calls value.TrimEnd([trimChars]) and checks if value.EndsWith([trimChars]) returns false.
        /// Background: if the last character is from the unicode "format" category, value.EndsWith(" ")
        /// returns true, but value.TrimEnd(' ') does not remove this space character. This may lead to
        /// an endless loop. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 04.03.2011. </remarks>
        ///
        /// <param name="value">		The value. </param>
        /// <param name="trimChars">	A variable-length parameters list containing trim chars. </param>
        ///
        /// <returns>	. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string TrimEndWithChecks(this string value, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(value)) return value;
            value = value.TrimEnd(trimChars);
            if (trimChars == null || trimChars.Length == 0) return value;

            try
            {
                var problem = false;
                foreach (var c in trimChars)
                {
                    var s = c.ToString();
                    problem = value.EndsWith(s);
                    if (problem) break;
                }

                if (!problem) return value;

                var countEnd = 0;
                if (problem)
                {
                    while (!trimChars.Any(c => c == value[value.Length - countEnd - 1])) countEnd++;
                }

                return value.Substring(0, value.Length - countEnd).TrimEnd(trimChars) + value.Substring(value.Length - countEnd);
            }

            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                return value;
            }
        }

        #endregion

        #region TrimStartWithChecks

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Calls value.TrimStart([trimChars]) and checks if value.StartsWith([trimChars]) returns false.
        /// Background: if the first character is from the unicode "format" category, value.StartsWith(" ")
        /// returns true, but value.TrimStart(' ') does not remove this space character. This may lead to
        /// an endless loop. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 04.03.2011. </remarks>
        ///
        /// <param name="value">		The value. </param>
        /// <param name="trimChars">	A variable-length parameters list containing trim chars. </param>
        ///
        /// <returns>	. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        static readonly char[] DefaultTrimChars = new[]
                {
                    CharUnicodeInfo.EmSpace,
                    CharUnicodeInfo.EnSpace,
                    CharUnicodeInfo.FourPerEmSpace,
                    CharUnicodeInfo.NarrowNoBreakSpace,
                    CharUnicodeInfo.NonBreakingSpace,
                    CharUnicodeInfo.PunctuationSpace,
                    CharUnicodeInfo.SixPerEmSpace,
                    CharUnicodeInfo.Space,
                    CharUnicodeInfo.ThreePerEmSpace,
                    CharUnicodeInfo.ZeroWidthNoBreakSpace
                };

        public static string TrimStartWithChecks(this string value, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(value)) return value;

            // Union takes care of duplicates
            trimChars = trimChars == null ?
                DefaultTrimChars :
                trimChars.Union(DefaultTrimChars).ToArray();

            value = value.TrimStart(trimChars);
            if (string.IsNullOrEmpty(value)) return value;

            try
            {
                var problem = false;
                foreach (var c in trimChars)
                {
                    var s = c.ToString();
                    problem = value.StartsWith(s, StringComparison.Ordinal);
                    if (problem) break;
                }

                if (!problem) return value;

                var countStart = 0;
                if (problem)
                {
                    while (!trimChars.Any(c => c == value[countStart])) countStart++;
                }

                return value.Substring(0, countStart) + value.Substring(countStart, value.Length - countStart).TrimStart();
            }

            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                return value;
            }
        }

        #endregion

        #region TrimWithChecks

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Calls value.Trim([trimChars]) and checks if value.StartsWith([trimChars]) and
        /// value.EndsWith([trimChars]) return false. Background: if the first or last character is from
        /// the unicode "format" category, value.StartsWith(" ") or value.EndsWith(" ")
        /// return true, but value.Trim(' ') does not remove this space character. This may lead to an
        /// endless loop. 
        /// </summary>
        ///
        /// <remarks>	Thomas Schempp, 04.03.2011. </remarks>
        ///
        /// <param name="value">		The value. </param>
        /// <param name="trimChars">	A variable-length parameters list containing trim chars. </param>
        ///
        /// <returns>	. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string TrimWithChecks(this string value, params char[] trimChars)
        {
            if (string.IsNullOrEmpty(value)) return value;
            value = value.Trim(trimChars);
            if (trimChars == null || trimChars.Length == 0) return value;

            try
            {
                var startProblem = false;
                var endProblem = false;
                foreach (var c in trimChars)
                {
                    var s = c.ToString();
                    startProblem = startProblem || value.StartsWith(s);
                    endProblem = endProblem || value.EndsWith(s);
                    if (startProblem && endProblem) break;
                }

                if (!startProblem && !endProblem) return value;

                var countStart = 0;
                if (startProblem)
                {
                    while (!trimChars.Any(c => c == value[countStart])) countStart++;
                }

                var countEnd = 0;
                if (endProblem && countStart < value.Length)
                {
                    while (!trimChars.Any(c => c == value[value.Length - countEnd - 1])) countEnd++;
                }

                return value.Substring(0, countStart) + value.Substring(countStart, value.Length - countStart - countEnd).Trim() + value.Substring(value.Length - countEnd);
            }

            catch (Exception exception)
            {
                Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                return value;
            }
        }

        #endregion

        #region Truncate

        public static string Truncate(this string value, int maxLength = 40, bool addEllipsis = true)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length <= maxLength) return value;
            if (addEllipsis && maxLength < 4) throw new ArgumentException("Invalid parameters: If addEllipsis is true, maxLength must be >= 4.");

            if (addEllipsis) return value.Substring(0, maxLength - 3) + "…";
            return value.Substring(0, maxLength);
        }

        public static string TruncateAtWordBoundary(this string value, int maxLength = 50, bool addEllipsis = true)
        {
            if (value == null || value.Length <= maxLength) return value;


            //string pattern = @"[\p{P}-[._]]"; any punctuation character except period and underscore
            string pattern = @"(\s|\p{P}|\p{Z}|\p{S})";
            string[] words = Regex.Split(value, pattern);

            //no splitting possible
            if (words.Length == 1) return value.Truncate(maxLength, addEllipsis);

            StringBuilder sb = new StringBuilder();

            foreach (string word in words)
            {
                if (sb.Length + word.Length >= maxLength) break;

                sb.Append(word);
            }
            string shortenedValue = sb.ToString();

            //splitting possible, but way too close to start of value
            if (shortenedValue.Length < maxLength * 0.7) return value.Truncate(maxLength, addEllipsis);

            if (addEllipsis) shortenedValue += "…";
            return shortenedValue;
        }

        #endregion

        #region IsValidEmail

        static Regex EMailValidationRegex = new Regex(@"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        public static bool IsValidEmail(this string self)
        {
            return EMailValidationRegex.IsMatch(self);
        }

        #endregion

        #region WrapInHtmlTag

        public static string WrapInHtmlTag(this string self, string htmlTag)
        {
            return $"<{htmlTag}>{self}</{htmlTag}>";
        }

        #endregion
    }

    #endregion

    #region BracketExpression

    public class BracketExpression
    {
        #region Constructors

        public BracketExpression(int openingBracketPosition, int closingBracketPosition, string input = null)
        {
            if (openingBracketPosition > closingBracketPosition) throw new ArgumentOutOfRangeException();

            _leftBracketPosition = openingBracketPosition;
            _rightBracketPosition = closingBracketPosition;
            _input = input;
        }

        public BracketExpression(int openingBracePosition, string input = null)
        {
            if (openingBracePosition < 0) throw new ArgumentOutOfRangeException();

            _leftBracketPosition = openingBracePosition;
            _input = input;
        }

        #endregion

        #region Properties

        #region RightBracketPosition

        int _rightBracketPosition = -1;
        public int RightBracketPosition
        {
            get
            {
                return _rightBracketPosition;
            }

            set
            {
                if (_leftBracketPosition >= 0)
                {
                    if (value < _leftBracketPosition) throw new ArgumentException("Position of closing brace must be greater than position of the opening brace.");
                }

                if (value < -1) value = -1;
                _rightBracketPosition = value;
            }
        }

        #endregion

        #region LeftBracketPosition

        int _leftBracketPosition = -1;
        public int LeftBracketPosition
        {
            get
            {
                return _leftBracketPosition;
            }

            set
            {
                if (_rightBracketPosition >= 0)
                {
                    if (value > _rightBracketPosition) throw new ArgumentException("Position of opening brace must be smaller than position of closing brace.");
                }

                if (value < -1) value = -1;
                _leftBracketPosition = value;
            }
        }

        #endregion

        #region BracketMatching

        public BracketMatching BracketMatching
        {
            get
            {
                if (_leftBracketPosition < 0 && _rightBracketPosition < 0) return BracketMatching.Undefined;
                if (_leftBracketPosition >= 0 && _rightBracketPosition < 0) return BracketMatching.LeftBracketOnly;
                if (_leftBracketPosition < 0 && _rightBracketPosition >= 0) return BracketMatching.RightBracketOnly;
                return BracketMatching.Paired;
            }
        }

        #endregion

        #region Content

        public string Content
        {
            get
            {
                if (string.IsNullOrEmpty(_input)) return null;

                switch (BracketMatching)
                {
                    case BracketMatching.Paired:
                        return _input.Substring(_leftBracketPosition, _rightBracketPosition - _leftBracketPosition + 1);

                    case BracketMatching.LeftBracketOnly:
                        return _input.Substring(_leftBracketPosition, 1);

                    case BracketMatching.RightBracketOnly:
                        return _input.Substring(_rightBracketPosition, 1);

                    default:
                        return null;

                }
            }
        }

        #endregion

        #region Input

        string _input;
        public string Input
        {
            get
            {
                return _input;
            }
        }

        #endregion

        #region IsFormatItem

        public bool IsFormatItem
        {
            get
            {
                var content = Content;
                if (string.IsNullOrEmpty(content)) return false;
                return content.IsFormatItem();
            }
        }

        #endregion

        #endregion

        #region Methods

        #region ToString

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Content))
            {
                return string.Format("{0},{1} [{2}]", _leftBracketPosition, _rightBracketPosition, BracketMatching);
            }
            else
            {
                return string.Format("{0},{1} [{2}:'{3}']", _leftBracketPosition, _rightBracketPosition, BracketMatching, Content);
            }
        }

        #endregion

        #region FindSortedBracketExpressions

        public static IEnumerable<BracketExpression> FindSortedBracketExpressions(string input, BracketType bracketType)
        {
            var tempBracketExpressionStack = new Stack<BracketExpression>();
            var foundBracketExpressions = new List<BracketExpression>();

            var previousMatch = CharUnicodeInfo.Null;

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == bracketType.Left)
                {
                    tempBracketExpressionStack.Push(new BracketExpression(i, input));

                    previousMatch = bracketType.Left;
                    continue;
                }

                if (c == bracketType.Right)
                {
                    if (previousMatch == CharUnicodeInfo.Null)
                    {
                        //unmatched right bracket
                        foundBracketExpressions.Add(new BracketExpression(-1, i, input));
                    }
                    else if (previousMatch == bracketType.Left)
                    {
                        if (tempBracketExpressionStack.Count > 0)
                        {
                            var bracePair = tempBracketExpressionStack.Pop();
                            bracePair.RightBracketPosition = i;
                            foundBracketExpressions.Add(bracePair);
                        }
                        else
                        {
                            //unmatched right bracket
                            foundBracketExpressions.Add(new BracketExpression(-1, i, input));
                            previousMatch = bracketType.Right;
                            continue;
                        }
                    }
                    else if (previousMatch == bracketType.Right)
                    {
                        if (tempBracketExpressionStack.Count > 0)
                        {
                            var bracePair = tempBracketExpressionStack.Pop();
                            bracePair.RightBracketPosition = i;
                            foundBracketExpressions.Add(bracePair);
                        }
                        else
                        {
                            //unmatched right bracket
                            foundBracketExpressions.Add(new BracketExpression(-1, i, input));
                            previousMatch = bracketType.Right;
                            continue;
                        }
                    }


                    previousMatch = bracketType.Right;
                    continue;
                }
            }


            while (tempBracketExpressionStack.Count > 0)
            {
                //unmatched left brackets
                var bracePair = tempBracketExpressionStack.Pop();
                foundBracketExpressions.Add(bracePair);
            }

            #region Sort BracePairs

            foundBracketExpressions.Sort((x, y) =>
            {
                switch (x.BracketMatching)
                {
                    case BracketMatching.Paired:
                        {
                            switch (y.BracketMatching)
                            {
                                case BracketMatching.Paired:
                                case BracketMatching.LeftBracketOnly:
                                    return x.LeftBracketPosition.CompareTo(y.LeftBracketPosition);

                                case BracketMatching.RightBracketOnly:
                                    return x.RightBracketPosition.CompareTo(y.RightBracketPosition);

                                default:
                                    return 0;
                            }
                        }

                    case BracketMatching.LeftBracketOnly:
                        {
                            switch (y.BracketMatching)
                            {
                                case BracketMatching.Paired:
                                case BracketMatching.LeftBracketOnly:
                                    return x.LeftBracketPosition.CompareTo(y.LeftBracketPosition);

                                case BracketMatching.RightBracketOnly:
                                    return x.LeftBracketPosition.CompareTo(y.RightBracketPosition);

                                default:
                                    return 0;
                            }
                        }

                    case BracketMatching.RightBracketOnly:
                        {
                            switch (y.BracketMatching)
                            {
                                case BracketMatching.Paired:
                                case BracketMatching.RightBracketOnly:
                                    return x.RightBracketPosition.CompareTo(y.RightBracketPosition);

                                case BracketMatching.LeftBracketOnly:
                                    return x.RightBracketPosition.CompareTo(y.LeftBracketPosition);

                                default:
                                    return 0;
                            }
                        }

                    default:
                        return 0;
                }
            });

            #endregion

            return foundBracketExpressions;
        }

        #endregion

        #endregion
    }

    #endregion

    #region BracketType

    public abstract class BracketType
    {
        #region Properties

        public abstract char Left { get; }

        public abstract char Right { get; }

        #endregion

        #region Static Instances

        static List<BracketType> _all;

        public static IEnumerable<BracketType> All
        {
            get
            {
                if (_all == null)
                {
                    _all = new List<BracketType>
                    {
                        Parentheses,
                        SquareBrackets,
                        CurlyBrackets
                    };
                }

                return _all;
            }
        }

        public static Parentheses Parentheses
        {
            get { return Parentheses.Default; }
        }

        public static SquareBrackets SquareBrackets
        {
            get { return SquareBrackets.Default; }
        }

        public static CurlyBrackets CurlyBrackets
        {
            get { return CurlyBrackets.Default; }
        }

        #endregion
    }

    public sealed class Parentheses : BracketType
    {
        static Parentheses _default = new Parentheses();
        public static Parentheses Default
        {
            get { return _default; }
        }

        public override char Left
        {
            get { return CharUnicodeInfo.LeftParenthesis; }
        }

        public override char Right
        {
            get { return CharUnicodeInfo.RightParenthesis; }
        }
    }

    public sealed class SquareBrackets : BracketType
    {
        static SquareBrackets _default = new SquareBrackets();
        public static SquareBrackets Default
        {
            get { return _default; }
        }

        public override char Left
        {
            get { return CharUnicodeInfo.LeftSquareBracket; }
        }

        public override char Right
        {
            get { return CharUnicodeInfo.RightSquareBracket; }
        }
    }

    public sealed class CurlyBrackets : BracketType
    {
        static CurlyBrackets _default = new CurlyBrackets();
        public static CurlyBrackets Default
        {
            get { return _default; }
        }

        public override char Left
        {
            get { return CharUnicodeInfo.LeftCurlyBracket; }
        }

        public override char Right
        {
            get { return CharUnicodeInfo.RightCurlyBracket; }
        }
    }

    #endregion

    #region StringComparerEx

    public class StringComparerEx
    :
    IComparer<string>
    {
        #region Constructors

        public StringComparerEx()
        {
        }

        #endregion

        #region Properties

        #region CompareTrailingOrderInfo

        public bool CompareTrailingOrderInfo { get; set; }

        #endregion

        #region StringComparer

        IComparer<string> _comparer;

        public IComparer<string> Comparer
        {
            get { return LazyInitializer.EnsureInitialized(ref _comparer, () => StringComparer.CurrentCulture); }
            set { _comparer = value; }
        }

        #endregion

        #endregion

        #region Methods

        #region Compare

        public int Compare(string x, string y)
        {
            try
            {
                if (!CompareTrailingOrderInfo)
                {
                    return Comparer.Compare(x, y);
                }

                if (string.IsNullOrWhiteSpace(x) || string.IsNullOrWhiteSpace(y))
                {
                    return Comparer.Compare(x, y);
                }

                if (x.Equals(y, StringComparison.OrdinalIgnoreCase))
                {
                    return Comparer.Compare(x, y);
                }

                var xDotIndex = x.LastIndexOf('.');
                var xSpaceIndex = xDotIndex == -1 ?
                    x.LastIndexOf(' ') :
                    x.LastIndexOf(' ', xDotIndex - 1);

                var yDotIndex = y.LastIndexOf('.');
                var ySpaceIndex = yDotIndex == -1 ?
                    y.LastIndexOf(' ') :
                    y.LastIndexOf(' ', yDotIndex - 1);

                if (xSpaceIndex == -1 && ySpaceIndex == -1)
                {
                    return Comparer.Compare(x, y);
                }

                int firstResult;
                if (xSpaceIndex == -1)
                {
                    firstResult = Comparer.Compare(x.Substring(0, xDotIndex), y.Substring(0, ySpaceIndex));
                    if (firstResult != 0) return Comparer.Compare(x, y);
                }
                else if (ySpaceIndex == -1)
                {
                    firstResult = Comparer.Compare(x.Substring(0, xSpaceIndex), y.Substring(0, yDotIndex));
                    if (firstResult != 0) return Comparer.Compare(x, y);
                }
                else
                {
                    firstResult = Comparer.Compare(x.Substring(0, xSpaceIndex), y.Substring(0, ySpaceIndex));
                    if (firstResult != 0) return Comparer.Compare(x, y);
                }

                if (Uri.TryCreate(x, UriKind.RelativeOrAbsolute, out var xUri) &&
                    Uri.TryCreate(y, UriKind.RelativeOrAbsolute, out var yUri))
                {
                    if ((xUri.IsAbsoluteUri && !yUri.IsAbsoluteUri) ||
                        (!xUri.IsAbsoluteUri && yUri.IsAbsoluteUri))
                    {
                        return Comparer.Compare(x, y);
                    }

                    if ((xUri.IsAbsoluteUri && !xUri.IsFile) ||
                        (yUri.IsAbsoluteUri && !yUri.IsFile))
                    {
                        return Comparer.Compare(x, y);
                    }
                }
                else
                {
                    return Comparer.Compare(x, y);
                }

                var xExtension = Path.GetExtension(x);
                var yExtension = Path.GetExtension(y);

                var xName = string.IsNullOrEmpty(xExtension) ? x : Path.ChangeExtension(x, null);
                var yName = string.IsNullOrEmpty(yExtension) ? y : Path.ChangeExtension(y, null);
                string xOrderInfo = null;
                string yOrderInfo = null;

                var match = TrailingOrderInfoInParenthesesRegex.MatchWithCache(xName);
                if (match.Success)
                {
                    xName = match.Groups["Name"].Value;
                    xOrderInfo = match.Groups["OrderInfo"].Value;
                }
                else
                {
                    match = TrailingOrderInfoRegex.MatchWithCache(xName);
                    if (match.Success)
                    {
                        xName = match.Groups["Name"].Value;
                        xOrderInfo = match.Groups["OrderInfo"].Value;
                    }
                }

                match = TrailingOrderInfoInParenthesesRegex.MatchWithCache(yName);
                if (match.Success)
                {
                    yName = match.Groups["Name"].Value;
                    yOrderInfo = match.Groups["OrderInfo"].Value;
                }
                else
                {
                    match = TrailingOrderInfoRegex.MatchWithCache(yName);
                    if (match.Success)
                    {
                        yName = match.Groups["Name"].Value;
                        yOrderInfo = match.Groups["OrderInfo"].Value;
                    }
                }

                var nameCompareResult = Comparer.Compare(xName, yName);
                if (nameCompareResult != 0)
                {
                    return nameCompareResult;
                }

                var extensionResult = Comparer.Compare(xExtension, yExtension);
                if (extensionResult != 0)
                {
                    return extensionResult;
                }

                if (string.IsNullOrEmpty(xOrderInfo))
                {
                    return string.IsNullOrEmpty(yOrderInfo) ? 0 : -1;
                }

                if (string.IsNullOrEmpty(yOrderInfo))
                {
                    return 1;
                }

                int xNumber;
                int yNumber;
                if (int.TryParse(xOrderInfo, out xNumber))
                {
                    if (int.TryParse(yOrderInfo, out yNumber))
                    {
                        return xNumber.CompareTo(yNumber);
                    }
                }

                return Comparer.Compare(xOrderInfo, yOrderInfo);
            }
            catch
            {
                return Comparer.Compare(x, y);
            }
        }

        #endregion

        #endregion

        #region Static Instances

        #region CurrentCultureCompareTrailingOrderInfo

        static StringComparerEx _currentCultureCompareTrailingOrderInfo;

        public static StringComparerEx CurrentCultureCompareTrailingOrderInfo
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _currentCultureCompareTrailingOrderInfo, () =>
                {
                    return new StringComparerEx
                    {
                        Comparer = StringComparer.CurrentCulture,
                        CompareTrailingOrderInfo = true
                    };
                });
            }
        }

        #endregion

        #region CurrentCultureIgnoreCaseCompareTrailingOrderInfo

        static StringComparerEx _currentCultureIgnoreCaseCompareTrailingOrderInfo;

        public static StringComparerEx CurrentCultureIgnoreCaseCompareTrailingOrderInfo
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _currentCultureIgnoreCaseCompareTrailingOrderInfo, () =>
                {
                    return new StringComparerEx
                    {
                        Comparer = StringComparer.CurrentCultureIgnoreCase,
                        CompareTrailingOrderInfo = true
                    };
                });
            }
        }

        #endregion

        #region OrdinalCompareTrailingOrderInfo

        static StringComparerEx _ordinalCompareTrailingOrderInfo;

        public static StringComparerEx OrdinalCompareTrailingOrderInfo
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _ordinalCompareTrailingOrderInfo, () =>
                {
                    return new StringComparerEx
                    {
                        Comparer = StringComparer.Ordinal,
                        CompareTrailingOrderInfo = true
                    };
                });
            }
        }

        #endregion

        #region OrdinalIgnoreCaseCompareTrailingOrderInfo

        static StringComparerEx _ordinalIgnoreCaseCompareTrailingOrderInfo;

        public static StringComparerEx OrdinalIgnoreCaseCompareTrailingOrderInfo
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _ordinalIgnoreCaseCompareTrailingOrderInfo, () =>
                {
                    return new StringComparerEx
                    {
                        Comparer = StringComparer.OrdinalIgnoreCase,
                        CompareTrailingOrderInfo = true
                    };
                });
            }
        }

        #endregion

        public static readonly Regex TrailingOrderInfoInParenthesesRegex = new Regex(@"(?<Name>.*)[\s_-]+\((?<OrderInfo>\w{1,3})\)$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        public static readonly Regex TrailingDigitInParenthesesRegex = new Regex(@"(?<Name>.*)\s+\((?<Digit>[\d]+)\)$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);
        public static readonly Regex TrailingOrderInfoRegex = new Regex(@"(?<Name>.*)[\s_-]+(?<OrderInfo>\w{1,3})$", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);

        #endregion
    }

    public static class RegexWithCache
    {
        public static Match MatchWithCache(this Regex regex, string input)
        {
            var cache = CallContext2.GetData("StringCompareDictionary") as Dictionary<Regex, Dictionary<string, Match>>;
            if (cache == null)
            {
                cache = new Dictionary<Regex, Dictionary<string, Match>>();
                CallContext2.SetData("StringCompareDictionary", cache);
            }

            Dictionary<string, Match> dict;
            if (!cache.TryGetValue(regex, out dict))
            {
                dict = new Dictionary<string, Match>();
                cache[regex] = dict;
            }

            Match match;
            if (!dict.TryGetValue(input, out match))
            {
                match = regex.Match(input);
                dict[input] = match;
            }

            return match;
        }
    }

    #endregion
}
