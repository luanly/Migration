using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SwissAcademic
{
    #region NumberStringCompareMode

    public enum NumberStringCompareMode
    {
        ByNumbersIfFullStringIsNumeric = 0, //DEFAULT - equivalent to obsolte UseNumberInformation = FALSE
        ByNumbersExtractedFromString = 1,   //equivalent to obsolete UseNumberInformation = TRUE 
        ByTextAndNumbersSegmentwise = 2 //NEW
    }

    #endregion NumberStringCompareMode

    public class NumberStringComparer
        :
        IComparer<string>,
        System.Collections.IComparer
    {
        #region Constructors

        public NumberStringComparer()
        {
        }

        #endregion Constructors

        #region Enums

        private enum SegmentType
        {
            Text,
            Number,
            RomanNumber
        }

        #endregion Enums

        #region Structs

        private struct Segment
        {
            public string text;
            public SegmentType type;
            public decimal number;
        }

        #endregion Structs

        #region Properties

        #region NumberStringCompareMode

        private NumberStringCompareMode _compareMode;

        public NumberStringCompareMode CompareMode
        {
            get
            {
                return _compareMode;
            }
            set
            {
                if (value == _compareMode) return;
                _compareMode = value;

                switch (_compareMode)
                {
                    case NumberStringCompareMode.ByTextAndNumbersSegmentwise:
                    case NumberStringCompareMode.ByNumbersExtractedFromString:
                        {
                            _useNumberInformation = true;
                        }
                        break;

                    default:
                    case NumberStringCompareMode.ByNumbersIfFullStringIsNumeric:
                        {
                            _useNumberInformation = false;
                        }
                        break;
                }
            }
        }

        #endregion NumberStringCompareMode

        #region RomanNumberMatchType

        public RomanNumberMatchType RomanNumberMatchType { get; set; }

        #endregion

        #region SortDirection

        public ListSortDirection SortDirectionNumbers { get; set; }

        public ListSortDirection SortDirectionText { get; set; }

        #endregion SortDirection

        #region SumUpMultipleMatches

        public bool SumUpMultipleMatches { get; set; }

        #endregion

        #region TreatEmptyAsOne

        public bool TreatEmptyAsOne { get; set; }

        #endregion

        #region UseNumberInformation

        private bool _useNumberInformation;

        [Obsolete("For UseNumberInformation = false use CompareMode = NumberStringCompareMode.ByNumbersIfFullStringIsNumeric and for UseNumberInformation = true use CompareMode = NumberStringCompareMode.ByNumbersExtractedFromString")]
        public bool UseNumberInformation
        {
            get
            {
                return _useNumberInformation;
            }
            set
            {
                if (value == _useNumberInformation) return;
                _useNumberInformation = value;

                if (_useNumberInformation)
                {
                    _compareMode = NumberStringCompareMode.ByNumbersExtractedFromString;
                }
                else
                {
                    _compareMode = NumberStringCompareMode.ByNumbersIfFullStringIsNumeric;
                }
            }
        }

        #endregion

        #region UseAbsoluteNumbersOnly

        //JHP added because sorting norms caused troubles, see BugInput 9000
        //Sorting the following in an ascending manner...
        //DIN EN ISO 10211-1; DIN EN ISO 10211-2; DIN EN ISO 10211-3
        //... resulted in ...
        //DIN EN ISO 10211-3; DIN EN ISO 10211-2; DIN EN ISO 10211-1
        //... because the second numbers were recognized as negatives (-3; -2; -1)

        public bool UseAbsoluteNumbersOnly { get; set; }

        #endregion UseAbsoluteNumbersOnly

        #endregion Properties

        #region Methods

        #region Compare

        public int Compare(string x, string y)
        {
            try
            {
                var directionFactorNumbers = SortDirectionNumbers == ListSortDirection.Ascending ? 1 : -1;
                var directionFactorStrings = SortDirectionText == ListSortDirection.Ascending ? 1 : -1;

                #region Treat IsNullOrEmpty(x and/or y)

                if (string.IsNullOrEmpty(x))
                {
                    if (string.IsNullOrEmpty(y)) return 0;
                    if (TreatEmptyAsOne) x = "1";
                    else return -1;
                }
                if (string.IsNullOrEmpty(y))
                {
                    if (TreatEmptyAsOne) y = "1";
                    else return 1;
                }

                #endregion Treat IsNullOrEmpty(x and/or y)

                switch (CompareMode)
                {
                    #region ByNumbersExtractedFromString

                    case NumberStringCompareMode.ByNumbersExtractedFromString:
                        {
                            //WAS: UseNumberInformation = TRUE

                            var xMatches = SwissAcademic.NumberInformation.Matches(x, RomanNumberMatchType, UseAbsoluteNumbersOnly);
                            var yMatches = SwissAcademic.NumberInformation.Matches(y, RomanNumberMatchType, UseAbsoluteNumbersOnly);

                            if (!xMatches.Any())
                            {
                                if (TreatEmptyAsOne)
                                {
                                    xMatches = SwissAcademic.NumberInformation.Matches("1");
                                }
                                else
                                {
                                    if (!yMatches.Any()) return StringComparer.CurrentCulture.Compare(x, y);
                                    return 1 * directionFactorNumbers;
                                }
                            }

                            if (!yMatches.Any())
                            {
                                if (TreatEmptyAsOne) yMatches = SwissAcademic.NumberInformation.Matches("1");
                                else return -1 * directionFactorNumbers;
                            }

                            if (SumUpMultipleMatches)
                            {
                                var xSum = xMatches.Sum(item => item.Number);
                                var ySum = yMatches.Sum(item => item.Number);
                                return xSum.CompareTo(ySum) * directionFactorNumbers;
                            }

                            int minCount = Math.Min(xMatches.Count, yMatches.Count);
                            int result;

                            for (int i = 0; i < minCount; i++)
                            {
                                result = xMatches[i].Number.CompareTo(yMatches[i].Number) * directionFactorNumbers;
                                if (result != 0) return result;
                            }

                            return xMatches.Count.CompareTo(yMatches.Count) * directionFactorNumbers;

                        }

                    #endregion ByNumbersExtractedFromString

                    #region ByStringsAndNumbersSegmentwise

                    case NumberStringCompareMode.ByTextAndNumbersSegmentwise:
                        {
                            //NEW variant of UseNumberInformation = TRUE

                            var xSegments = GetSegments(x);
                            var ySegments = GetSegments(y);

                            var minSegmentNumber = Math.Min(xSegments.Count(), ySegments.Count());

                            for (int i = 0; i < minSegmentNumber; i++)
                            {
                                var xSegment = xSegments.ElementAt(i);
                                var ySegment = ySegments.ElementAt(i);

                                if (xSegment.type == SegmentType.Text && ySegment.type == SegmentType.Text)
                                {
                                    var result = xSegment.text.CompareTo(ySegment.text) * directionFactorStrings;
                                    if (result != 0) return result;
                                }
                                else if (xSegment.type == SegmentType.Text && (ySegment.type == SegmentType.Number || ySegment.type == SegmentType.RomanNumber))
                                {
                                    return 1 * directionFactorStrings;
                                }
                                else if ((xSegment.type == SegmentType.Number || xSegment.type == SegmentType.RomanNumber) && ySegment.type == SegmentType.Text)
                                {
                                    return -1 * directionFactorStrings;
                                }
                                else if (xSegment.type == SegmentType.Number && ySegment.type == SegmentType.Number)
                                {
                                    var result = xSegment.number.CompareTo(ySegment.number) * directionFactorNumbers;
                                    if (result != 0) return result;
                                }
                                else if (xSegment.type == SegmentType.Number && ySegment.type == SegmentType.RomanNumber)
                                {
                                    return 1 * directionFactorNumbers;
                                }
                                else if (xSegment.type == SegmentType.RomanNumber && ySegment.type == SegmentType.Number)
                                {
                                    return -1 * directionFactorNumbers;
                                }
                            }

                            return 0;
                        }

                    #endregion ByStringsAndNumbersSegmentwise

                    #region ByNumbersIfFullStringIsNumeric

                    default:
                    case NumberStringCompareMode.ByNumbersIfFullStringIsNumeric:
                        {
                            //WAS: UseNumberInformation = FALSE

                            x = x.GetCompareValue();
                            y = y.GetCompareValue();

                            decimal xDecimal;
                            decimal yDecimal;

                            if (decimal.TryParse(x, out xDecimal))
                            {
                                if (decimal.TryParse(y, out yDecimal)) return xDecimal.CompareTo(yDecimal);
                                return 1 * directionFactorNumbers;
                            }
                            if (decimal.TryParse(y, out yDecimal)) return -1 * directionFactorNumbers;

                            return StringComparer.CurrentCulture.Compare(x, y) * directionFactorStrings;
                        }

                        #endregion ByNumbersIfFullStringIsNumeric
                }

            }

            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                return StringComparer.CurrentCulture.Compare(x, y);
            }
        }

        #endregion Compare

        #region GetSegments

        private IEnumerable<Segment> GetSegments(string completeString)
        {
            if (string.IsNullOrEmpty(completeString)) yield return new Segment() { text = completeString, type = SegmentType.Text, number = 0 };

            //var matches = SwissAcademic.NumberInformation.Matches(completeString, RomanNumberMatchType.AnyCase, true);
            var matches = SwissAcademic.NumberInformation.Matches(completeString, RomanNumberMatchType, UseAbsoluteNumbersOnly);

            if (matches.Count == 0)
            {
                yield return new Segment() { text = completeString, type = SegmentType.Text, number = 0 };
            }

            int currentPosition = 0;
            foreach (NumberMatch match in matches)
            {
                var matchPosition = match.Match.Index;
                var matchLength = match.Match.Length;
                var matchType = match.IsRoman ? SegmentType.RomanNumber : SegmentType.Number;

                if (matchPosition > currentPosition)
                {
                    //we add a simple non-numerical string fragment
                    yield return new Segment() { text = completeString.Substring(currentPosition, matchPosition - currentPosition).Trim(), type = SegmentType.Text, number = 0 };
                    currentPosition = matchPosition - currentPosition;
                }

                //we add the found numerical string fragment
                yield return new Segment() { text = completeString.Substring(matchPosition, matchLength).Trim(), type = matchType, number = match.Number };
                currentPosition = matchPosition + matchLength;
            }

            if (currentPosition <= completeString.Length - 1)
            {
                //we add a remaining simple non-numerical string fragment
                yield return new Segment() { text = completeString.Substring(currentPosition, completeString.Length - currentPosition).Trim(), type = SegmentType.Text, number = 0 };
            }
        }

        #endregion GetSegments

        #endregion Methods

        #region IComparer Member

        int System.Collections.IComparer.Compare(object x, object y)
        {
            return Compare(x as string, y as string);
        }

        #endregion
    }
}
