using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace SwissAcademic.Crm.Web
{
    public static class StringExtensions
    {
        static readonly List<char> _whitespaces = new List<char>
        {
            '\u200B',
            '\uFEFF',
            '\u180E',
            '\uFFFD',
        };

        public static string RemoveNonStandardWhitespace(this string text)
        {
            if (text == null)
            {
                return null;
            }
            return StringExtensions.RemoveNonStandardWhitespace(text.AsSpan()).ToString();
        }

        static ReadOnlySpan<char> RemoveNonStandardWhitespace(this ReadOnlySpan<char> text)
        {
            if (text == null)
            {
                return null;
            }

            int length_BeforeTrim;
            
            do
            {
                length_BeforeTrim = text.Length;
                text = text.Trim();
                foreach (var w in _whitespaces)
                {
                    text = text.Trim(w);
                }
            }
            while (text.Length != length_BeforeTrim);

            return text;
        }

        /// <summary>
        /// Replaces ß -> ss and Umlaute (ö -> o)
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ReplaceNonStandardEmailChars(this string text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            text = text.Replace("ö", "o");
            text = text.Replace("ä", "a");
            text = text.Replace("ü", "u");
            return text.Replace("ß", "ss");
        }

        public static string RemoveAccents(this string input)
        {
            return new string(input
                .Normalize(System.Text.NormalizationForm.FormD)
                .ToCharArray()
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
        }
    }
}
