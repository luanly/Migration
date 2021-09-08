using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace SwissAcademic
{
    [DataContract]
    public struct PageCount
        :
        IEquatable<PageCount>,
        IComparable<PageCount>,
        IEquatable<string>
    {
        #region Felder

        string _originalString;

        #endregion

        #region Konstruktoren

        public PageCount(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                _count = null;
                _firstMatch = null;
                _isFullyNumeric = false;
                _numeralSystem = NumeralSystem.Arabic;
                _originalString = null;
                _prettyString = null;
            }
            else
            {
                var parseResult = Parse(input);

                _count = parseResult._count;
                _firstMatch = parseResult._firstMatch;
                _isFullyNumeric = parseResult._isFullyNumeric;
                _numeralSystem = parseResult._numeralSystem;
                _originalString = parseResult._originalString;
                _prettyString = parseResult._prettyString;
            }
        }

        internal PageCount(int? count, int? firstMatch, bool isFullyNumeric, NumeralSystem numeralSystem, string originalString, string prettyString)
        {
            _count = count;
            _firstMatch = firstMatch;
            _isFullyNumeric = isFullyNumeric;
            _numeralSystem = numeralSystem;
            _originalString = originalString;
            _prettyString = prettyString;
        }

        #endregion

        #region Properties

        #region Count

        int? _count;

        [DataMember]
        public int? Count
        {
            get { return _count; }
        }

        #endregion

        #region Empty

        public static readonly PageCount Empty = new PageCount(null);

        #endregion

        #region IsRoman

        public bool IsRoman
        {
            get { return NumeralSystem == NumeralSystem.RomanLowerCase || NumeralSystem == NumeralSystem.RomanUpperCase; }
        }

        #endregion

        #region FirstMatch

        int? _firstMatch;
        internal int? FirstMatch
        {
            get { return _firstMatch; }
        }

        #endregion

        #region IsFullyNumeric

        bool _isFullyNumeric;

        public bool IsFullyNumeric
        {
            get { return _isFullyNumeric; }
        }

        #endregion

        #region NumeralSystem

        NumeralSystem _numeralSystem;

        [DataMember]
        public NumeralSystem NumeralSystem
        {
            get { return _numeralSystem; }
        }

        #endregion

        #region PrettyString

        string _prettyString;

        [DataMember]
        public string PrettyString
        {
            get { return _prettyString ?? string.Empty; }
        }

        #endregion

        #endregion

        #region Methoden

        #region Compare

        public static int Compare(PageCount x, PageCount y)
        {
            if (x == PageCount.Empty)
            {
                if (y == PageCount.Empty) return 0;
                return -1;
            }
            if (y == PageCount.Empty) return 1;

            if (x.Count.HasValue)
            {
                if (y.Count.HasValue)
                {
                    return x.Count.Value.CompareTo(y.Count.Value);
                }

                return -1;
            }

            else if (y.Count.HasValue)
            {
                return 1;
            }

            return 0;
        }

        #endregion

        #region Convert

        public PageCount Convert(NumeralSystem numeralSystem)
        {
            if (numeralSystem == NumeralSystem)
            {
                return this;
            }

            if (numeralSystem == SwissAcademic.NumeralSystem.Omit || string.IsNullOrEmpty(_originalString))
            {
                return new PageCount
                {
                    _count = null,
                    _firstMatch = null,
                    _isFullyNumeric = false,
                    _numeralSystem = numeralSystem,
                    _originalString = _originalString,
                    _prettyString = _originalString,
                };
            }



            int intValue;
            if (!int.TryParse(_originalString, out intValue))
            {
                if (NumeralSystemConverter.IsRomanNumber(_originalString))
                {
                    intValue = int.Parse(NumeralSystemConverter.ToArabicNumber(_originalString));
                }
                else
                {
                    return this;
                }
            }

            string prettyString;

            switch (numeralSystem)
            {
                case NumeralSystem.Arabic:
                    prettyString = intValue.ToString();
                    break;

                case NumeralSystem.RomanLowerCase:
                    prettyString = NumeralSystemConverter.ToRomanNumber(intValue, true);
                    break;

                case NumeralSystem.RomanUpperCase:
                    prettyString = NumeralSystemConverter.ToRomanNumber(intValue, false);
                    break;

                case NumeralSystem.LetterLowerCase:
                    prettyString = NumeralSystemConverter.ToLetter(intValue, true);
                    break;

                case NumeralSystem.LetterUpperCase:
                    prettyString = NumeralSystemConverter.ToLetter(intValue, false);
                    break;

                default:
                    throw new NotSupportedException("Cannot convert to NumeralSystem.{0}".FormatString(numeralSystem));
            }

            return new PageCount
            {
                _count = intValue,
                _firstMatch = _count,
                _isFullyNumeric = true,
                _numeralSystem = numeralSystem,
                _originalString = _originalString,
                _prettyString = prettyString
            };
        }

        #endregion

        #region CompareTo

        public int CompareTo(PageCount other)
        {
            return Compare(this, other);
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is PageCount) return Equals((PageCount)obj);
            return false;
        }

        public bool Equals(PageCount other)
        {
            return StringComparer.Ordinal.Compare(PrettyString, other.PrettyString) == 0;
        }

        public bool Equals(string other)
        {
            return PrettyString.Equals(new PageCount(other).PrettyString, StringComparison.Ordinal);
        }

        #endregion

        #region FromXml

        public static PageCount FromXml(string xml)
        {
            var pageCount = PageCount.Empty;
            if (string.IsNullOrEmpty(xml)) return pageCount;

            using (var stringReader = new StringReader(xml))
            {
                using (var r = XmlReader.Create(stringReader, XmlReaderExtensions.FragmentSettings))
                {
                    while (r.ReadToElement())
                    {
                        switch (r.Name)
                        {
                            case "c":
                                pageCount._count = System.Convert.ToInt32(r.ReadElementContentAsInt());
                                break;

                            case "in":
                                pageCount._isFullyNumeric = r.ReadElementContentAsBoolean();
                                break;

                            case "ns":
                                pageCount._numeralSystem = r.ReadEnum<NumeralSystem>();
                                break;

                            case "os":
                                pageCount._originalString = r.ReadElementContentAsString();
                                break;

                            case "ps":
                                pageCount._prettyString = r.ReadElementContentAsString();
                                break;
                        }
                    }
                }
            }

            return pageCount;
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

        internal static PageCount Parse(string input)
        {
            var orignalString = input.Clean(IllegalCharacters.Return | IllegalCharacters.Tab);
            if (string.IsNullOrWhiteSpace(orignalString)) return PageCount.Empty;


            int? count = null;
            int? firstMatch = null;
            var numeralSystem = NumeralSystem.Arabic;
            var isFullyNumeric = false;
            var prettyString = orignalString;

            int value;
            if (int.TryParse(orignalString, out value))
            {
                count = Math.Abs(value);
                firstMatch = count;
                isFullyNumeric = true;
                numeralSystem = NumeralSystem.Arabic;

                // BUG 17280 Citavi normalisierte die Seitenzahlen
                //prettyString = value.ToString();
            }

            else if (NumeralSystemConverter.IsRomanNumber(orignalString))
            {
                count = int.Parse(NumeralSystemConverter.ToArabicNumber(orignalString));
                firstMatch = count;
                isFullyNumeric = true;
                numeralSystem = NumeralSystemConverter.IsLowercaseRomanNumber(orignalString) ? NumeralSystem.RomanLowerCase : NumeralSystem.RomanUpperCase;
                prettyString = NumeralSystemConverter.ToRomanNumber(count.Value, numeralSystem == NumeralSystem.RomanLowerCase);
            }

            else
            {
                var matches = SwissAcademic.NumberInformation.Matches(input, RomanNumberMatchType.AnyCase, false);

                if (matches.Any())
                {
                    var sum = matches.Sum(item => Math.Abs(item.Number));
                    if (sum < int.MaxValue)
                    {
                        count = System.Convert.ToInt32(sum);
                        firstMatch = System.Convert.ToInt32(Math.Abs(matches.First().Number));

                        numeralSystem = NumeralSystem.Arabic;

                        prettyString = orignalString;
                    }
                }
            }

            return new PageCount
            {
                _count = count,
                _firstMatch = firstMatch,
                _isFullyNumeric = isFullyNumeric,
                _numeralSystem = numeralSystem,
                _prettyString = prettyString,
                _originalString = orignalString
            };
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return PrettyString;
        }

        #endregion

        #region ToXml

        public string ToXml()
        {
            if (string.IsNullOrEmpty(_prettyString)) return null;

            var stringBuilder = new StringBuilder();

            using (var w = XmlWriterSafe.Create(stringBuilder, XmlWriterExtensions.FragmentSettings))
            {
                if (_count.HasValue)
                {
                    w.WriteStartElement("c");
                    w.WriteValue(_count);
                    w.WriteEndElement();
                }

                // FirstMatch is currently not serialized, it's needed for PageNumber parsing

                if (_isFullyNumeric) w.WriteElementString("in", "true");
                if (_numeralSystem != NumeralSystem.Arabic) w.WriteElementString("ns", _numeralSystem.ToString());
                if (!string.IsNullOrEmpty(_originalString)) w.WriteElementString("os", _originalString);
                if (!string.IsNullOrEmpty(_prettyString)) w.WriteElementString("ps", _prettyString);
            }

            return stringBuilder.ToString();
        }

        #endregion

        #endregion

        #region Operators

        public static bool operator ==(PageCount a, PageCount b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(PageCount a, PageCount b)
        {
            return !(a == b);
        }

        public static implicit operator string(PageCount pageCount)
        {
            return pageCount.PrettyString;
        }

        public static implicit operator PageCount(string input)
        {
            return new PageCount(input);
        }

        #endregion
    }
}
