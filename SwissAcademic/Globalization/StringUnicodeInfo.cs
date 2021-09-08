using System.Collections.Generic;

namespace SwissAcademic.Globalization
{
    public static class StringUnicodeUtility
    {
        #region GetBidiGroups

        #region Documentation -----------------------------------------------------------------------------
        /// <summary>	Splits a string into unambiguous bidi groups. </summary>
        ///
        /// <remarks>	Thomas Schempp, 29.11.2011. </remarks>
        ///
        /// <param name="text">	The text to act on. </param>
        ///
        /// <returns>	A collection of unambiguous bidi groups. </returns>
        #endregion

        public static List<BidiGroup> GetBidiGroups(this string text)
        {
            var bidiGroups = new List<BidiGroup>();
            if (string.IsNullOrEmpty(text)) return bidiGroups;

            BidiGroup currentGroup = null;

            for (int i = 0; i < text.Length; i++)
            {
                var bidiCategorySimplified = CharUnicodeInfo.GetBidiCategorySimplified(text, i);

                #region currentGroup == null

                if (currentGroup == null)
                {
                    currentGroup = new BidiGroup(text[i], bidiCategorySimplified);
                    bidiGroups.Add(currentGroup);
                }

                #endregion

                #region Neutral: Search for next unambigouus direction

                else if (bidiCategorySimplified == BidiCategorySimplified.Neutral)
                {
                    if (currentGroup.BidiCategorySimplified != BidiCategorySimplified.RightToLeft)
                    {
                        currentGroup.Append(text[i]);
                        continue;
                    }

                    var found = false;

                    for (int j = i + 1; j < text.Length; j++)
                    {
                        var nextBidiCategorySimplified = CharUnicodeInfo.GetBidiCategorySimplified(text, j);
                        if (nextBidiCategorySimplified == BidiCategorySimplified.Neutral) continue;

                        if (currentGroup.BidiCategorySimplified != nextBidiCategorySimplified)
                        {
                            var newGroup = new BidiGroup(text[i], nextBidiCategorySimplified);
                            bidiGroups.Add(newGroup);
                            currentGroup = newGroup;
                        }
                        else
                        {
                            currentGroup.Append(text[i]);
                        }

                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        currentGroup.Append(text[i]);

                        //switch (currentGroup.BidiCategorySimplified)
                        //{
                        //    case BidiCategorySimplified.RightToLeft:
                        //        {
                        //            var newGroup = new BidiGroup(text[i], BidiCategorySimplified.LeftToRight);
                        //            bidiGroups.Add(newGroup);
                        //            currentGroup = newGroup;
                        //        }
                        //        break;

                        //    default:
                        //        currentGroup.Append(text[i]);
                        //        break;
                        //}
                    }
                }

                #endregion

                #region Direction change

                else if (currentGroup.BidiCategorySimplified != bidiCategorySimplified)
                {
                    var newGroup = new BidiGroup(text[i], bidiCategorySimplified);
                    bidiGroups.Add(newGroup);
                    currentGroup = newGroup;
                }

                #endregion

                #region Append at same direction group

                else
                {
                    currentGroup.Append(text[i]);
                }

                #endregion
            }

            return bidiGroups;
        }

        #endregion
    }
}
