using SwissAcademic.ApplicationInsights;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace SwissAcademic
{
    [DataContract]
    public struct PageNumber
        :
        IEquatable<PageNumber>,
        IComparable<PageNumber>,
        IEquatable<string>
    {

        #region Properties

        #region Empty

        public static readonly PageNumber Empty = new PageNumber();

        #endregion

        #region IsFullyNumeric

        bool _isFullyNumeric;

        [DataMember]
        public bool IsFullyNumeric
        {
            get { return _isFullyNumeric; }
            private set { _isFullyNumeric = value; }
        }

        #endregion

        #region IsRoman

        public bool IsRoman
        {
            get { return NumeralSystem == NumeralSystem.RomanLowerCase || NumeralSystem == NumeralSystem.RomanUpperCase; }
        }

        #endregion

        #region Number

        int? _number;
        [DataMember]
        public int? Number
        {
            get { return _number; }
            set { _number = value; }
        }

        #endregion

        #region NumberingType

        NumberingType _numberingType;
        [DataMember]
        public NumberingType NumberingType
        {
            get { return _numberingType; }
            private set { _numberingType = value; }
        }

        #endregion

        #region NumeralSystem

        NumeralSystem _numeralSystem;
        [DataMember]
        public NumeralSystem NumeralSystem
        {
            get { return _numeralSystem; }
            private set { _numeralSystem = value; }
        }

        #endregion

        #region OriginalString

        string _originalString;
        [DataMember]
        public string OriginalString
        {
            get { return _originalString; }
            private set { _originalString = value; }
        }

        #endregion

        #region PrettyString

        string _prettyString;
        [DataMember]
        public string PrettyString
        {
            get { return _prettyString ?? string.Empty; }
            private set { _prettyString = value; }
        }

        #endregion

        #endregion

        #region Methoden

        #region Compare

        public static int Compare(PageNumber x, PageNumber y)
        {
            //ML 29.01.19: in case of changes to this routine please change JS-method accordingly:
            //SwissAcademic.Citavi.Web/scr/classes/singletons/utilities.js#comparePagerangePage            
            try
            {
                if (x == PageNumber.Empty)
                {
                    if (y == PageNumber.Empty) return 0;
                    return -1;
                }

                if (y == PageNumber.Empty) return 1;

                if (x.Number.HasValue)
                {
                    if (y.Number.HasValue)
                    {
                        if (x.IsRoman)
                        {
                            if (y.IsRoman) return x.Number.Value.CompareTo(y.Number.Value);
                            return -1;
                        }
                        else if (y.IsRoman) return 1;

                        return x.Number.Value.CompareTo(y.Number.Value);
                    }

                    return -1;
                }

                else if (y.Number.HasValue)
                {
                    return 1;
                }

                return StringComparer.CurrentCulture.Compare(x.PrettyString, y.PrettyString);
            }

            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                return StringComparer.CurrentCulture.Compare(x.PrettyString, y.PrettyString);
            }
        }

        #endregion

        #region CompareTo

        public int CompareTo(PageNumber other)
        {
            return Compare(this, other);
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is PageNumber) return Equals((PageNumber)obj);
            return false;
        }

        public bool Equals(PageNumber other)
        {
            return Equals(other, PageNumberComparison.Default);
        }

        public bool Equals(PageNumber other, PageNumberComparison comparison)
        {
            // if PrettyString is null, we don't care about NumberingType - empty is empty
            //Debug.Assert(PrettyString != string.Empty && other.PrettyString != string.Empty);

            switch (comparison)
            {
                case PageNumberComparison.IgnoreNumberingType:
                    return StringComparer.Ordinal.Compare(PrettyString, other.PrettyString) == 0 &&
                        NumeralSystem == other.NumeralSystem;

                case PageNumberComparison.IgnoreNumeralSystem:
                    return StringComparer.Ordinal.Compare(PrettyString, other.PrettyString) == 0 &&
                        NumberingType == other.NumberingType;

                case PageNumberComparison.PrettyStringOnly:
                    return StringComparer.Ordinal.Compare(PrettyString, other.PrettyString) == 0;

                default:
                    return StringComparer.Ordinal.Compare(PrettyString, other.PrettyString) == 0 &&
                        NumberingType == other.NumberingType &&
                        NumeralSystem == other.NumeralSystem;
            }
        }

        public bool Equals(string other)
        {
            return Equals(PageNumber.Empty.Update(other));
        }

        #endregion

        #region FromXml

        public static PageNumber FromXml(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return PageNumber.Empty;

            using (var stringReader = new StringReader(xml))
            {
                using (var r = XmlReader.Create(stringReader, XmlReaderExtensions.FragmentSettings))
                {
                    return FromXml(r);
                }
            }
        }

        public static PageNumber FromXml(XmlReader r)
        {
            var pageNumber = new PageNumber();

            while (r.ReadToElement())
            {
                switch (r.Name)
                {
                    case "in":
                        pageNumber._isFullyNumeric = r.ReadElementContentAsBoolean();
                        break;

                    case "n":
                        pageNumber._number = System.Convert.ToInt32(r.ReadElementContentAsInt());
                        break;

                    case "nt":
                        pageNumber._numberingType = r.ReadEnum<NumberingType>();
                        break;

                    case "ns":
                        pageNumber._numeralSystem = r.ReadEnum<NumeralSystem>();
                        break;

                    case "os":
                        pageNumber._originalString = r.ReadElementContentAsString();
                        break;

                    case "ps":
                        pageNumber._prettyString = r.ReadElementContentAsString();
                        break;
                }
            }

            return pageNumber;
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

        static PageNumber Parse(string input)
        {
            var orignalString = input.Clean(IllegalCharacters.Return | IllegalCharacters.Tab);
            if (string.IsNullOrWhiteSpace(orignalString)) return PageNumber.Empty;


            int? count = null;
            int? firstMatch = null;
            var numeralSystem = NumeralSystem.Omit;
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

                //PageRange '120921130020003' leads to error: 'Der Wert für einen Int32 war zu gross oder zu klein'
                var first = matches.FirstOrDefault();
                if (first != null && first.Number <= Int32.MaxValue)
                {
                    firstMatch = Convert.ToInt32(Math.Abs(Math.Floor(first.Number)));
                }
            }

            return new PageNumber
            {
                _number = firstMatch,
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
            if (string.IsNullOrEmpty(PrettyString)) return null;

            var stringBuilder = new StringBuilder();

            using (var w = XmlWriterSafe.Create(stringBuilder, XmlWriterExtensions.FragmentSettings))
            {
                ToXml(w);
            }

            return stringBuilder.ToString();
        }

        public void ToXml(XmlWriterSafe w)
        {
            if (_number.HasValue)
            {
                w.WriteStartElement("n");
                w.WriteValue(_number);
                w.WriteEndElement();
            }

            if (_isFullyNumeric) w.WriteElementString("in", "true");
            if (_numberingType != NumberingType.Page) w.WriteElementString("nt", _numberingType.ToString());
            if (_numeralSystem != NumeralSystem.Arabic) w.WriteElementString("ns", _numeralSystem.ToString());
            if (!string.IsNullOrEmpty(_originalString)) w.WriteElementString("os", _originalString);
            if (!string.IsNullOrEmpty(_prettyString)) w.WriteElementString("ps", _prettyString);
        }

        #endregion

        #region Update

        public PageNumber Update(int value)
        {
            return new PageNumber
            {
                _isFullyNumeric = true,
                _number = value,
                _numberingType = _numberingType,
                _numeralSystem = NumeralSystem.Arabic,
                _prettyString = value.ToString(),
                _originalString = value.ToString()
            };
        }

        public PageNumber Update(string value)
        {
            value = value.Clean(IllegalCharacters.Return | IllegalCharacters.Tab);

            if (string.IsNullOrEmpty(value))
            {
                return new PageNumber
                {
                    _isFullyNumeric = false,
                    _number = null
                };
            }

            var parsed = Parse(value);
            parsed._numberingType = _numberingType;
            return parsed;
        }

        public PageNumber Update(NumberingType numberingType)
        {
            if (numberingType == _numberingType) return this;

            return new PageNumber
            {
                _isFullyNumeric = _isFullyNumeric,
                _number = _number,
                _numberingType = numberingType,
                _numeralSystem = _numeralSystem,
                _prettyString = _prettyString,
                _originalString = _originalString
            };
        }

        public PageNumber Update(NumeralSystem numeralSystem)
        {
            if (_numeralSystem == numeralSystem) return this;


            if (numeralSystem == NumeralSystem.Omit)
            {
                var newPageNumber = new PageNumber
                {
                    _isFullyNumeric = false,
                    _number = null,
                    _numberingType = _numberingType,
                    _numeralSystem = numeralSystem,
                    _prettyString = _originalString,
                    _originalString = _originalString
                };

                return newPageNumber;
            }

            var pageCount = new PageCount(_number, _number, _isFullyNumeric, _numeralSystem, _originalString, _prettyString).Convert(numeralSystem);

            return new PageNumber
            {
                _isFullyNumeric = pageCount.IsFullyNumeric,
                _number = pageCount.FirstMatch,
                _numberingType = _numberingType,
                _numeralSystem = pageCount.NumeralSystem,
                _prettyString = pageCount.PrettyString,
                _originalString = _originalString
            };
        }

        public PageNumber CorrectPageCount(int? value)
        {
            return new PageNumber
            {
                _isFullyNumeric = _isFullyNumeric,
                _number = value,
                _numberingType = _numberingType,
                _numeralSystem = _numeralSystem,
                _prettyString = _prettyString,
                _originalString = _originalString
            };
        }

        #endregion

        #endregion

        #region Operators

        public static bool operator ==(PageNumber a, PageNumber b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(PageNumber a, PageNumber b)
        {
            return !(a == b);
        }

        #endregion
    }

    public enum PageNumberComparison
    {
        Default,
        IgnoreNumberingType,
        IgnoreNumeralSystem,
        PrettyStringOnly
    }
}
