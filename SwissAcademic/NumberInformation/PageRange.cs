using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using CharUnicodeInfo = SwissAcademic.Globalization.CharUnicodeInfo;

namespace SwissAcademic
{
    [DataContract]
    public struct PageRange
        :
        IEquatable<PageRange>,
        IEquatable<string>,
        IComparable<PageRange>
    {
        #region Felder

        string _originalString;

        #endregion

        #region Eigenschaften

        #region Count

        public int? Count
        {
            get
            {
                if (!StartPage.Number.HasValue) return null;
                if (!EndPage.Number.HasValue) return 1;

                if (StartPage.IsFullyNumeric && EndPage.IsFullyNumeric)
                {
                    return EndPage.Number.Value - StartPage.Number.Value + 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion

        #region Empty

        public static readonly PageRange Empty = new PageRange();	//is always of NumberingType.Page

        #endregion

        #region EndPage

        PageNumber _endPage;
        [DataMember]
        public PageNumber EndPage
        {
            get { return _endPage; }
            private set { _endPage = value; }
        }

        #endregion

        #region NumberingType

        [DataMember]
        public NumberingType NumberingType
        {
            get { return StartPage.NumberingType; }
        }

        #endregion

        #region NumeralSystem

        [DataMember]
        public NumeralSystem NumeralSystem
        {
            get
            {
                if (string.IsNullOrEmpty(_endPage.OriginalString) || 
                    _startPage.NumeralSystem == _endPage.NumeralSystem)
                {
                    return _startPage.NumeralSystem;
                }
                else
                {
                    return NumeralSystem.Omit;
                }
            }
        }

        #endregion

        #region OriginalString

        [DataMember]
        public string OriginalString
        {
            get
            {
                return _originalString;
            }
            private set { _originalString = value; }
        }


        #endregion

        #region PrettyString

        public string PrettyString
        {
            get
            {
                var startPageOutput = StartPage.ToString();
                var endPageOutput = EndPage.ToString();

                if (string.IsNullOrEmpty(startPageOutput)) return string.Empty;
                if (string.IsNullOrEmpty(endPageOutput)) return StartPage.ToString();

                return startPageOutput + "–" + endPageOutput;
            }
        }

        #endregion

        #region RangeChars

        //http://en.wikipedia.org/wiki/Dash
        //minus					U+2212		8722	==Divis??
        //hyphen				U+2010		8208
        //hyphen-minus			U+002D		45
        //non-breaking hyphen	U+2011		8209	Viertelgeviertstrich
        //figure dash			U+2012		8210
        //en dash				U+2013		8211	Halbgeviertstrich	ALT+0150
        //em dash				U+2014		8212	Geviertstrich		ALT+0151
        //horizontal bar		U+2015		8213	
        static readonly char[] RangeChars = new char[] { (char)8722, (char)8208, (char)45, (char)8209, (char)8210, (char)8211, (char)8212, (char)8213 };

        //(char)8259 ?   http://www.sql-und-xml.de/unicode-database/online-tools/ 

        #endregion

        #region StartPage

        PageNumber _startPage;
        [DataMember]
        public PageNumber StartPage
        {
            get { return _startPage; }
            private set { _startPage = value; }
        }

        #endregion

        #endregion Eigenschaften

        #region TrimChars

        static readonly char[] TrimChars = new char[] { '0', CharUnicodeInfo.Space, CharUnicodeInfo.EmSpace, CharUnicodeInfo.EnSpace, CharUnicodeInfo.FourPerEmSpace, CharUnicodeInfo.NarrowNoBreakSpace, CharUnicodeInfo.NonBreakingSpace, CharUnicodeInfo.PunctuationSpace, CharUnicodeInfo.SixPerEmSpace, CharUnicodeInfo.ThreePerEmSpace, CharUnicodeInfo.ZeroWidthNoBreakSpace };

        #endregion TrimChars

        #region Methoden

        #region Compare

        public static int Compare(PageRange x, PageRange y)
        {
            //ML 29.01.19: in case of changes to this routine please change JS-method accordingly:
            //SwissAcademic.Citavi.Web/scr/classes/singletons/model.js#knowledgeItemsSorted
            if (string.IsNullOrEmpty(x.StartPage.ToString()))
            {
                if (string.IsNullOrEmpty(y.StartPage.ToString())) return 0;
                return -1;
            }

            if (string.IsNullOrEmpty(y.StartPage.ToString())) return 1;

            var result = x.StartPage.CompareTo(y.StartPage);
            if (result != 0) return result;

            if (string.IsNullOrEmpty(x.EndPage.ToString()))
            {
                if (string.IsNullOrEmpty(y.EndPage.ToString())) return 0;
                return -1;
            }

            if (string.IsNullOrEmpty(y.EndPage.ToString())) return 1;

            result = x.EndPage.CompareTo(y.EndPage);
            if (result != 0) return result;

            result = x.NumberingType.CompareTo(y.NumberingType);
            if (result != 0) return result;

            return x.NumeralSystem.CompareTo(y.NumeralSystem);
        }

        #endregion

        #region CompareTo

        public int CompareTo(PageRange other)
        {
            return Compare(this, other);
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is PageRange) return Equals((PageRange)obj);
            return false;
        }

        public bool Equals(PageRange other)
        {
            return StartPage.Equals(other.StartPage) && EndPage.Equals(other.EndPage);
        }

        public bool Equals(string other)
        {
            return PrettyString.Equals(PageNumber.Empty.Update(other).PrettyString, StringComparison.Ordinal);
        }

        #endregion

        #region FromXml

        public static PageRange FromXml(string xml)
        {
            var pageRange = PageRange.Empty;
            if (string.IsNullOrEmpty(xml)) return pageRange;

            using (var stringReader = new StringReader(xml))
            {
                using (var r = XmlReader.Create(stringReader, XmlReaderExtensions.FragmentSettings))
                {
                    while (r.ReadToElement())
                    {
                        switch (r.Name)
                        {
                            case "sp":
                                {
                                    using (var subtree = r.ReadSubtree())
                                    {
                                        pageRange._startPage = PageNumber.FromXml(subtree);
                                    }
                                }
                                break;

                            case "ep":
                                {
                                    using (var subtree = r.ReadSubtree())
                                    {
                                        pageRange._endPage = PageNumber.FromXml(subtree);
                                    }
                                }
                                break;

                            case "os":
                                pageRange._originalString = r.ReadElementContentAsString();
                                break;
                        }
                    }
                }
            }

            return pageRange;
        }

        #endregion

        #region GetHashCode

        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(PrettyString)) return 0;
            return PrettyString.GetHashCode();
        }

        #endregion

        #region Parse

        static PageRange Parse(string input)
        {
            // This is only called from Update(string)
            // We clean the input there already and don't call Parse if the cleaned value is null,
            // so we don't have to repeat that check here.

            if (string.IsNullOrEmpty(input))
            {
                return new PageRange
                {
                    _startPage = PageNumber.Empty,
                    _endPage = PageNumber.Empty,
                    _originalString = null
                };
            }

            var startPage = PageNumber.Empty;
            var endPage = PageNumber.Empty;

            try
            {
                if (input.IndexOfAny(RangeChars) == -1)
                {
                    // Only start page defined
                    startPage = startPage.Update(input);
                }
                else
                {
                    var split = input.Split(RangeChars);

                    if (split.Length == 2)
                    {
                        startPage = startPage.Update(split[0]);
                        endPage = endPage.Update(split[1]);

                        if (startPage.Number.HasValue && endPage.IsFullyNumeric ||
                            (!startPage.IsFullyNumeric && startPage.Number.HasValue &&
                            !endPage.IsFullyNumeric && endPage.Number.HasValue))
                        {
                            /* The above means:
                             * "Kap. 34-89" is a range
                             * "Kap. 34 - Kap. 89" is a range
                             * "34 - Kap. 2" is not a range
                             * */

                            if (startPage.NumeralSystem != endPage.NumeralSystem)
                            {
                                switch (startPage.NumeralSystem)
                                {
                                    case NumeralSystem.Arabic:
                                        {
                                            // something like "53-v" where we think that "v" is Roman Lowercase
                                            // we treat it as a numeric start page.

                                            startPage = PageNumber.Empty.Update(input);
                                            endPage = PageNumber.Empty;
                                        }
                                        break;

                                    default:
                                        {
                                            // something like "I-53" where we think that "I" is Roman Uppercase
                                            // we treat it as a non-numeric start page.

                                            startPage = PageNumber.Empty.Update(input);
                                            startPage = startPage.Update(NumeralSystem.Omit);
                                            endPage = PageNumber.Empty;
                                        }
                                        break;
                                }
                            }

                            else if (startPage.IsFullyNumeric && endPage.IsFullyNumeric && startPage.Number == endPage.Number)
                            {
                                endPage = PageNumber.Empty;
                            }

                            else if ((startPage.Number > endPage.Number) && endPage.IsFullyNumeric)
                            {
                                // try if we need to update 92-7 to 92-97 (from import scenarios)
                                //JHP: The following is completely useless in some situations. It will turn "Einl. D Rn. 7–11 m.w.N." into "Einl. D Rn. 7–Einl11 m.w.N.", see ST 134-1E4E14AB-048B
                                //Therefore I added "&& endPage.IsFullyNumeric as a prerequisite for this else if branch.
                                var endPageCompleted = startPage.PrettyString.Substring(0, startPage.PrettyString.Length - endPage.PrettyString.Length) + endPage.PrettyString;
                                endPage = endPage.Update(endPageCompleted);

                                if (endPage.Number <= startPage.Number)
                                {
                                    // cases like 45-32 - that's not a range and not a number, treat it as text.
                                    startPage = startPage.Update(input);
                                    startPage = startPage.Update(NumeralSystem.Omit);
                                    endPage = PageNumber.Empty;
                                }
                            }
                        }

                        else
                        {
                            // everything else like "45-abc"
                            // may contain a number, so don't set omit
                            startPage = startPage.Update(input);
                            endPage = PageNumber.Empty;
                        }
                    }
                    else
                    {
                        // More than start and end page defined
                        // we treat it as a non-numeric start page.
                        startPage = startPage.Update(input);
                        startPage = startPage.Update(NumeralSystem.Omit);
                    }
                }

                return new PageRange
                {
                    _startPage = startPage,
                    _endPage = endPage,
                    _originalString = input
                };
            }

            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);

                return new PageRange
                {
                    _startPage = PageNumber.Empty.Update(input),
                    _endPage = PageNumber.Empty,
                    _originalString = input
                };
            }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return PrettyString ?? string.Empty;
        }

        #endregion

        #region ToXml

        public string ToXml()
        {
            if (StartPage == PageNumber.Empty) return null;

            var stringBuilder = new StringBuilder();

            using (var w = XmlWriterSafe.Create(stringBuilder, XmlWriterExtensions.FragmentSettings))
            {
                w.WriteStartElement("sp");
                StartPage.ToXml(w);
                w.WriteEndElement();

                if (EndPage != PageNumber.Empty)
                {
                    w.WriteStartElement("ep");
                    EndPage.ToXml(w);
                    w.WriteEndElement();
                }

                if (!string.IsNullOrEmpty(_originalString)) w.WriteElementString("os", _originalString);
            }

            return stringBuilder.ToString();
        }

        #endregion ToXml

        #region Update

        public PageRange Update(string value)
        {
            value = value.Clean(IllegalCharacters.Return | IllegalCharacters.Tab);

            if (string.IsNullOrEmpty(value))
            {
                return new PageRange
                {
                    _startPage = PageNumber.Empty.Update(_startPage.NumberingType).Update(_startPage.NumeralSystem),
                    _endPage = PageNumber.Empty.Update(_endPage.NumberingType).Update(_endPage.NumeralSystem),
                    _originalString = null
                };
            }

            var pageRangeParsed = Parse(value);

            if (NumeralSystem == NumeralSystem.Omit)
            {
                return new PageRange
                {
                    _startPage = pageRangeParsed.StartPage.Update(NumberingType).Update(NumeralSystem),
                    _endPage = pageRangeParsed.EndPage == PageNumber.Empty ? PageNumber.Empty : pageRangeParsed.EndPage.Update(NumberingType).Update(NumeralSystem),
                    _originalString = pageRangeParsed._originalString,
                };
            }
            else
            {
                return new PageRange
                {
                    _startPage = pageRangeParsed.StartPage.Update(NumberingType),
                    _endPage = pageRangeParsed.EndPage == PageNumber.Empty ? PageNumber.Empty : pageRangeParsed.EndPage.Update(NumberingType),
                    _originalString = pageRangeParsed._originalString,
                };
            }
        }

        public PageRange Update(int value)
        {
            return new PageRange
            {
                _startPage = PageNumber.Empty.Update(_startPage.NumberingType).Update(value.ToString()),
                _endPage = PageNumber.Empty.Update(_endPage.NumberingType),
                _originalString = value.ToString()
            };
        }

        public PageRange Update(NumberingType numberingType)
        {
            var currentNumberingType = NumberingType;
            var targetNumberingType = numberingType;

            if (currentNumberingType == targetNumberingType) return this;

            if (EndPage == PageNumber.Empty)
            {
                return new PageRange
                {
                    _startPage = StartPage.Update(numberingType),
                    _endPage = PageNumber.Empty,
                    _originalString = _originalString
                };
            }

            return new PageRange
            {
                _startPage = StartPage.Update(numberingType),
                _endPage = EndPage.Update(numberingType),
                _originalString = _originalString
            };
        }

        public PageRange Update(NumeralSystem numeralSystem)
        {
            var currentNumeralSystem = NumeralSystem;
            var targetNumeralSystem = numeralSystem;

            if (currentNumeralSystem == targetNumeralSystem) return this;

            if (targetNumeralSystem == NumeralSystem.Omit)
            {
                return new PageRange
                {
                    _startPage = PageNumber.Empty.Update(_originalString).Update(NumeralSystem.Omit).Update(NumberingType),
                    _endPage = PageNumber.Empty.Update(NumeralSystem.Omit),
                    _originalString = _originalString
                };
            }

            var pageRangeReparsed = Parse(_originalString).Update(NumberingType);
            if (pageRangeReparsed.NumeralSystem == targetNumeralSystem || pageRangeReparsed.NumeralSystem == NumeralSystem.Omit)
            {
                return pageRangeReparsed;
            }

            return new PageRange
            {
                _startPage = pageRangeReparsed.StartPage.Update(targetNumeralSystem),
                _endPage = pageRangeReparsed.EndPage.Update(targetNumeralSystem),
                _originalString = _originalString
            };
        }

        public PageRange CorrectPageCount(int? startNumber, int? endNumber)
        {
            //Fehler beim Speichern nach JSON
            //Im WAI hab ich jetzt die Freude, dass ich diese Structs aktualisieren muss :-( 
            //Adhoc via diese Methode. Kann später gekillt werden und ev. Structs -> Class :-)
            return new PageRange
            {
                _startPage = StartPage.CorrectPageCount(startNumber),
                _endPage = EndPage.CorrectPageCount(endNumber),
                _originalString = _originalString,
            };
        }

        #endregion

        #endregion Methoden

        #region Operators

        public static implicit operator PageRange(string input)
        {
            return PageRange.Empty.Update(input);
        }

        public static implicit operator PageRange(int input)
        {
            return PageRange.Empty.Update(input);
        }

        public static bool operator ==(PageRange a, PageRange b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(PageRange a, PageRange b)
        {
            return !(a == b);
        }

        #endregion
    }
    public class PageNumberEqualityComparer : IEqualityComparer<PageNumber>
    {
        public bool Equals(PageNumber x, PageNumber y) => x.Equals(y, PageNumberComparison.IgnoreNumberingType);

        public int GetHashCode(PageNumber obj) => obj.Number.HasValue ? obj.Number.GetHashCode() : obj.GetHashCode();
    }
}
