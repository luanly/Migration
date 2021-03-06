using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace SwissAcademic
{
    #region DateTimeMatch

    public class DateTimeMatch
    {
        #region Konstruktoren

        internal DateTimeMatch(Match match, DateTime dateTime, bool missingMonthWasAutoCompleted, bool missingDayWasAutoCompleted)
        {
            _match = match;
            _dateTime = dateTime;

            MissingMonthWasAutoCompleted = missingMonthWasAutoCompleted;
            MissingDayWasAutoCompleted = missingDayWasAutoCompleted;
        }

        #endregion

        #region Eigenschaften

        #region DateTime

        DateTime _dateTime;

        public DateTime DateTime
        {
            get { return _dateTime; }
        }

        #endregion

        #region Match

        Match _match;

        public Match Match
        {
            get { return _match; }
        }

        #endregion

        #region MissingDayWasAutoCompleted

        public bool MissingDayWasAutoCompleted { get; private set; }

        #endregion

        #region MissingInformationWasAutoCompleted

        public bool MissingInformationWasAutoCompleted
        {
            get { return MissingDayWasAutoCompleted || MissingMonthWasAutoCompleted; }
        }

        #endregion

        #region MissingMonthWasAutoCompleted

        public bool MissingMonthWasAutoCompleted { get; private set; }

        #endregion

        #endregion

        #region Methoden

        #region MatchesFormat

        public bool MatchesFormat(string format)
        {
            if (MissingDayWasAutoCompleted || MissingMonthWasAutoCompleted)
            {
                switch (format)
                {
                    //http://msdn.microsoft.com/en-us/library/az4se3k1.aspx
                    case "d":
                    case "D":
                    case "f":
                    case "F":
                    case "g":
                    case "G":
                    case "M":
                    case "m":
                    case "O":
                    case "o":
                    case "R":
                    case "r":
                    case "s":
                    case "t":
                    case "T":
                    case "u":
                    case "U":
                        return false;
                }
            }


            if (MissingDayWasAutoCompleted)
            {
                var index = format.IndexOf('d');
                while (index != -1)
                {
                    if (index == 0 || format[index - 1] != '\\') return false;
                    index = format.IndexOf('d', index + 1);
                }
            }

            if (MissingMonthWasAutoCompleted)
            {
                switch (format)
                {
                    //http://msdn.microsoft.com/en-us/library/az4se3k1.aspx
                    //Year month pattern
                    case "Y":
                    case "y":
                        return false;
                }

                var index = format.IndexOf('M');
                while (index != -1)
                {
                    if (index == 0 || format[index - 1] != '\\') return false;
                    index = format.IndexOf('M', index + 1);
                }
            }

            return true;
        }

        #endregion

        #endregion
    }

    #endregion

    #region DateTimeMatchInternal

    class DateTimeMatchInternal
    {
        public Match RegexMatch { get; set; }
        public DateTimePattern Pattern { get; set; }
    }

    #endregion

    #region DateTimePattern

    enum DateTimePattern
    {
        Unkown,
        MonthNameDayYear,
        RIS,
        Technical,
        American,
        General,
        ParseExact
    }

    #endregion

    #region DateTimeFormatPattern

    public static class DateTimeFormatPattern
    {
        public const string SortableDateTime = "yyyy-MM-dd HH-mm";
    }

    #endregion

    #region DateTimeInformation

    public static class DateTimeInformation
    {
        #region Statische Regex-Ausdrücke

        static readonly Regex ContainsAmericanDate = new Regex(@"(?<=\b)(?:(?#StartMonthGroup)(?<StartMonth>1[0-2]|0?[1-9])(?:(?#StartMonthSeparator)\.)?(?:(?#StartDayGroup)/(?<StartDay>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#StartDaySeparator)\.)?)?(?:(?#StartMonthConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?:(?#MonthGroup)(?<Month>1[0-2]|0?[1-9])(?:(?#MonthSeparatorGroup)\.?\p{Zs}*/\p{Zs}*))(?:(?#StartDayGroup)(?<StartDay>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#StartDaySeparator)\.)?(?:(?#StartDayConnector)\p{Zs}*\p{Pd}\p{Zs}*))?(?:(?#DayGroup)(?<Day>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#DaySeparatorGroup)\.?\p{Zs}*/\p{Zs}*))(?<Year>[1-2][0-9][0-9][0-9]) (?:(?#TimeGroup)(?:(?#TimeSeparator)(?:\p{Zs}|\p{P}){1,3})  (?<Hour>0?[0-9]|1[0-9]|2[0-4])  (?<MinuteSeparator>\p{Pd}|:) (?<Minute>[0-5][0-9]) (?:(?#SecondGroup) (?:(?#SecondSeparator)\k<MinuteSeparator>) (?<Second>[0-5][0-9])|(?:\p{Zs}|$)))?(?=\b)", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        static readonly Regex ContainsGeneralDate = new Regex(@"(?:(?#DaysMonthsYearGroup)(?<=\b)(?:(?#DaysAndMonthsGroup)(?:(?#DayAndMonthRangeGroup)(?:(?#DayGroup)(?<Day>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#DaySeparator)\.\p{Zs}*|\.?[\p{Zs}\p{Pd}]+))(?:(?#MonthGroup)(?<Month>1[0-2]|0?[1-9])(?:(?#MonthSeparator)\.)?(?:(?#MonthConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))(?:(?#EndDayGroup)(?:1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#EndDaySeparator)\.\p{Zs}*|\.?[\p{Zs}\p{Pd}]+))(?:(?#EndMonthGroup)(?:1[0-2]|0?[1-9])(?:(?#EndMonthSeparator)\.\p{Zs}*|\.?[\p{Zs}\p{Pd}]+)))|(?:(?#DaysAndMonthsWithSeparatorGroup)(?:(?#StartDayGroup)(?<StartDay>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#StartDaySeparator)\.)?(?:(?#StartDayConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?:(?#DayGroup)(?<Day>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#DaySeparatorGroup)\.?\p{Zs}*(?<DaySeparator>[\p{Pd}/])\p{Zs}*))(?:(?#StartMonthGroup)(?<StartMonth>1[0-2]|0?[1-9])(?:(?#StartMonthSeparator)\.)?(?:(?#StartMonthConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?:(?#MonthGroup)(?<Month>1[0-2]|0?[1-9])(?:(?#MonthSeparatorGroup)\.?\p{Zs}*\k<DaySeparator>\p{Zs}*)))|(?:(?#DaysAndMonthsWithoutSeparatorGroup)(?:(?#StartDayGroup)(?<StartDay>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#StartDaySeparator)\.)?(?:(?#StartDayConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?:(?#DayGroup)(?<Day>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#DaySeparator)\.\p{Zs}*))(?:(?#StartMonthGroup)(?<StartMonth>1[0-2]|0?[1-9])(?:(?#StartMonthSeparator)\.)?(?:(?#StartMonthConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?:(?#MonthGroup)(?<Month>1[0-2]|0?[1-9])(?:(?#MonthSeparator)\.\p{Zs}*)))|(?:(?#MonthsOnlyGroup)(?:(?#StartMonthGroup)(?<StartMonth>1[0-2]|0?[1-9])(?:(?#StartMonthSeparator)\.)?(?:(?#StartMonthConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?:(?#MonthGroup)(?<Month>1[0-2]|0?[1-9])(?:(?#MonthSeparatorGroup)\.\p{Zs}*|\.?\p{Zs}*[\p{Pd}/]\p{Zs}*)))|(?:(?#DayAndMonthRangeAsWordsGroup)(?:(?#DayGroup)(?<Day>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#DaySeparator)\.\p{Zs}*|\.?[\p{Zs}\p{Pd}]+))(?:(?#MonthGroup)(?<Month>\p{L}+)(?:(?#MonthSeparator)\.)?(?:(?#MonthConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))(?:(?#EndDayGroup)(?:1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#EndDaySeparator)\.\p{Zs}*|\.?[\p{Zs}\p{Pd}]+))(?:(?#EndMonthGroup)(?:\p{L}+)(?:(?#EndMonthSeparator)\.?\p{Zs}+)))|(?:(?#DaysAndMonthsAsWordsGroup)(?:(?#StartDayGroup)(?<StartDay>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#StartDaySeparator)\.)?(?:(?#StartDayConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?:(?#DayGroup)(?<Day>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#DaySeparator)\.?\p{Zs}*))(?:(?#StartMonthGroup)(?<StartMonth>\p{L}+)(?:(?#StartMonthSeparator)\.)?(?:(?#StartMonthConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?:(?#MonthGroup)(?<Month>\p{L}+)(?:(?#MonthSeparator)\.?\p{Zs}+)))|(?:(?#MonthsAsWordsGroup)(?:(?#StartMonthGroup)(?<StartMonth>\p{L}+)(?:(?#StartMonthSeparator)\.)?(?:(?#StartMonthConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?:(?#MonthGroup)(?<Month>\p{L}+)(?:(?#MonthSeparator)\.?\p{Zs}+))))(?<Year>[1-2][0-9][0-9][0-9]) (?:(?#TimeGroup)(?:(?#TimeSeparator)(?:\p{Zs}|\p{P}){1,3})  (?<Hour>0?[0-9]|1[0-9]|2[0-4])  (?<MinuteSeparator>\p{Pd}|:) (?<Minute>[0-5][0-9]) (?:(?#SecondGroup) (?:(?#SecondSeparator)\k<MinuteSeparator>) (?<Second>[0-5][0-9])|(?:\p{Zs}|$)))? |(?:(?#YearOnlyGroup)(?<=\b)(?<!\d\p{Zs}*)(?<Year>[1-2][0-9][0-9][0-9])))(?=\b)", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        static readonly Regex ContainsMonthNameDayYearDate = new Regex(@"(?<!\d\.?\p{Zs}*)(?:(?#DateGroup)(?:(?#MonthGroup)(?:(?#StartMonthGroup) (?<StartMonth>January|Jan|February|Feb|March|Mar|April|Apr|May|June|Jun|July|Jul|August|Aug|September|Sep|Sept|October|Oct|November|Nov|December|Dec) (?:(?#StartMonthSeparator)\p{Zs}*\.?\p{Zs}*)(?:(?#StartDayGroup)(?<StartDay>1[0-9]|2[0-9]|3[0-1]|0?[1-9]))?(?:(?#StartMonthConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?<Month>January|Jan|February|Feb|March|Mar|April|Apr|May|June|Jun|July|Jul|August|Aug|September|Sep|Sept|October|Oct|November|Nov|December|Dec) (?:(?#MonthSeparator)\p{Zs}*\.?,?\p{Zs}*))(?:(?#DayGroup)(?:(?#StartDayGroup)(?<StartDay>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#StartDayConnector)\p{Zs}*[\p{Pd}/]\p{Zs}*))?(?<Day>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#DaySeparator)\p{Zs}*,?\p{Zs}*))?(?<Year>000[1-9]|00[1-9][0-9]|0[1-9][0-9][0-9]|[1-2][0-9][0-9][0-9])(?:(?#TimeGroup)(?:(?#TimeSeparator)(?:\p{Zs}|\p{P}){1,3})  (?<Hour>0?[0-9]|1[0-9]|2[0-4])  (?<MinuteSeparator>\p{Pd}|:) (?<Minute>[0-5][0-9]) (?:(?#SecondGroup) (?:(?#SecondSeparator)\k<MinuteSeparator>) (?<Second>[0-5][0-9])|(?:\p{Zs}|$)))?)  (?=\b)", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        static readonly Regex ContainsRISDate = new Regex(@"(?<=\b)(?<Year>[1-2][0-9][0-9][0-9])(?:(?#MonthGroup)(?:(?#MonthSeparator)/)(?<Month>1[0-2]|0?[1-9])?)(?:(?#DayGroup)(?:(?#DaySeparator)/)(?:(?<Day>1[0-9]|2[0-9]|3[0-1]|0?[1-9])?(?:(?#EndDayGroup)(?:(?#EndDayConnector)\p{Pd})(?:(?#EndDay)1[0-9]|2[0-9]|3[0-1]|0?[1-9]))?)?)(?:(?#OtherInfoGroup)(?:(?#OtherInfoSeparator)/)(?:(?#OtherInfo)[\p{L}\p{N}\p{Pd}][\p{L}\p{N}\p{Pd}\p{Zs}]*)?)?", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        static readonly Regex ContainsTechnicalDate = new Regex(@"(?<=\b)(?<Year>000[1-9]|00[1-9][0-9]|0[1-9][0-9][0-9]|[1-2][0-9][0-9][0-9])  (?:(?#MonthGroup)(?<MonthSeparator>/|\p{Pd}|\.) (?<Month>1[0-2]|0?[1-9]|\p{L}+))(?:(?:(?#DayOrEndMonthGroup)(?:(?#DayGroup)(?:(?#DaySeparator)\k<MonthSeparator>) (?<Day>1[0-9]|2[0-9]|3[0-1]|0?[1-9])(?:(?#EndDayGroup)(?:(?#EndDayConnector)\p{Zs}*?(?:\p{Pd}|/)\p{Zs}*?)(?:(?#EndDay)1[0-9]|2[0-9]|3[0-1]|0?[1-9]))?)  |(?:(?#EndMonthGroup)(?:(?#EndMonthConnector)/|\p{Pd})(?:(?#EndMonth)1[0-2]|0?[1-9]|\p{L}+))|(?:\p{Zs}|$) )(?:(?#TimeGroup)(?:(?#TimeSeparator)(?:\p{Zs}|\p{P}|T){1,3})  (?<Hour>0?[0-9]|1[0-9]|2[0-4])  (?<MinuteSeparator>\p{Pd}|:) (?<Minute>[0-5][0-9]) (?:(?#SecondGroup) (?:(?#SecondSeparator)\k<MinuteSeparator>) (?<Second>[0-5][0-9])|(?:\p{Zs}|$)))?) (?=\b)", RegexOptions.IgnorePatternWhitespace | RegexOptions.CultureInvariant);
        static readonly Regex ContainsHoursAndMinutes = new Regex(@"(?:(?<hours>0?[0-9]|1[0-2]):(?<minutes>[0-5][0-9]) *(?<ampm>[ap]m)|(?:(?<hours>[01][0-9]|2[0-3])):(?<minutes>[0-5][0-9]))");
        static readonly Regex SimpleYear = new Regex(@"^(?<Year>000[1-9]|00[1-9][0-9]|0[1-9][0-9][0-9]|[1-2][0-9][0-9][0-9])$");
        static readonly Regex AllMatch = new Regex(@".*");

        #endregion

        #region Felder

        static Dictionary<string, int> _monthNameDictionary;
        static Dictionary<string, int> _seasonNameDictionary;

        #endregion

        #region Konstruktoren

        static DateTimeInformation()
        {
            _monthNameDictionary = new Dictionary<string, int>(300, StringComparer.OrdinalIgnoreCase);

            // http://tfs2012:8080/tfs/CITAVICollection/Citavi/_workitems/edit/7481
            // LOCALIZATION: für den Moment zurückstellen
            var cultureInfos = new List<CultureInfo>();
            cultureInfos.Add(CultureInfo.GetCultureInfo("en-US"));
            cultureInfos.Add(CultureInfo.GetCultureInfo("de-DE"));
            cultureInfos.Add(CultureInfo.GetCultureInfo("fr-FR"));
            cultureInfos.Add(CultureInfo.GetCultureInfo("it-IT"));
            cultureInfos.Add(CultureInfo.GetCultureInfo("es-ES"));
            cultureInfos.Add(CultureInfo.GetCultureInfo("pl-PL"));
            cultureInfos.Add(CultureInfo.GetCultureInfo("pt-PT"));
            cultureInfos.Add(CultureInfo.GetCultureInfo("ru-RU"));
            cultureInfos.Add(CultureInfo.GetCultureInfo("sv-SE"));
            cultureInfos.Add(CultureInfo.GetCultureInfo("nl-NL"));

            foreach (CultureInfo culture in cultureInfos)
            {
                for (int i = 0; i < 12; i++)
                {
                    _monthNameDictionary[culture.DateTimeFormat.AbbreviatedMonthNames[i].Trim(new char[] { '.' })] = i + 1;
                    _monthNameDictionary[culture.DateTimeFormat.MonthNames[i]] = i + 1;
                }
            }

            if (!cultureInfos.Contains(CultureInfo.CurrentCulture))
            {
                for (int i = 0; i < 12; i++)
                {
                    _monthNameDictionary[CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedMonthNames[i].Trim(new char[] { '.' })] = i + 1;
                    _monthNameDictionary[CultureInfo.CurrentCulture.DateTimeFormat.MonthNames[i]] = i + 1;
                }
            }

            _seasonNameDictionary = new Dictionary<string, int>(40, StringComparer.OrdinalIgnoreCase);

            AddSeasonNames(new[] { "de", "es", "fr", "it", "nl", "pl", "pt", "en", "ru", "sv" });
        }

        static void AddSeasonNames(string[] cultureNames)
        {
            foreach (var cultureName in cultureNames)
            {
                var seasonCulture = CultureInfo.GetCultureInfo(cultureName);

                var seasonNames = Properties.Resources.ResourceManager.GetString("Spring", seasonCulture).Split('|');
                foreach (var seasonName in seasonNames) _seasonNameDictionary[seasonName] = 1;

                seasonNames = Properties.Resources.ResourceManager.GetString("Summer", seasonCulture).Split('|');
                foreach (var seasonName in seasonNames) _seasonNameDictionary[seasonName] = 2;

                seasonNames = Properties.Resources.ResourceManager.GetString("Autumn", seasonCulture).Split('|');
                foreach (var seasonName in seasonNames) _seasonNameDictionary[seasonName] = 3;

                seasonNames = Properties.Resources.ResourceManager.GetString("Winter", seasonCulture).Split('|');
                foreach (var seasonName in seasonNames) _seasonNameDictionary[seasonName] = 4;
            }
        }

        #endregion

        #region Eigenschaften

        #region DefaultFormat

        static string _defaultFormat;

        public static string DefaultFormat
        {
            get { return _defaultFormat; }
            set
            {
                value = value.Clean(IllegalCharacters.Return);

                if (string.IsNullOrEmpty(value))
                {
                    _defaultFormat = null;
                }

                else
                {
                    // https://msdn.microsoft.com/en-us/library/362btx8f%28v=vs.90%29.aspx?f=255&MSPPError=-2147217396
                    switch (value)
                    {
                        case "G":
                        case "D":
                        case "d":
                        case "T":
                        case "t":
                        case "f":
                        case "F":
                        case "g":
                        case "M":
                        case "m":
                        case "R":
                        case "r":
                        case "s":
                        case "u":
                        case "U":
                        case "Y":
                        case "y":
                            _defaultFormat = null;
                            return;

                        default:
                            _defaultFormat = value;
                            break;
                    }
                }
            }
        }

        #endregion

        #region NextSunday

        public static DateTime NextSunday
        {
            get
            {
                DateTime dateTime = DateTime.Today;

                int dayNumber = (int)dateTime.DayOfWeek;
                if (dayNumber == 0)
                {
                    return dateTime;
                }

                return dateTime.AddDays(7 - dayNumber);
            }
        }

        #endregion

        #endregion

        #region Methoden

        #region ContainsTime

        public static bool ContainsTime(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString)) return false;

            return ContainsHoursAndMinutes.IsMatch(dateTimeString);
        }

        #endregion

        #region ContainsFullyQualifiedDate

        public static bool ContainsFullyQualifiedDate(string dateTimeString)
        {
            if (string.IsNullOrEmpty(dateTimeString)) return false;

            DateTime dateTime;
            return TryParseExact(dateTimeString, out dateTime);
        }

        #endregion

        #region Matches

        public static List<DateTimeMatch> Matches(string dateTimeString)
        {
            var matches = new List<DateTimeMatch>();

            if (string.IsNullOrEmpty(dateTimeString))
            {
                return matches;
            }

            var searchMatches = SearchMatches(dateTimeString);
            if (searchMatches.FirstOrDefault()?.Pattern == DateTimePattern.ParseExact)
            {
                var dateTime = DateTime.ParseExact(dateTimeString, _defaultFormat, DateTimeFormatInfo.CurrentInfo);
                matches.Add(new DateTimeMatch(searchMatches.First().RegexMatch, dateTime, false, false));
                return matches;
            }

            foreach (var match in searchMatches)
            {
                int year;
                int month = 1;
                int season = -1;
                int day = 1;
                int hour = 0;
                int minute = 0;
                int second = 0;
                int startDay = -1;
                int startMonth = -1;
                int startYear = -1;

                var missingDayWasAutoCompleted = true;
                var missingMonthWasAutoCompleted = true;

                if (!int.TryParse(match.RegexMatch.Groups["Year"].Value, out year)) continue;

                if (!string.IsNullOrEmpty(match.RegexMatch.Groups["StartMonth"].Value))
                {
                    if (int.TryParse(match.RegexMatch.Groups["StartMonth"].Value, out startMonth))
                    {
                        if (startMonth < 1 || startMonth > 12) startMonth = -1;
                    }

                    else
                    {
                        if (TryParseSeasonName(match.RegexMatch.Groups["StartMonth"].Value, out season))
                        {
                            hour = 23;
                            minute = 59;
                            second = 59;

                            switch (season)
                            {
                                case 1:
                                    startDay = DateTime.IsLeapYear(year) ? 29 : 28;
                                    startMonth = 2;
                                    break;

                                case 2:
                                    startDay = 31;
                                    startMonth = 5;
                                    break;

                                case 3:
                                    startDay = 31;
                                    startMonth = 8;
                                    break;

                                case 4:
                                    startDay = 31;
                                    startMonth = 12;
                                    startYear = year - 1;
                                    break;
                            }
                        }
                        else
                        {
                            TryParseMonthName(match.RegexMatch.Groups["StartMonth"].Value, out startMonth);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(match.RegexMatch.Groups["Month"].Value))
                {
                    if (int.TryParse(match.RegexMatch.Groups["Month"].Value, out month))
                    {
                        if (month < 1 || month > 12)
                        {
                            var yearMatch = new Regex(year.ToString()).Match(dateTimeString, match.RegexMatch.Index, match.RegexMatch.Length);
                            matches.Add(new DateTimeMatch(yearMatch, new DateTime(year, 1, 1), true, true));
                            continue;
                        }
                        else if (startMonth != -1 && startMonth < month)
                        {
                            month = startMonth;
                            missingMonthWasAutoCompleted = false;
                        }
                        else
                        {
                            missingMonthWasAutoCompleted = false;
                        }
                    }

                    else
                    {
                        if (TryParseMonthName(match.RegexMatch.Groups["Month"].Value, out month))
                        {
                            missingMonthWasAutoCompleted = false;

                            if (startMonth != -1 && startMonth < month) month = startMonth;
                        }

                        else if (TryParseSeasonName(match.RegexMatch.Groups["Month"].Value, out season))
                        {
                            hour = 23;
                            minute = 59;
                            second = 59;

                            switch (season)
                            {
                                case 1:
                                    day = DateTime.IsLeapYear(year) ? 29 : 28;
                                    month = 2;
                                    break;

                                case 2:
                                    day = 31;
                                    month = 5;
                                    break;

                                case 3:
                                    day = 31;
                                    month = 8;
                                    break;

                                case 4:
                                    day = 31;
                                    month = 12;
                                    year -= 1;
                                    break;
                            }

                            if (startMonth != -1 && (startMonth < month || startYear < year))
                            {
                                if (startDay != -1) day = startDay;
                                if (startYear != -1 && startYear < year) year = startYear;
                                month = startMonth;

                                hour = 0;
                                minute = 0;
                                second = 0;
                            }
                        }
                        else
                        {
                            var yearMatch = new Regex(year.ToString()).Match(dateTimeString, match.RegexMatch.Index, match.RegexMatch.Length);
                            matches.Add(new DateTimeMatch(yearMatch, new DateTime(year, 1, 1), true, true));
                            continue;
                        }
                    }
                }

                if (season == -1)
                {
                    if (!string.IsNullOrEmpty(match.RegexMatch.Groups["StartDay"].Value))
                    {
                        if (int.TryParse(match.RegexMatch.Groups["StartDay"].Value, out startDay))
                        {
                            if (startDay < 1 || startDay > DateTime.DaysInMonth(year, month))
                            {
                                startDay = -1;
                            }
                        }
                        else
                        {
                            startDay = -1;
                        }
                    }

                    if (string.IsNullOrEmpty(match.RegexMatch.Groups["Day"].Value))
                    {
                        if (startDay > 0)
                        {
                            // bei der Syntax "9./10. 2009" wird "9." vom Regex als StartDay gematcht,
                            // "10." als Monat; diesen Fall berücksichtigen wir hier.

                            var startDayConnector = match.RegexMatch.Groups["StartDayConnector"].Value;
                            var monthSeparator = match.RegexMatch.Groups["MonthSeparator"].Value;

                            if (
                                !string.IsNullOrEmpty(startDayConnector) &&
                                !string.IsNullOrEmpty(monthSeparator) &&
                                startDayConnector != monthSeparator &&
                                startDay < month)
                            {
                                month = startDay;
                            }

                            else
                            {
                                day = startDay;
                                missingDayWasAutoCompleted = false;
                            }
                        }
                    }
                    else
                    {
                        if (int.TryParse(match.RegexMatch.Groups["Day"].Value, out day))
                        {
                            if (startDay != -1 && startDay < day) day = startDay;
                            if (day < 1 || day > DateTime.DaysInMonth(year, month)) continue;

                            missingDayWasAutoCompleted = false;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                if (
                    !string.IsNullOrEmpty(match.RegexMatch.Groups["Hour"].Value) &&
                    !int.TryParse(match.RegexMatch.Groups["Hour"].Value, out hour))
                {
                    continue;
                }

                if (
                    !string.IsNullOrEmpty(match.RegexMatch.Groups["Minute"].Value) &&
                    !int.TryParse(match.RegexMatch.Groups["Minute"].Value, out minute))
                {
                    continue;
                }

                if (
                    !string.IsNullOrEmpty(match.RegexMatch.Groups["Second"].Value) &&
                    !int.TryParse(match.RegexMatch.Groups["Second"].Value, out second))
                {
                    continue;
                }

                try
                {
                    if (match.Pattern == DateTimePattern.American &&
                        !missingMonthWasAutoCompleted && !missingDayWasAutoCompleted &&
                        Thread.CurrentThread.CurrentCulture.DateTimeFormat.DateSeparator.Equals("/", StringComparison.Ordinal) &&
                        Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern.StartsWith("d", StringComparison.Ordinal))
                    {
                        var tempMonth = month;
                        month = day;
                        day = tempMonth;
                    }

                    var dateTime = new DateTime(year, month, day, hour, minute, second);
                    matches.Add(new DateTimeMatch(match.RegexMatch, dateTime, missingMonthWasAutoCompleted, missingDayWasAutoCompleted));
                }

                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Warning, ExceptionFlow.Eat);
                }
            }

            return matches;
        }

        #endregion

        #region SearchMatches

        static List<DateTimeMatchInternal> SearchMatches(string dateTimeString)
        {
            var all = new List<DateTimeMatchInternal>();

            var simpleYearMatch = SimpleYear.Match(dateTimeString);
            if (simpleYearMatch.Success)
            {
                all.Add(new DateTimeMatchInternal { RegexMatch = simpleYearMatch, Pattern = DateTimePattern.General });
                return all;
            }

            if (!string.IsNullOrEmpty(_defaultFormat) &&
                DateTime.TryParseExact(dateTimeString, _defaultFormat, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out var result))
            {
                var match = AllMatch.Match(dateTimeString);
                all.Add(new DateTimeMatchInternal
                {
                    RegexMatch = match,
                    Pattern = DateTimePattern.ParseExact
                });
                return all;
            }

            foreach (Match match in ContainsMonthNameDayYearDate.Matches(dateTimeString))
            {
                all.Add(new DateTimeMatchInternal { RegexMatch = match, Pattern = DateTimePattern.MonthNameDayYear });
            }

            var ris = new List<DateTimeMatchInternal>();
            foreach (Match match in ContainsRISDate.Matches(dateTimeString))
            {
                ris.Add(new DateTimeMatchInternal { RegexMatch = match, Pattern = DateTimePattern.RIS });
            }
            //JHP "prefix 2019-05 suffix" > hier findet der Match "2019-05 " mit trailing space, ärgerlich ...
            var technical = new List<DateTimeMatchInternal>();
            foreach (Match match in ContainsTechnicalDate.Matches(dateTimeString))
            {
                technical.Add(new DateTimeMatchInternal { RegexMatch = match, Pattern = DateTimePattern.Technical });
            }

            var american = new List<DateTimeMatchInternal>();
            foreach (Match match in ContainsAmericanDate.Matches(dateTimeString))
            {
                american.Add(new DateTimeMatchInternal { RegexMatch = match, Pattern = DateTimePattern.American });
            }

            var general = new List<DateTimeMatchInternal>();
            foreach (Match match in ContainsGeneralDate.Matches(dateTimeString))
            {
                general.Add(new DateTimeMatchInternal { RegexMatch = match, Pattern = DateTimePattern.General });
            }

            if (all.Any())
            {
                ris.RemoveAll(matchItem => all.Exists(existing =>
                    (matchItem.RegexMatch.Index >= existing.RegexMatch.Index && matchItem.RegexMatch.Index <= existing.RegexMatch.Index + existing.RegexMatch.Length) ||
                    (matchItem.RegexMatch.Index + matchItem.RegexMatch.Length >= existing.RegexMatch.Index && matchItem.RegexMatch.Index + matchItem.RegexMatch.Length <= existing.RegexMatch.Index + existing.RegexMatch.Length ||
                    matchItem.RegexMatch.Index <= existing.RegexMatch.Index && matchItem.RegexMatch.Index + matchItem.RegexMatch.Length >= existing.RegexMatch.Index + existing.RegexMatch.Length)));
            }

            all.AddRange(ris);

            if (all.Any())
            {
                technical.RemoveAll(matchItem => all.Exists(existing =>
                    (matchItem.RegexMatch.Index >= existing.RegexMatch.Index && matchItem.RegexMatch.Index <= existing.RegexMatch.Index + existing.RegexMatch.Length) ||
                    (matchItem.RegexMatch.Index + matchItem.RegexMatch.Length >= existing.RegexMatch.Index && matchItem.RegexMatch.Index + matchItem.RegexMatch.Length <= existing.RegexMatch.Index + existing.RegexMatch.Length ||
                    matchItem.RegexMatch.Index <= existing.RegexMatch.Index && matchItem.RegexMatch.Index + matchItem.RegexMatch.Length >= existing.RegexMatch.Index + existing.RegexMatch.Length)));
            }

            all.AddRange(technical);

            if (all.Any())
            {
                american.RemoveAll(matchItem => all.Exists(existing =>
                    (matchItem.RegexMatch.Index >= existing.RegexMatch.Index && matchItem.RegexMatch.Index <= existing.RegexMatch.Index + existing.RegexMatch.Length) ||
                    (matchItem.RegexMatch.Index + matchItem.RegexMatch.Length >= existing.RegexMatch.Index && matchItem.RegexMatch.Index + matchItem.RegexMatch.Length <= existing.RegexMatch.Index + existing.RegexMatch.Length ||
                    matchItem.RegexMatch.Index <= existing.RegexMatch.Index && matchItem.RegexMatch.Index + matchItem.RegexMatch.Length >= existing.RegexMatch.Index + existing.RegexMatch.Length)));
            }

            all.AddRange(american);

            if (all.Any())
            {
                general.RemoveAll(matchItem => all.Exists(existing =>
                    (matchItem.RegexMatch.Index >= existing.RegexMatch.Index && matchItem.RegexMatch.Index <= existing.RegexMatch.Index + existing.RegexMatch.Length) ||
                    (matchItem.RegexMatch.Index + matchItem.RegexMatch.Length >= existing.RegexMatch.Index && matchItem.RegexMatch.Index + matchItem.RegexMatch.Length <= existing.RegexMatch.Index + existing.RegexMatch.Length ||
                    matchItem.RegexMatch.Index <= existing.RegexMatch.Index && matchItem.RegexMatch.Index + matchItem.RegexMatch.Length >= existing.RegexMatch.Index + existing.RegexMatch.Length)));
            }

            all.AddRange(general);
            all.Sort((x, y) => x.RegexMatch.Index.CompareTo(y.RegexMatch.Index));

            return all;
        }

        #endregion

        #region TryParseExact

        /* Versucht, in einem String ein Datum zu erkennen - aber nur dann, wenn es sich um ein volles Datum handelt.
		 * Beispiel: Oktober 2005 wird nicht als Datum erkannt, 1.10.2005 schon. Anwendungsfall: Bei den Zitationsstilen
		 * soll die Datumsformatierung nur bei einem vollständigen Datum greifen. */
        public static bool TryParseExact(string dateTimeString, out DateTime result)
        {
            try
            {
                if (string.IsNullOrEmpty(dateTimeString))
                {
                    result = DateTime.MinValue;
                    return false;
                }

                List<DateTimeMatch> matches = DateTimeInformation.Matches(dateTimeString);
                if (matches.Count == 1 && !matches[0].MissingInformationWasAutoCompleted && matches[0].Match.Index == 0 && matches[0].Match.Length == dateTimeString.Length)
                {
                    result = matches[0].DateTime;
                    return true;
                }

                else
                {
                    result = DateTime.MinValue;
                    return false;
                }
            }

            catch (Exception ignored2)
            {
                Telemetry.TrackException(ignored2, SeverityLevel.Warning, ExceptionFlow.Eat);
                result = DateTime.MinValue;
                return false;
            }
        }

        #endregion

        #region TryParse

        public static bool TryParse(string dateTimeString, out DateTime result)
        {
            try
            {
                List<DateTimeMatch> matches = Matches(dateTimeString);
                if (!matches.Any())
                {
                    result = DateTime.MinValue;
                    return false;
                }

                result = matches[0].DateTime;
                return true;
            }

            catch (Exception ignored2)
            {
                Telemetry.TrackException(ignored2, SeverityLevel.Warning, ExceptionFlow.Eat);
                result = DateTime.MinValue;
                return false;
            }
        }

        #endregion

        #region TryParseMonthName

        public static bool TryParseMonthName(string monthName, out int month)
        {
            if (_monthNameDictionary.TryGetValue(monthName, out month))
            {
                return true;
            }

            month = -1;
            return false;
        }

        #endregion

        #region TryParseSeasonName

        public static bool TryParseSeasonName(string seasonName, out int season)
        {
            if (_seasonNameDictionary.TryGetValue(seasonName, out season))
            {
                return true;
            }

            season = -1;
            return false;
        }

        #endregion

        #endregion
    }

    #endregion
}