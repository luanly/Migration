using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace SwissAcademic.Globalization
{
    public static class CharUnicodeInfo
    {
        #region Char Constants

        //see http://www.langeneggers.ch/nuetzliches/umrechner-hex-dez.html
        //and http://www.fileformat.info/info/unicode

        /// <summary>
        /// '\r', 0x000D
        /// See http://www.fileformat.info/info/unicode/char/000D/index.htm
        /// </summary>
        public const char CarriageReturn = (char)13;


        public const char Dot = '.';
        public const char Ellipsis = '…';



        public const char Space = ' ';
        public const char LineFeed = (char)10;           // http://www.fileformat.info/info/unicode/char/000A/index.htm, '\n'
        public const char NonBreakingSpace = (char)0xA0;			//'\u00a0'
        public const char Tab = (char)0x0009;       // Character Tabulation, Horizontal Tabulation, HT
        public const char EmQuad = (char)2001;          // http://www.cs.tut.fi/~jkorpela/chars/spaces.html
        public const char EmSpace = (char)2003;         // http://www.cs.tut.fi/~jkorpela/chars/spaces.html
        public const char EnQuad = (char)2000;          // http://www.cs.tut.fi/~jkorpela/chars/spaces.html
        public const char EnSpace = (char)2002;         // http://www.cs.tut.fi/~jkorpela/chars/spaces.html

        /// <summary>
        /// Unicode: '\u002d' Glyph: -
        /// </summary>
        public const char Divis = (char)45;

        /// <summary>
        /// Unicode: '\u2010' Glyph: ‐  DE: Viertelgeviertstrich
        /// </summary>
        public const char Hyphen = (char)8208;

        /// <summary>
        /// Unicode: '\u2011' Glyph: ‐  NOTE: MS Word uses '\u001e'
        /// </summary>
        public const char NonBreakingHyphen = (char)0x2011;

        /// <summary>
        /// Unicode: '\u2012' Glyph: ‒  DE: Ziffernbreiter Gedankenstrich
        /// </summary>
        public const char FigureDash = (char)8210;

        /// <summary>
        /// Unicode: '\u2013' Glyph: –  DE: Halbgeviertstrich
        /// </summary>
        public const char EnDash = (char)8211;

        /// <summary>
        /// Unicode: '\u2014' Glyph: —  DE: Geviertstrich
        /// </summary>
        public const char EmDash = (char)8212;

        /// <summary>
        /// Unicode: '\u2015' Glyph: —  DE: Waagerechter Strich
        /// </summary>
        public const char HorizontalBar = (char)8213;

        /// <summary>
        /// Unicode: '\u2e3a' Glyph: ⸺  DE: Doppelgeviertstrich
        /// </summary>
        public const char TwoEmDash = (char)11834;

        /// <summary>
        /// Unicode: '\u2e3b' Glyph: ⸻  DE: Dreifachgeviertstrich
        /// </summary>
        public const char ThreeEmDash = (char)11835;

        /// <summary>
        /// Unicode: '\u00ad' Glyph: ‐  NOTE: MS Word uses '\u001f'
        /// </summary>
        public const char SoftHyphen = (char)0xad;

        /// <summary>
        /// Unicode: '\u002d' Glyph: -  DE: Bindestrich-Minuszeichen, Divis
        /// </summary>
        public const char HyphenMinus = (char)45;

        /// <summary>
        /// Unicode: '\u2212' Glyph: −
        /// </summary>
        public const char MinusSign = (char)8722;

        public const char InvertedQuestionMark = (char)0x00BF;
        public const char QuestionMark = (char)0x003F;

        public const char Period = (char)0x002E;
        public const char FullStop = (char)0x002E;

        public const char ExclamationMark = (char)0x0021;
        public const char InvertedExclamationMark = (char)0x00A1;

        public const char Colon = (char)0x003A;

        public const char NarrowNoBreakSpace = (char)0x202F;       //'\u202f http://support.citavi.com/forum/viewtopic.php?f=156&t=9038
        public const char ZeroWidthNoBreakSpace = (char)0xFEFF;     //'\ufeff'
        public const char ZeroWidthBreakOpportunity = (char)0x200B;     // http://en.wikipedia.org/wiki/Zero-width_space '\u200B' or ALT+08203
        public const char WordJoiner = (char)8288;          // https://en.wikipedia.org/wiki/Word_joiner

        public const char Ampersand = (char)0x26;           //'\u0026' &

        public const char VerticalLine = (char)0x7C;            //'\u007C' | 
        public const char Solidus = (char)0x2F;         //'\u002f' / Slash

        public const char LineSeparator = (char)0x2028;       // http://www.fileformat.info/info/unicode/char/2028/index.htm, '\u2028' or: '\v', new line, vertical tab, Chr(11) in VBA
        public const char ParagraphSeparator = (char)0x2029;       // http://www.fileformat.info/info/unicode/char/2029/index.htm, '\u2029'

        public const char ParagraphSign = (char)0x00B6;       // http://www.fileformat.info/info/unicode/char/00b6/index.htm, '\u00B6', Absatzzeichen, Pilcrow Sign
        public const char PilcrowSign = (char)0x00B6;
        public const char NewlineSign = (char)0x21B5;       // http://www.fileformat.info/info/unicode/char/21b5/index.htm, '\u21B5', Zeilenschaltungszeichen
        public const char TabSign = (char)0x21E5;       // Rightwards Arrow To Bar (rightward tab)
        public const char Null = (char)0x0;         //'\u0000' NULL


        //see http://xahlee.info/comp/unicode_matching_brackets.html

        /// <summary>
        /// Unicode: '\u0028' Glyph: (
        /// </summary>
        public const char LeftParenthesis = (char)0x28;
        /// <summary>
        /// Unicode: '\u0029' Glyph: )
        /// </summary>
        public const char RightParenthesis = (char)0x29;

        /// <summary>
        /// Unicode: '\u005b' Glyph: [
        /// </summary>
        public const char LeftSquareBracket = (char)0x5B;
        /// <summary>
        /// Unicode: '\u005d' Glyph: ]
        /// </summary>
        public const char RightSquareBracket = (char)0x5D;

        /// <summary>
        /// Unicode: '\u007b' Glyph: {
        /// </summary>
        public const char LeftCurlyBracket = (char)0x7B;
        /// <summary>
        /// Unicode: '\u007d' Glyph: }
        /// </summary>
        public const char RightCurlyBracket = (char)0x7D;

        /// <summary>
        /// Unicode: '\uff08' Glyph: （
        /// </summary>
        public const char FullWidthLeftParenthesis = (char)0xFF08;
        /// <summary>
        /// Unicode: '\uff09' Glyph: ）
        /// </summary>
        public const char FullWidhtRightParenthesis = (char)0xFF09;

        /// <summary>
        /// Unicode: '\uff3b' Glyph: ［
        /// </summary>
        public const char FullWidthLeftSquareBracket = (char)0xFF3B;
        /// <summary>
        /// Unicode: '\uff3d' Glyph: ］
        /// </summary>
        public const char FullWidthRightSquareBracket = (char)0xFF3D;

        /// <summary>
        /// Unicode: '\uff5b' Glyph: ｛
        /// </summary>
        public const char FullWidthLeftCurlyBracket = (char)0xFF5B;
        /// <summary>
        /// Unicode: '\uff5d' Glyph: ｝
        /// </summary>
        public const char FullWidthRightCurlyBracket = (char)0xFF5D;

        /// <summary>
        /// Unicode: '\u2985' Glyph: ⦅
        /// </summary>
        public const char LeftWhiteParenthesis = (char)0x2985;
        /// <summary>
        /// Unicode: '\u2986' Glyph: ⦆
        /// </summary>
        public const char RightWhiteParenthesis = (char)0x2986;

        /// <summary>
        /// Unicode: '\u301a' Glyph: 〚
        /// </summary>
        public const char LeftWhiteSquareBracket = (char)0x301A;
        /// <summary>
        /// Unicode: '\u301b' Glyph: 〛
        /// </summary>
        public const char RightWhiteSquareBracket = (char)0x301B;

        /// <summary>
        /// Unicode: '\u2983' Glyph: ⦃
        /// </summary>
        public const char LeftWhiteCurlyBracket = (char)0x2983;
        /// <summary>
        /// Unicode: '\u2984' Glyph: ⦄
        /// </summary>
        public const char RightWhiteCurlyBracket = (char)0x2984;

        /// <summary>
        /// Unicode: '\u300c' Glyph: 「
        /// </summary>
        public const char LeftCornerBracket = (char)0x300C;
        /// <summary>
        /// Unidcoe: '\u300d' Glyph: 」
        /// </summary>
        public const char RightCornerBracket = (char)0x300D;

        /// <summary>
        /// Unicode: '\u3008' Glyph: 〈
        /// </summary>
        public const char LeftAngleBracket = (char)0x3008;
        /// <summary>
        /// Unicode: '\u3009' Glyph: 〉
        /// </summary>
        public const char RightAngleBracket = (char)0x3009;

        /// <summary>
        /// Unicode: '\u300a' Glyph: 《
        /// </summary>
        public const char LeftDoubleAngleBracket = (char)0x300A;
        /// <summary>
        /// Unicode: '\u300b' Glyph: 》
        /// </summary>
        public const char RightDoubleAngleBracket = (char)0x300B;

        /// <summary>
        /// Unicode: '\u3010' Glyph: 【
        /// </summary>
        public const char LeftBlackLenticularBracket = (char)0x3010;
        /// <summary>
        /// Unicode: '\u3011' Glyph: 】
        /// </summary>
        public const char RightBlackLenticularBracket = (char)0x3011;

        /// <summary>
        /// Unicode: '\u3014' Glyph: 〔
        /// </summary>
        public const char LeftTortoiseShellBracket = (char)0x3014;
        /// <summary>
        /// Unicode: '\u3015' Glyph: 〕
        /// </summary>
        public const char RightTortoiseShellBracket = (char)0x3015;

        /// <summary>
        /// Unicode: '\u2997' Glyph: ⦗
        /// </summary>
        public const char LeftBlackTortoiseShellBracket = (char)0x2997;
        /// <summary>
        /// Unicode: '\u2998' Glyph: ⦘
        /// </summary>
        public const char RightBlackTortoiseShellBracket = (char)0x2998;

        /// <summary>
        /// Unicode: '\u300e' Glyph: 『
        /// </summary>
        public const char LeftWhiteCornerBracket = (char)0x300e;
        /// <summary>
        /// Unicode: '\u300f' Glyph: 』
        /// </summary>
        public const char RightWhiteCornerBracket = (char)0x300f;

        /// <summary>
        /// Unicode: '\u3016' Glyph: 〖
        /// </summary>
        public const char LeftWhiteLenticularBracket = (char)0x3016;
        /// <summary>
        /// Unicode: '\u3017' Glyph: 〗
        /// </summary>
        public const char RightWhiteLenticularBracket = (char)0x3017;

        /// <summary>
        /// Unicode: '\u3018' Glyph: 〘
        /// </summary>
        public const char LeftWhiteTortoiseShellBracket = (char)0x3018;
        /// <summary>
        /// Unicode: '\u3019' Glyph: 〙
        /// </summary>
        public const char RightWhiteTortoiseShellBracket = (char)0x3019;

        /// <summary>
        /// Unidcode: '\uFF62' Glyph: ｢
        /// </summary>
        public const char HalfWidthLeftCornerBracket = (char)0xFF62;
        /// <summary>
        /// Unicode: '\uFF63' Glyph: ｣
        /// </summary>
        public const char HalfWidthRightCornerBracket = (char)0xFF63;

        /// <summary>
        /// Unicode: '\u2329' Glyph: 〈
        /// </summary>
        public const char LeftPointingAngleBracket = (char)0x2329;
        /// <summary>
        /// Unicode: '\u232A' Glyph: 〉
        /// </summary>
        public const char RightPointingAngleBracket = (char)0x232A;




        //see http://en.wikipedia.org/wiki/Non-English_usage_of_quotation_marks
        /// <summary>
        /// Unicode: '\u0022' Glyph: " (non-typographic quotation mark)
        /// </summary>
        public const char DoubleQuotationMark = (char)0x22;

        /// <summary>
        /// Unicode: '\u0027' Glyph: ' (non-typographic quotation mark)
        /// </summary>
        public const char Apostroph = (char)0x27;

        public const char ModifierLetterApostrophe = (char)0x02BC;
        public const char ModifierLetterVerticalLine = (char)0x02C8;
        public const char ModifierLetterTurnedComma = (char)0x02BB;
        public const char AcuteAccent = (char)0x00B4;
        public const char GraveAccent = (char)0x0060;
        public const char Prime = (char)0x2032;

        /// <summary>
        /// Unicode: '\u201C' Glyph: “
        /// </summary>
        public const char LeftDoubleQuotationMark = (char)0x201C;
        /// <summary>
        /// Unicode: '\u201D' Glyph: ”
        /// </summary>
        public const char RightDoubleQuotationMark = (char)0x201D;
        /// <summary>
        /// Unicode: '\u201E' Glyph: „
        /// Windows: Alt+0132
        /// HTML: &amp;bdquo;
        /// </summary>
        public const char DoubleLow9QuotationMark = (char)0x201E;
        /// <summary>
        /// Unicode: '\u201F' Glyph: ‟
        /// </summary>
        public const char DoulbeHighReversed9QuotationMark = (char)0x201F;
        /// <summary>
        /// Unicode: '\u2E42' Glyph: ⹂
        /// </summary>
        public const char DoubleLowReversed9QuotationMark = (char)0x2E42;


        /// <summary>
        /// Unicode: '\u2018' Glyph: ‘
        /// </summary>
        public const char LeftSingleQuotationMark = (char)0x2018;
        /// <summary>
        /// Unicode: '\u2019' Glyph: ’
        /// </summary>
        public const char RightSingleQuotationMark = (char)0x2019;
        /// <summary>
        /// Unicode: '\u201A' Glyph: ‚
        /// </summary>
        public const char SingleLow9QuotationMark = (char)0x201A;

        /// <summary>
        /// Unicode: '\u00AB' Glyph: «
        /// </summary>
        public const char LeftPointingDoubleAngleQuotationMark = (char)0xAB;
        /// <summary>
        /// Unicode: '\u00BB' Glyph: »
        /// </summary>
        public const char RightPointingDoubleAngleQuotationMark = (char)0xBB;

        /// <summary>
        /// Unicode: '\u2039' Glyph: ‹
        /// </summary>
        public const char SingleLeftPointingAngleQuotationMark = (char)0x2039;
        /// <summary>
        /// Unicode: '\u203A' Glyph: ›
        /// </summary>
        public const char SingleRightPointingAngleQuotationMark = (char)0x203A;

        /// <summary>
        /// Unicode: '\u301D' Glyph: 〝
        /// </summary>
        public const char ReversedDoublePrimeQuotationMark = (char)0x301D;
        /// <summary>
        /// Unicode: '\u301E' Glyph: 〞
        /// </summary>
        public const char DoublePrimeQuotationMark = (char)0x301E;
        /// <summary>
        /// Unicode: '\u301F' Glyph: 〟
        /// </summary>
        public const char LowDoublePrimeQuotationMark = (char)0x301F;


        /// <summary>
        /// http://www.cs.tut.fi/~jkorpela/chars/spaces.html
        /// </summary>
        public const char ThreePerEmSpace = (char)0x2004;
        /// <summary>
        /// http://www.cs.tut.fi/~jkorpela/chars/spaces.html
        /// </summary>
        public const char FourPerEmSpace = (char)0x2005;
        /// <summary>
        ///  http://www.cs.tut.fi/~jkorpela/chars/spaces.html
        /// </summary>
        public const char SixPerEmSpace = (char)0x2006;
        /// <summary>
        ///  http://www.cs.tut.fi/~jkorpela/chars/spaces.html
        /// </summary>
        public const char PunctuationSpace = (char)0x2008;


        public const char AsposeWordsDefaultBullet = (char)61623;

        #endregion Char Constants

        #region Fields

        static Type CharUnicodeInfoType;
        static BindingFlags BindingFlags;
        static MethodInfo GetBidiCategoryMethod;

        #endregion

        #region Constructors

        static CharUnicodeInfo()
        {
            CharUnicodeInfoType = typeof(System.Globalization.CharUnicodeInfo);
            BindingFlags = BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.InvokeMethod;
            GetBidiCategoryMethod = CharUnicodeInfoType.GetMethod("GetBidiCategory", BindingFlags, null, new[] { typeof(string), typeof(int) }, null);
        }

        #endregion

        #region Methods

        #region GetBidiCategorySimplified

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>	Returns the <typeparamref name="BidiCategorySimplified"/ of a character>. </summary>
        ///
        /// <remarks>	Thomas Schempp, 30.11.2011. </remarks>
        ///
        /// <param name="value">	The text to pick the character from. </param>
        /// <param name="index">	Zero-based index of the character in <paramref name="value"/>. </param>
        ///
        /// <returns>	The <typeparamref name="BidiCategorySimplified"/> of the character. </returns>
        #endregion

        public static BidiCategorySimplified GetBidiCategorySimplified(string value, int index)
        {
            var bidiCategory = GetBidiCategoryReflected(value, index);

            switch (bidiCategory)
            {
                case BidiCategory.ArabicNumber:
                case BidiCategory.RightToLeft:
                case BidiCategory.RightToLeftArabic:
                case BidiCategory.RightToLeftEmbedding:
                case BidiCategory.RightToLeftOverride:
                    return BidiCategorySimplified.RightToLeft;

                //case BidiCategory.EuropeanNumber:
                //case BidiCategory.EuropeanNumberSeparator:
                //case BidiCategory.EuropeanNumberTerminator:
                case BidiCategory.LeftToRight:
                case BidiCategory.LeftToRightEmbedding:
                case BidiCategory.LeftToRightOverride:
                    return BidiCategorySimplified.LeftToRight;

                // TODO Thomas RTL: PopDirectionalFormat should be treated differently ...
                // perhaps others, too?
                default:
                    return BidiCategorySimplified.Neutral;
            }
        }

        #endregion

        #region GetBidiCategoryReflected

        // See http://blogs.msdn.com/b/michkap/archive/2007/01/06/1421178.aspx

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>	Returns the <typeparamref name="BidiCategory"/> of a character. </summary>
        ///
        /// <remarks>	Thomas.schempp, 29.11.2011. </remarks>
        ///
        /// <param name="value">	The text to pick the character from. </param>
        /// <param name="index">	Zero-based index of the character in <paramref name="value"/>. </param>
        ///
        /// <returns>	The <typeparamref name="BidiCategory"/> of the character. </returns>
        #endregion

        public static BidiCategory GetBidiCategoryReflected(string value, int index)
        {
            var parameters = new object[2] { value, index };
            return (BidiCategory)GetBidiCategoryMethod.Invoke(CharUnicodeInfoType, BindingFlags, null, parameters, CultureInfo.InvariantCulture);
        }

        #endregion

        #region GetUnicodeInfo

        public static UnicodeCategory GetUnicodeCategory(char input)
        {
            return System.Globalization.CharUnicodeInfo.GetUnicodeCategory(input);
        }

        #endregion

        #region IsApostroph

        /// <summary>
        /// Characters that are used as apostrophs.
        /// </summary>
        public static readonly char[] Apostrophs = {
            Apostroph                       //non-typographic
            , RightSingleQuotationMark      //typographic
            , LeftSingleQuotationMark
            , ModifierLetterApostrophe
            , ModifierLetterVerticalLine
            , AcuteAccent
            , GraveAccent
            , Prime
            , ModifierLetterTurnedComma
        };

        public static bool IsApostroph(this char input, bool strictPerUnicodeDefinition = true)
        {
            if (strictPerUnicodeDefinition) return input == Apostroph;

            return Apostrophs.Any(c => c == input);
        }

        #endregion

        #region IsFinalQuotePunctuation

        /// <summary>
        /// Format character that affects the layout of text or the operation of text processes, but is not normally rendered. 
        /// Signified by the Unicode designation "Pf" (other, format).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsFinalQuotePunctuation(this char input, bool strictPerUnicodeDefinition = true)
        {
            if (strictPerUnicodeDefinition) return GetUnicodeCategory(input) == UnicodeCategory.FinalQuotePunctuation;

            //https://graphemica.com/categories/final-quote-punctuation
            return GetUnicodeCategory(input) == UnicodeCategory.FinalQuotePunctuation ||
                input == DoubleQuotationMark ||
                input == LeftPointingAngleBracket ||
                input == LeftPointingDoubleAngleQuotationMark;
        }

        #endregion

        #region IsCJKUnifiedIdeograph / IsChinese

        public static bool IsChinese(this char input)
        {
            return input.IsCJKUnifiedIdeograph() ||
                input.IsCJKUnifiedIdeographsExtensionA() ||
                input.IsCJKCompatibility() ||
                input.IsCJKSymbolsandPunctuation() ||
                input.IsCJKRadicalsSupplement();
        }

        public static bool IsCJKUnifiedIdeograph(this char input)
        {
            //https://jrgraphix.net/research/unicode_blocks.php
            //https://docs.microsoft.com/en-us/dotnet/standard/base-types/character-classes-in-regular-expressions
            //4E00 - 9FFF	IsCJKUnifiedIdeographs

            return (int)input >= 0x4E00 && (int)input <= 0x9FFF;
        }

        public static bool IsCJKUnifiedIdeographsExtensionA(this char input)
        {
            //3400 - 4DBF IsCJKUnifiedIdeographsExtensionA
            return (int)input >= 0x3300 && (int)input <= 0x33FF;
        }

        public static bool IsCJKRadicalsSupplement(this char input)
        {
            //2E80 - 2EFF IsCJKRadicalsSupplement
            return (int)input >= 0x2E80 && (int)input <= 0x2EFF;
        }

        public static bool IsCJKSymbolsandPunctuation(this char input)
        {
            //3000 - 303F	IsCJKSymbolsandPunctuation
            return (int)input >= 0x3000 && (int)input <= 0x303F;
        }

        public static bool IsCJKCompatibility(this char input)
        {
            //3300 - 33FF	IsCJKCompatibility
            return (int)input >= 0x3300 && (int)input <= 0x33FF;
        }

        #endregion

        #region IsClosePunctuation

        /// <summary>
        /// Closing character of one of the paired punctuation marks, such as parentheses, square brackets, and braces.
        /// Signified by the Unicode designation "Pe" (punctuation, close).
        /// </summary>
        /// <param name="input"></param>
        /// <returns>true or false</returns>
        public static bool IsClosePunctuation(this char input, bool strictPerUnicodeDefinition = true)
        {
            if (strictPerUnicodeDefinition) return GetUnicodeCategory(input) == UnicodeCategory.ClosePunctuation;

            //https://graphemica.com/categories/close-punctuation
            //we exclude some quotation marks that are - for some reason - categorized as ClosePunctuation instead of FinalQuotePunctuation
            return GetUnicodeCategory(input) == UnicodeCategory.ClosePunctuation && !(
                input == DoublePrimeQuotationMark ||
                input == LowDoublePrimeQuotationMark
            );
        }

        #endregion

        #region IsConnectorPunctuation

        /// <summary>
        /// Connector punctuation character that connects two characters. 
        /// Signified by the Unicode designation "Pc" (punctuation, connector). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>true or false</returns>
        public static bool IsConnectorPunctuation(this char input)
        {
            //https://graphemica.com/categories/connector-punctuation
            return GetUnicodeCategory(input) == UnicodeCategory.ConnectorPunctuation;
        }

        #endregion

        #region IsControl

        /// <summary>
        /// Control code character, with a Unicode value of U+007F or in the range U+0000 through U+001F or U+0080 through U+009F. 
        /// Signified by the Unicode designation "Cc" (other, control). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>true or false</returns>
        public static bool IsControl(this char input)
        {
            return GetUnicodeCategory(input) == UnicodeCategory.Control;
        }

        #endregion

        #region IsCurrentcySymbol

        /// <summary>
        /// Currency symbol character. 
        /// Signified by the Unicode designation "Sc" (symbol, currency).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsCurrencySymbol(this char input)
        {
            //https://graphemica.com/categories/currency-symbol
            return GetUnicodeCategory(input) == UnicodeCategory.CurrencySymbol;
        }

        #endregion

        #region IsDashPunctuation

        /// <summary>
        /// Dash or hyphen character. Signified by the Unicode designation "Pd" (punctuation, dash).
        /// </summary>
        /// <param name="input"></param>
        /// <returns>true or false</returns>
        public static bool IsDashPunctuation(this char input)
        {
            //https://graphemica.com/categories/dash-punctuation
            return GetUnicodeCategory(input) == UnicodeCategory.DashPunctuation;
        }

        #endregion

        #region IsDecimalDigitNumber

        /// <summary>
        /// Decimal digit character, that is, a character in the range 0 through 9. 
        /// Signified by the Unicode designation "Nd" (number, decimal digit). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns>true or false</returns>
        public static bool IsDecimalDigitNumber(this char input)
        {
            //https://graphemica.com/categories/decimal-digit-number
            return GetUnicodeCategory(input) == UnicodeCategory.DecimalDigitNumber;
        }

        #endregion

        #region IsEnclosingMark

        /// <summary>
        /// Closing or final quotation mark character. 
        /// Signified by the Unicode designation "Pf" (punctuation, final quote).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsEnclosingMark(this char input)
        {
            //https://graphemica.com/categories/enclosing-mark
            return GetUnicodeCategory(input) == UnicodeCategory.EnclosingMark;
        }

        #endregion

        #region IsFormat

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsFormat(this char input)
        {
            return GetUnicodeCategory(input) == UnicodeCategory.Format;
        }

        #endregion

        #region IsInitialQuotePunctuation

        /// <summary>
        /// Opening or initial quotation mark character. 
        /// Signified by the Unicode designation "Pi" (punctuation, initial quote).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsInitialQuotePunctuation(this char input, bool strictPerUnicodeDefinition = true)
        {
            if (strictPerUnicodeDefinition) return GetUnicodeCategory(input) == UnicodeCategory.InitialQuotePunctuation;

            //https://graphemica.com/categories/initial-quote-punctuation
            return GetUnicodeCategory(input) == UnicodeCategory.InitialQuotePunctuation ||
                input == DoubleQuotationMark ||
                input == RightPointingAngleBracket ||
                input == RightPointingDoubleAngleQuotationMark;
        }

        #endregion

        #region IsLetterNumber

        /// <summary>
        /// Number represented by a letter, instead of a decimal digit, for example, the Roman numeral for five, which is "V". 
        /// The indicator is signified by the Unicode designation "Nl" (number, letter).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsLetterNumber(this char input)
        {
            //https://graphemica.com/categories/letter-number
            return GetUnicodeCategory(input) == UnicodeCategory.LetterNumber;
        }

        #endregion

        #region IsLineSeparator

        /// <summary>
        /// Character that is used to separate lines of text. 
        /// Signified by the Unicode designation "Zl" (separator, line).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsLineSeparator(this char input)
        {
            //https://graphemica.com/categories/line-separator
            return GetUnicodeCategory(input) == UnicodeCategory.LineSeparator;
        }

        #endregion

        #region IsLowercaseLetter

        /// <summary>
        /// Lowercase letter. 
        /// Signified by the Unicode designation "Ll" (letter, lowercase).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsLowercaseLetter(this char input)
        {
            //https://graphemica.com/categories/lowercase-letter
            return GetUnicodeCategory(input) == UnicodeCategory.LowercaseLetter;
        }

        #endregion

        #region IsMathSymbol

        /// <summary>
        /// Mathematical symbol character, such as "+" or "= ". 
        /// Signified by the Unicode designation "Sm" (symbol, math).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsMathSymbol(this char input)
        {
            //https://graphemica.com/categories/math-symbol
            return GetUnicodeCategory(input) == UnicodeCategory.MathSymbol;
        }

        #endregion

        #region IsModifierLetter

        /// <summary>
        /// Modifier letter character, which is free-standing spacing character that indicates modifications of a preceding letter. 
        /// Signified by the Unicode designation "Lm" (letter, modifier).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsModifierLetter(this char input)
        {
            //https://graphemica.com/categories/modifier-letter
            return GetUnicodeCategory(input) == UnicodeCategory.ModifierLetter;
        }

        #endregion

        #region IsModifierSymbol

        /// <summary>
        /// Modifier symbol character, which indicates modifications of surrounding characters. 
        /// For example, the fraction slash indicates that the number to the left is the numerator and the number to the right is the denominator. 
        /// The indicator is signified by the Unicode designation "Sk" (symbol, modifier).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsModifierSymbol(this char input)
        {
            return GetUnicodeCategory(input) == UnicodeCategory.ModifierSymbol;
        }

        #endregion

        #region IsNonSpacingMark

        /// <summary>
        /// Nonspacing character that indicates modifications of a base character. 
        /// Signified by the Unicode designation "Mn" (mark, nonspacing).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsNonSpacingMark(this char input)
        {
            return GetUnicodeCategory(input) == UnicodeCategory.NonSpacingMark;
        }

        #endregion

        #region IsOpenPunctuation

        /// <summary>
        /// Opening character of one of the paired punctuation marks, such as parentheses, square brackets, and braces. 
        /// Signified by the Unicode designation "Ps" (punctuation, open). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsOpenPunctuation(this char input, bool strictPerUnicodeDefinition = true)
        {
            if (strictPerUnicodeDefinition) return GetUnicodeCategory(input) == UnicodeCategory.OpenPunctuation;

            //https://graphemica.com/categories/open-punctuation
            //we exclude some quotation marks that are - for some reason - categorized as OpenPunctuation instead of InitialQuotePunctuation
            return GetUnicodeCategory(input) == UnicodeCategory.OpenPunctuation && !(
                input == SingleLow9QuotationMark ||
                input == DoubleLow9QuotationMark ||
                input == ReversedDoublePrimeQuotationMark ||
                input == DoubleLowReversed9QuotationMark
            );
        }

        #endregion

        #region IsOtherLetter

        /// <summary>
        /// Letter that is not an uppercase letter, a lowercase letter, a titlecase letter, or a modifier letter. 
        /// Signified by the Unicode designation "Lo" (letter, other). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsOtherLetter(this char input)
        {
            //https://graphemica.com/categories/other-letter
            return GetUnicodeCategory(input) == UnicodeCategory.OtherLetter;
        }

        #endregion

        #region IsOtherNotAssigned

        /// <summary>
        /// Character that is not assigned to any Unicode category. 
        /// Signified by the Unicode designation "Cn" (other, not assigned). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsOtherNotAssigned(this char input)
        {
            return GetUnicodeCategory(input) == UnicodeCategory.OtherNotAssigned;
        }

        #endregion

        #region IsOtherNumber

        /// <summary>
        /// Number that is neither a decimal digit nor a letter number, for example, the fraction 1/2. 
        /// The indicator is signified by the Unicode designation "No" (number, other). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsOtherNumber(this char input)
        {
            //https://graphemica.com/categories/other-number
            return GetUnicodeCategory(input) == UnicodeCategory.OtherNumber;
        }

        #endregion

        #region IsOtherPunctuation

        /// <summary>
        /// Punctuation character that is not a connector, a dash, open punctuation, close punctuation, an initial quote, or a final quote.
        /// Signified by the Unicode designation "Po" (punctuation, other).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsOtherPunctuation(this char input)
        {
            //https://graphemica.com/categories/other-punctuation
            return GetUnicodeCategory(input) == UnicodeCategory.OtherPunctuation;
        }

        #endregion

        #region IsOtherSymbol

        /// <summary>
        /// Symbol character that is not a mathematical symbol, a currency symbol or a modifier symbol. 
        /// Signified by the Unicode designation "So" (symbol, other).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsOtherSymbol(this char input)
        {
            //https://graphemica.com/categories/other-symbol
            return GetUnicodeCategory(input) == UnicodeCategory.OtherSymbol;
        }

        #endregion

        #region IsParagraphSeparator

        /// <summary>
        /// Character used to separate paragraphs. 
        /// Signified by the Unicode designation "Zp" (separator, paragraph).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsParagraphSeparator(this char input)
        {
            //https://graphemica.com/categories/paragraph-separator
            return GetUnicodeCategory(input) == UnicodeCategory.ParagraphSeparator;
        }

        #endregion

        #region IsPrivateUse

        /// <summary>
        /// Private-use character, with a Unicode value in the range U+E000 through U+F8FF. 
        /// Signified by the Unicode designation "Co" (other, private use). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsPrivateUse(this char input)
        {
            return GetUnicodeCategory(input) == UnicodeCategory.PrivateUse;
        }

        #endregion

        #region IsPunctuation

        public static bool IsPunctuation(this char input, bool strictPerUnicodeDefinition = true)
        {
            return 
                input.IsOpenPunctuation(strictPerUnicodeDefinition) ||
                input.IsClosePunctuation(strictPerUnicodeDefinition) ||
                input.IsDashPunctuation() ||
                input.IsInitialQuotePunctuation(strictPerUnicodeDefinition) ||
                input.IsFinalQuotePunctuation(strictPerUnicodeDefinition) ||
                input.IsOtherPunctuation();
        }

        public static bool IsPunctuationForTitleCasing(this char input)
        {
            //all punctuation (strict per unicode definition) except all quotation marks (not strict per unicode definition)

            //old Regex Patterns used for casing scripts
            //string matchInterpunctuation = @"\.|:|\?|!|\u00BF|\u2014|\(|\)|\[|\]";
            //string matchInterpunctuation = @"(?=[^\p{Pi}\p{Pf}\u201A\u201E\u301D\u301E\u301F\u2e42])[\p{P}]";

            return input.IsPunctuation() && !(input.IsInitialQuotePunctuation(false) || input.IsFinalQuotePunctuation(false));
        }

        /// <summary>
        /// Characters that are used as apostrophs.
        /// </summary>
        public static readonly char[] PunctuationForSentenceCasing = {
            Period
            , Colon
            , ExclamationMark
            , InvertedExclamationMark
            , QuestionMark
            , InvertedQuestionMark
        };

        public static bool IsPunctuationForSentenceCasing(this char input)
        {
            //string interpunctuactionFollowedByCapitalization = @"(\.)|(:)|(\?)|(!)"; //next word will be capitalized if possible

            return PunctuationForSentenceCasing.Any(c => c == input);
        }

        #endregion

        #region IsQuotePunctuation

        public static bool IsQuotePunctuation(this char input, bool strictPerUnicodeDefinition = true)
        {
            return
                input.IsInitialQuotePunctuation(strictPerUnicodeDefinition) ||
                input.IsFinalQuotePunctuation(strictPerUnicodeDefinition);
        }

        #endregion

        #region IsSeparator

        public static bool IsSeparator(this char input)
        {
            return
                GetUnicodeCategory(input) == UnicodeCategory.SpaceSeparator ||      //Zs
                GetUnicodeCategory(input) == UnicodeCategory.LineSeparator ||       //Zl
                GetUnicodeCategory(input) == UnicodeCategory.ParagraphSeparator;    //Zp
        }

        #endregion

        #region IsSpaceSeparator

        /// <summary>
        /// Space character, which has no glyph but is not a control or format character. 
        /// Signified by the Unicode designation "Zs" (separator, space).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsSpaceSeparator(this char input)
        {
            //https://graphemica.com/categories/space-separator
            return GetUnicodeCategory(input) == UnicodeCategory.SpaceSeparator;
        }

        #endregion

        #region IsSpacingCombiningMark

        /// <summary>
        /// Spacing character that indicates modifications of a base character and affects the width of the glyph for that base character. 
        /// Signified by the Unicode designation "Mc" (mark, spacing combining). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsSpacingCombiningMark(this char input)
        {
            //https://graphemica.com/categories/spacing-combining-mark
            return GetUnicodeCategory(input) == UnicodeCategory.SpacingCombiningMark;
        }

        #endregion

        #region IsSurrogate

        /// <summary>
        /// High surrogate or a low surrogate character. Surrogate code values are in the range U+D800 through U+DFFF. 
        /// Signified by the Unicode designation "Cs" (other, surrogate).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsSurrogate(this char input)
        {
            return GetUnicodeCategory(input) == UnicodeCategory.Surrogate;
        }

        #endregion

        #region IsLetter

        public static bool IsLetter(this char input)
        {
            return input.IsUppercaseLetter() || input.IsLowercaseLetter() || input.IsTitlecaseLetter();
        }

        #endregion

        #region IsTitlecaseLetter

        /// <summary>
        /// Titlecase letter. 
        /// Signified by the Unicode designation "Lt" (letter, titlecase).
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsTitlecaseLetter(this char input)
        {
            //https://graphemica.com/categories/titlecase-letter
            return GetUnicodeCategory(input) == UnicodeCategory.TitlecaseLetter;
        }

        #endregion

        #region IsUppercaseLetter

        /// <summary>
        /// Uppercase letter. 
        /// Signified by the Unicode designation "Lu" (letter, uppercase). 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsUppercaseLetter(this char input)
        {
            //https://graphemica.com/categories/uppercase-letter
            return GetUnicodeCategory(input) == UnicodeCategory.UppercaseLetter;
        }

        #endregion

        #region IsWhitespace

        public static bool IsWhitespace(this char input)
        {
            //https://en.wikipedia.org/wiki/Template:Whitespace_(Unicode)
            return
                input.IsSpaceSeparator() ||
                input.IsLineSeparator() ||
                input.IsParagraphSeparator() ||
                input.IsFormat() ||
                input.IsControl();
        }

        #endregion

        #endregion
    }
}
