using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

namespace SwissAcademic.Resources
{
    public static class ResourceHelper
    {
        #region Methoden

        #region Diverse

        #region AppendColon

        public static string AppendColon(string input, CultureInfo culture = null)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var twoLetterISOLanguageName = culture == null ?
                System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName :
                culture.TwoLetterISOLanguageName;

            string output = input;

            switch (twoLetterISOLanguageName)
            {
                case "fr":
                    //using a non-breaking space: in dec: &#160 or in hex: &#xA0
                    output = string.Format("{0} :", input);
                    break;

                default:
                    output = string.Format("{0}:", input);
                    break;
            }

            return output;
        }

        #endregion AppendColon

        #region CoreStatementMissing

        public static string CoreStatementMissing(string knowledgeItemType)
        {
            return Strings.ResourceManager.GetString(string.Format("CoreStatementMissing_{0}", knowledgeItemType));
        }

        #endregion

        #region GetImportSourceStatusCaption

        public static string GetImportSourceStatusCaption(string importSource, int importedReferenceCount)
        {
            if (importedReferenceCount > 0)
            {
                return importedReferenceCount.ToString() + " " + Strings.ResourceManager.GetString("ImportSourceStatusCaption_" + importSource + "_Success");
            }
            else
            {
                return Strings.ResourceManager.GetString("ImportSourceStatusCaption_" + importSource + "_NotSuccess");
            }
        }

        #endregion

        #region GetNumberingTypeFormCaption

        public static string GetNumberingTypeFormCaption(string numberingType)
        {
            return Strings.ResourceManager.GetString(string.Format("NumberingTypeFormCaption_{0}", numberingType));
        }

        #endregion

        #region GetNumberingTypeSmartRepeaterRowCaption

        public static string GetNumberingTypeSmartRepeaterRowCaption(string numberingType)
        {
            return Strings.ResourceManager.GetString(string.Format("NumberingTypeSmartRepeaterRowCaption_{0}", numberingType));
        }

        #endregion

        #region GetTemplateUseCaseNameLocalized

        public static string GetTemplateUseCaseNameLocalized(string referenceTypeIdText, string parentReferenceTypeIdText)
        {
            return Strings.ResourceManager.GetString(string.Format("TemplateUseCaseNameLocalized_{0}_{1}", referenceTypeIdText, parentReferenceTypeIdText));
        }

        public static string GetTemplateUseCaseNameLocalized(string referenceTypeIdText, string parentReferenceTypeIdText, CultureInfo culture)
        {
            if (culture == null) return Strings.ResourceManager.GetString(string.Format("TemplateUseCaseNameLocalized_{0}_{1}", referenceTypeIdText, parentReferenceTypeIdText));
            return Strings.ResourceManager.GetString(string.Format("TemplateUseCaseNameLocalized_{0}_{1}", referenceTypeIdText, parentReferenceTypeIdText), culture);
        }

        #endregion

        #region GetImportReferenceTypeNameLocalized

        public static string GetImportReferenceTypeNameLocalized(string importReferenceTypeName)
        {
            return Enums.ResourceManager.GetString(string.Format("ImportReferenceType_{0}", importReferenceTypeName));
        }

        #endregion

        #region GetBibTeXExportWizardPageTitle

        public static string GetBibTeXExportWizardPageTitle(int page)
        {
            return Strings.ResourceManager.GetString(string.Format("BibTeXExportWizardPageTitle_{0}", page.ToString()));
        }

        #endregion

        #region ExportWizardOption

        public static string ExportWizardTitle(int pageNumber)
        {
            return Strings.ResourceManager.GetString(string.Format("ExportWizard_Title_{0}", pageNumber));
        }

        #endregion

        #region ImportWizardTitle

        public static string ImportWizardTitle(int pageNumber)
        {
            return Strings.ResourceManager.GetString(string.Format("ImportWizard_Title_{0}", pageNumber));
        }

        #endregion

        #region ImportProgressStatusChangedCaption

        public static string ImportProgressStatusChangedCaption(string referencePropertyId)
        {
            return Strings.ResourceManager.GetString("ImportProgressStatusChangedCaption_" + referencePropertyId);
        }

        #endregion

        #endregion

        #region GetEnumName

        public static string GetEnumName(Enum e)
        {
            return GetEnumName(e, (List<int>)null, ", ", null, false);
        }

        public static string GetEnumName(Enum e, CultureInfo culture)
        {
            return GetEnumName(e, (List<int>)null, ", ", culture, false);
        }

        public static string GetEnumName(Enum e, bool sort = false)
        {
            return GetEnumName(e, (List<int>)null, ", ", null, sort);
        }

        public static string GetEnumName(Enum e, string separator, bool sort = false)
        {
            return GetEnumName(e, (List<int>)null, separator, null, sort);
        }

        public static string GetEnumName(Enum e, List<int> ignoreValues)
        {
            return GetEnumName(e, ignoreValues, ", ", null);
        }

        public static string GetEnumName(Enum e, List<int> ignoreValues, string separator)
        {
            return GetEnumName(e, ignoreValues, separator, null, false);
        }

        public static string GetEnumName(Enum e, List<int> ignoreValues, string separator, CultureInfo culture)
        {
            return GetEnumName(e, ignoreValues, separator, culture, false);
        }

        public static string GetEnumName(Enum e, List<int> ignoreValues, string separator, CultureInfo culture, bool sort = false)
        {
            Type type = e.GetType();

            if (type.Name.Equals("ReferenceTypeId", StringComparison.Ordinal))
            {
                return ReferenceTypeLabels.ResourceManager.GetString(e.ToString(), culture);
            }


            if (string.IsNullOrEmpty(separator)) separator = ", ";

            if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0)
            {
                if (culture != null) return Enums.ResourceManager.GetString(string.Format("{0}_{1}", type.Name, e.ToString()), culture) ?? e.ToString();
                return Enums.ResourceManager.GetString(string.Format("{0}_{1}", type.Name, e.ToString())) ?? e.ToString();
            }

            else
            {
                int intValue;

                #region SwissAcademic.Drawing.FontStyle

                if (string.CompareOrdinal(type.FullName, "SwissAcademic.Drawing.FontStyle") == 0)
                {
                    /* Spezialfall FontStyle:
					 * 
					 * 1. Es gibt die speziellen Enumerationswerte -2, -1 und 0, die inhaltlich eine andere Bedeutung 
					 * haben als die Enumerationswerte > 0
					 * 
					 * 2. Die Enumerationswerte orientieren sich an Subsystems RTF Generator und sind deshalb
					 * in anderer Reihenfolge als in unserem GUI. Deshalb ist ein foreach(...GetValues...) nicht
					 * möglich.
					 * 
					 * Aus diesen Gründen wird diese Enumeration hier speziell behandelt. */


                    intValue = Convert.ToInt32(e);

                    #region SameAsPrevious

                    //if (intValue == -2)
                    if (intValue == 524288)
                    {
                        if (culture != null) return Enums.ResourceManager.GetString(nameof(Enums.FontStyle_SameAsPrevious), culture);
                        else return Enums.FontStyle_SameAsPrevious;
                    }

                    #endregion

                    #region SameAsNext

                    //else if (intValue == -1)
                    else if (intValue == 262144)
                    {
                        if (culture != null) return Enums.ResourceManager.GetString(nameof(Enums.FontStyle_SameAsNext), culture);
                        else return Enums.FontStyle_SameAsNext;
                    }

                    #endregion

                    #region Neutral

                    else if (intValue == 0)
                    {
                        if (culture != null) return Enums.ResourceManager.GetString(nameof(Enums.FontStyle_Neutral), culture);
                        else return Enums.FontStyle_Neutral;
                    }

                    #endregion

                    else
                    {
                        intValue = Convert.ToInt32(e);

                        List<string> namedBitValues = new List<string>();

                        #region Bold

                        if ((intValue & 2) == 2)
                        {
                            if (culture != null) namedBitValues.Add(Enums.ResourceManager.GetString(nameof(Enums.FontStyle_Bold), culture));
                            else namedBitValues.Add(Enums.FontStyle_Bold);
                        }

                        #endregion

                        #region Italic

                        if ((intValue & 4) == 4)
                        {
                            if (culture != null) namedBitValues.Add(Enums.ResourceManager.GetString(nameof(Enums.FontStyle_Italic), culture));
                            else namedBitValues.Add(Enums.FontStyle_Italic);
                        }

                        #endregion

                        #region Underline

                        if ((intValue & 1) == 1)
                        {
                            if (culture != null) namedBitValues.Add(Enums.ResourceManager.GetString(nameof(Enums.FontStyle_Underline), culture));
                            else namedBitValues.Add(Enums.FontStyle_Underline);
                        }

                        #endregion

                        #region DoubleUnderline

                        if ((intValue & 256) == 256)
                        {
                            if (culture != null) namedBitValues.Add(Enums.ResourceManager.GetString(nameof(Enums.FontStyle_DoubleUnderline), culture));
                            else namedBitValues.Add(Enums.FontStyle_DoubleUnderline);
                        }

                        #endregion

                        #region StrikeThrough

                        if ((intValue & 8) == 8)
                        {
                            if (culture != null) namedBitValues.Add(Enums.ResourceManager.GetString(nameof(Enums.FontStyle_StrikeThrough), culture));
                            else namedBitValues.Add(Enums.FontStyle_StrikeThrough);
                        }

                        #endregion

                        #region SmallCaps

                        if ((intValue & 131072) == 131072)
                        {
                            if (culture != null) namedBitValues.Add(Enums.ResourceManager.GetString(nameof(Enums.FontStyle_SmallCaps), culture));
                            else namedBitValues.Add(Enums.FontStyle_SmallCaps);
                        }

                        #endregion

                        #region AllCaps

                        if ((intValue & 65536) == 65536)
                        {
                            if (culture != null) namedBitValues.Add(Enums.ResourceManager.GetString(nameof(Enums.FontStyle_AllCaps), culture));
                            else namedBitValues.Add(Enums.FontStyle_AllCaps);
                        }

                        #endregion

                        #region Superscript

                        if ((intValue & 16) == 16)
                        {
                            if (culture != null) namedBitValues.Add(Enums.ResourceManager.GetString(nameof(Enums.FontStyle_Superscript), culture));
                            else namedBitValues.Add(Enums.FontStyle_Superscript);
                        }

                        #endregion

                        #region Subscript

                        if ((intValue & 32) == 32)
                        {
                            if (culture != null) namedBitValues.Add(Enums.ResourceManager.GetString(nameof(Enums.FontStyle_Subscript), culture));
                            else namedBitValues.Add(Enums.FontStyle_Subscript);
                        }

                        #endregion

                        //output
                        if (namedBitValues.Count == 0)
                        {
                            return string.Empty;
                        }
                        else
                        {
                            if (sort) namedBitValues.Sort();
                            return string.Join(separator, namedBitValues);
                        }
                    }
                }

                #endregion SwissAcademic.Drawing.FontStyle

                #region Other Enums

                else
                {
                    List<string> namedBitValues = new List<string>();

                    foreach (Enum value in Enum.GetValues(type))
                    {
                        intValue = Convert.ToInt32(value);

                        if (ignoreValues == null || !ignoreValues.Contains(intValue))
                        {
                            if ((Convert.ToInt32(e) & intValue) == intValue)
                            {
                                string s = string.Empty;
                                if (culture == null)
                                {
                                    s = Enums.ResourceManager.GetString(string.Format("{0}_{1}", type.Name, value));
                                }
                                else
                                {
                                    s = Enums.ResourceManager.GetString(string.Format("{0}_{1}", type.Name, value), culture);
                                }
                                if (!string.IsNullOrEmpty(s)) namedBitValues.Add(s);
                            }
                        }
                    }

                    //output
                    if (namedBitValues.Count == 0) return string.Empty;

                    if (sort) namedBitValues.Sort();
                    return string.Join(separator, namedBitValues);
                }

                #endregion Other Enums
            }
        }

        public static string GetEnumName(Type enumType, string valueName)
        {
            return Enums.ResourceManager.GetString(string.Format("{0}_{1}", enumType.Name, valueName));
        }

        public static string GetEnumName(Type enumType, string valueName, CultureInfo culture)
        {
            if (culture == null) return Enums.ResourceManager.GetString(string.Format("{0}_{1}", enumType.Name, valueName));
            return Enums.ResourceManager.GetString(string.Format("{0}_{1}", enumType.Name, valueName), culture);
        }

        #endregion GetEnumName

        #region GetFormText

        public static string GetFormText(string formName)
        {
            return FormTexts.ResourceManager.GetString(formName);
        }

        public static string GetFormText(string formName, CultureInfo culture)
        {
            if (culture == null) return FormTexts.ResourceManager.GetString(formName);
            return FormTexts.ResourceManager.GetString(formName, culture);
        }

        #endregion

        #region GetCultureNameLocalized

        public static string GetCultureNameLocalized(CultureInfo culture)
        {
            return GetCultureNameLocalized(culture, null);
        }

        public static string GetCultureNameLocalized(CultureInfo culture, CultureInfo targetCulture)
        {
            var name = culture.Name.ToUpper().Replace("-", "_");

            var cultureNameLocalized = targetCulture == null ?
                LanguagesAndCultures.ResourceManager.GetString(name) :
                LanguagesAndCultures.ResourceManager.GetString(name, targetCulture);

            if (string.IsNullOrEmpty(cultureNameLocalized))
            {
                return culture.DisplayName;
            }

            return cultureNameLocalized;
        }

        #endregion GetCultureNameLocalized

        #region GetPropertyName

        public static string GetPropertyName(string typeName, string propertyName)
        {
            return GetPropertyName(typeName, propertyName, null);
        }

        public static string GetPropertyName(string typeName, string propertyName, CultureInfo culture)
        {
            switch (propertyName)
            {
                case "CreatedBy":
                case "CreatedByCitaviUserId":
                case "CreatedByCitaviUserName":
                case "CreatedBySid":
                case "CreatedByWindowsUserName":
                case "CreatedOn":
                case "FullName":
                case "Id":
                case "ModifiedBy":
                case "ModifiedByCitaviUserId":
                case "ModifiedByCitaviUserName":
                case "ModifiedBySid":
                case "ModifiedByWindowsUserName":
                case "ModifiedOn":
                case "Name":
                case "Notes":
                case "NumberOfAssignedReferences":
                    {
                        if (culture == null) return Entities.ResourceManager.GetString(string.Format("{0}_{1}", "CommonProperty", propertyName));
                        return Entities.ResourceManager.GetString(string.Format("{0}_{1}", "CommonProperty", propertyName), culture);
                    }


                default:
                    {
                        if (culture == null) return Entities.ResourceManager.GetString(string.Format("{0}_{1}", typeName, propertyName));
                        return Entities.ResourceManager.GetString(string.Format("{0}_{1}", typeName, propertyName), culture);
                    }

            }
        }

        #endregion

        #region GetLanguageName

        public static string GetLanguageName(string twoLetterIsoLanguageName)
        {
            return GetLanguageName(twoLetterIsoLanguageName, null);
        }

        public static string GetLanguageName(string twoLetterIsoLanguageName, CultureInfo targetCulture)
        {
            if (string.IsNullOrEmpty(twoLetterIsoLanguageName)) return string.Empty;

            var languageName = targetCulture == null ?
                Strings.ResourceManager.GetString(string.Format("{0}_{1}", "Language", twoLetterIsoLanguageName)) :
                Strings.ResourceManager.GetString(string.Format("{0}_{1}", "Language", twoLetterIsoLanguageName), targetCulture);

            if (string.IsNullOrEmpty(languageName))
            {
                return twoLetterIsoLanguageName;
            }

            return languageName;
        }

        #endregion GetLanguageName

        #region GetCountryName

        public static string GetCountryName(string twoLetterIsoCountryCode)
        {
            return GetCountryName(twoLetterIsoCountryCode, null);
        }

        public static string GetCountryName(string twoLetterIsoCountryCode, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(twoLetterIsoCountryCode)) return string.Empty;

            var countryName = culture == null ?
                Strings.ResourceManager.GetString(string.Format("{0}_{1}", "Country", twoLetterIsoCountryCode)) :
                Strings.ResourceManager.GetString(string.Format("{0}_{1}", "Country", twoLetterIsoCountryCode), culture);

            if (string.IsNullOrEmpty(countryName))
            {
                return twoLetterIsoCountryCode;
            }

            return countryName;
        }

        #endregion GetCountryName

        #region GetNameOfResource

        public static string GetNameOf<T>(Expression<Func<T>> property)
        {
            return (property.Body as MemberExpression).Member.Name;
        }

        #endregion

        #region Shorten

        public static string Shorten(string text)
        {
            return Shorten(text, 40);
        }

        public static string Shorten(string text, int length)
        {
            if (text.Length <= length)
                return text;
            else
            {
                text = text.Trim();
                int j = 0;
                while (text.IndexOf("  ") != -1)
                {
                    text = text.Replace("  ", " ");

                    // Sicherheitsausgang, siehe gleiche Routine bei ShortTitleHelper.cs
                    j++;
                    if (j > 50)
                    {
                        break;
                    }
                }

                string[] words = text.Split(new char[1] { ' ' });
                if (words.Length == 1)
                    return string.Format("{0}...", text.Substring(0, text.Length - 3));
                else
                {
                    string result = words[0];
                    for (int i = 1; i < words.Length; i++)
                    {
                        if ((result.Length + words[i].Length) > length)
                            return string.Format("{0}...", result);
                        result += " " + words[i];
                    }
                    return result;
                }
            }
        }

        #endregion //Shorten

        #endregion
    }
}
