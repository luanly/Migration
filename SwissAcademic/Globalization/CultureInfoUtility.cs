using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SwissAcademic.Globalization
{
    public static class CultureInfoUtility
    {
        static Regex _cultureNameRegex = new Regex(@"^([A-Z][a-z]-)?[a-z]{2}(-[A-Z]{2,3})?$", RegexOptions.CultureInvariant);

        #region InstalledCultures

#if !Web
        static List<CultureInfo> _installedCultures;
        public static ReadOnlyCollection<CultureInfo> InstalledCultures
        {
            get
            {
                if (_installedCultures == null) FindInstalledCultures();
                return _installedCultures.AsReadOnly();
            }
        }

        static void FindInstalledCultures()
        {
            _installedCultures = new List<CultureInfo>();
            _installedCultures.Add(CultureInfo.GetCultureInfo("en"));

            var directory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory;
            var subdirectories = directory.GetDirectories();

            foreach (var subdirectory in subdirectories)
            {
                if (_cultureNameRegex.IsMatch(subdirectory.Name))
                {
                    // Add only cultures for which SwissAcademic.Resources exists
                    var resourcesTest = subdirectory.GetFilesSafe("SwissAcademic.Resources.resources.dll");
                    if (resourcesTest.Length == 0) continue;

                    try
                    {
                        var cultureInfo = CultureInfo.GetCultureInfo(subdirectory.Name);
                        _installedCultures.Add(cultureInfo);
                    }
                    catch (Exception exception)
                    {
                        Telemetry.TrackException(exception, SeverityLevel.Error, ExceptionFlow.Eat);
                    }
                }
            }
        }

#endif
        #endregion InstalledCultures

        #region LanguageRegex

        static Regex LanguageRegex = new Regex("dt|englisch", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        #endregion LanguageRegex

        #region LocalizedLanguageNamesAndAbbreviations

#if !Web
        private static IEnumerable<Tuple<string, string>> _localizedLanguageNamesAndAbbreviations;
        private static IEnumerable<Tuple<string, string>> GetLocalizedLanguageNamesAndAbbreviations()
        {
            foreach (var neutralCulture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
            {
                foreach (var installedCulture in InstalledCultures)
                {
                    var localizedNamesAndAbbreviations = SwissAcademic.Resources.ResourceHelper.GetLanguageName(neutralCulture.TwoLetterISOLanguageName, installedCulture);
                    if (string.IsNullOrEmpty(localizedNamesAndAbbreviations)) continue;

                    foreach (var name in localizedNamesAndAbbreviations.Split('|'))
                    {
                        yield return new Tuple<string, string>(name, neutralCulture.TwoLetterISOLanguageName);
                    }
                }
            }
        }

        public static IEnumerable<Tuple<string, string>> LocalizedLanguageNamesAndAbbreviations
        {
            get
            {
                if (_localizedLanguageNamesAndAbbreviations != null) return _localizedLanguageNamesAndAbbreviations;

                _localizedLanguageNamesAndAbbreviations = GetLocalizedLanguageNamesAndAbbreviations();
                return _localizedLanguageNamesAndAbbreviations.AsEnumerable();
            }
        }

#endif

        #endregion LocalizedLanguageNamesAndAbbreviations

        #region TryGetCultureInfo

        /// <summary>
        /// Checks a natural lanugage name or abbreviation for the language and returns true if successful, as well as the corresponding CultureInfo object.
        /// </summary>
        /// <param name="languageName">Any localized name or abbreviation of a language. e.g. "dt." for German or "russki" for Russian.</param>
        /// <param name="cultureInfo">The CultureInfo object relating to the language found (neutral or specific).</param>
        /// <returns></returns>
        public static bool TryGetCultureInfo(string languageName, out CultureInfo cultureInfo)
        {

            //languageName takes precedence over textSample, if both are present
            languageName = languageName.CleanExceptLetterDigitsDashes();

            #region Check languageName

            if (!string.IsNullOrEmpty(languageName))
            {

                #region Check languageName against most common TwoLetterISOLanguageNames

                switch (languageName.ToUpperInvariant())
                {
                    case "DE":
                        cultureInfo = CultureInfo.GetCultureInfo("de");
                        return true;

                    case "EN":
                        cultureInfo = CultureInfo.GetCultureInfo("en");
                        return true;

                    case "ES":
                        cultureInfo = CultureInfo.GetCultureInfo("es");
                        return true;

                    case "FR":
                        cultureInfo = CultureInfo.GetCultureInfo("fr");
                        return true;

                    case "IT":
                        cultureInfo = CultureInfo.GetCultureInfo("it");
                        return true;

                    case "NL":
                        cultureInfo = CultureInfo.GetCultureInfo("nl");
                        return true;

                    case "PL":
                        cultureInfo = CultureInfo.GetCultureInfo("pl");
                        return true;

                    case "PT":
                        cultureInfo = CultureInfo.GetCultureInfo("pt");
                        return true;

                    case "BR":
                        cultureInfo = CultureInfo.GetCultureInfo("br");
                        return true;

                    case "RU":
                        cultureInfo = CultureInfo.GetCultureInfo("ru");
                        return true;
                }

                #endregion

                #region Check languageName against AllCultures .Name, .DisplayName, .EnglishName, .NativeName, .TwoLetterISOLanguageName, .ThreeLetterISOLanguageName, .ThreeLetterWindowsLanguageName

                foreach (var cultureInfoTest in CultureInfo.GetCultures(CultureTypes.AllCultures))
                {
                    if
                    (
                      languageName.Equals(cultureInfoTest.Name, StringComparison.OrdinalIgnoreCase) ||
                      languageName.Equals(cultureInfoTest.DisplayName, StringComparison.OrdinalIgnoreCase) ||
                      languageName.Equals(cultureInfoTest.EnglishName, StringComparison.OrdinalIgnoreCase) ||
                      languageName.Equals(cultureInfoTest.NativeName, StringComparison.OrdinalIgnoreCase) ||
                      languageName.Equals(cultureInfoTest.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase) ||
                      languageName.Equals(cultureInfoTest.ThreeLetterISOLanguageName, StringComparison.OrdinalIgnoreCase) ||
                      languageName.Equals(cultureInfoTest.ThreeLetterWindowsLanguageName, StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        cultureInfo = cultureInfoTest;
                        return true;
                    }
                }

                #endregion Check languageName against AllCultures .Name, .DisplayName, .EnglishName, .NativeName, .ThreeLetterISOLanguageName, .ThreeLetterWindowsLanguageName

                #region Check languageName against Resource Infos

                var matches = LanguageRegex.Matches(languageName);
                if (matches.Count == 1)
                {
                    switch (matches[0].Value)
                    {
                        case "dt":
                        case "ger":
                            cultureInfo = CultureInfo.GetCultureInfo("de");
                            return true;

                        case "englisch":
                            cultureInfo = CultureInfo.GetCultureInfo("en");
                            return true;
                    }
                }

                #endregion Check languageName against Resource Infos
            }

            #endregion Check languageName

            cultureInfo = null;
            return false;
        }

        #endregion

        #region TryGetRegionInfo

        public static bool TryGetRegionInfo(string name, out RegionInfo regionInfo)
        {
            regionInfo = null;
            try
            {
                regionInfo = new RegionInfo(name);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        #endregion
    }

    public class Language
    {
        //TODO for ISOCode to work as key override Equals() and GetHashCode(), maybe check
        //usage of implicit cast operators to and from a string
        //public static implicit operator String(Language languge) { return ISOCode; }
        //maybe use Dictionary<Language, Action/Func> as a switch


        //https://en.wiktionary.org/wiki/Index:All_languages            primary source
        //https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes         secondary source

        //https://docs.microsoft.com/en-us/windows/desktop/intl/language-identifier-constants-and-strings

        #region Constructors

        private Language()
        {
        }

        #endregion

        #region Properties

        #region Culture

        public CultureInfo Culture
        {
            get
            {
                return new CultureInfo(ISOCode);
            }
        }

        #endregion

        #region LanguageIdentifier

        public int LanguageIdentifier { get; private set; }

        #endregion

        #region ISOCode

        /// <summary>
        /// ISO 639-1: two-letter codes, one per language for ISO 639 macrolanguage
        /// </summary>
        public string ISOCode { get; private set; }

        #endregion

        #region ISOCode2

        /// <summary>
        /// ISO 639-2/T: three-letter codes, for the same languages as 639-1
        /// https://www.loc.gov/standards/iso639-2/php/code_list.php
        /// </summary>
        public string ISOCode2 { get; private set; }

        #endregion

        #region ISOCode2B

        /// <summary>
        /// ISO 639-2/B: three-letter codes, mostly the same as 639-2/T, but with some codes derived from English names rather than native names of languages
        /// </summary>
        public string ISOCode2B { get; private set; }

        #endregion

        #region ISOCode3

        /// <summary>
        /// ISO 639-3: three-letter codes, the same as 639-2/T for languages, but with distinct codes for each variety of an ISO 639 macrolanguage
        /// </summary>
        public string ISOCode3 { get; private set; }

        #endregion

        #region Name

        /// <summary>
        /// English name of language
        /// </summary>
        public string Name { get; private set; }


        #endregion

        #region NativeName

        public string NativeName { get; private set; }

        #endregion

        #endregion

        #region Method

        public override string ToString()
        {
            return ISOCode;
        }

        #endregion

        #region Static Instances

        #region All

        public static IEnumerable<Language> All
        {
            get
            {
                yield return Unknown;

                yield return English;
                yield return Abkhazian;
                yield return Adyghe;
                yield return Afar;
                yield return Afrikaans;
                yield return Akan;
                yield return Albanian;
                yield return AmericanSignLanguage;
                yield return Amharic;
                yield return Arabic;
                yield return Aragonese;
                yield return Aramaic;
                yield return Armenian;
                yield return Assamese;
                yield return Aymara;
                yield return Balinese;
                yield return Basque;
                yield return Bengali;
                yield return Betawi;
                yield return Bosnian;
                yield return Breton;
                yield return Bulgarian;
                yield return Cantonese;
                yield return Catalan;
                yield return Cherokee;
                yield return Chickasaw;
                yield return Chinese;
                yield return Coptic;
                yield return Cornish;
                yield return Corsican;
                yield return CrimeanTatar;
                yield return Croatian;
                yield return Czech;
                yield return Danish;
                yield return Dutch;
                yield return Dawro;
                yield return Esperanto;
                yield return Estonian;
                yield return Ewe;
                yield return Fijian;
                yield return FijiHindi;
                yield return Filipino;
                yield return Finnish;
                yield return French;
                yield return Galician;
                yield return Georgian;
                yield return German;
                yield return GreekModern;
                yield return AncientGreek;
                yield return Greenlandic;
                yield return HaitianCreole;
                yield return Hawaiian;
                yield return Hebrew;
                yield return Hindi;
                yield return Hungarian;
                yield return Icelandic;
                yield return Indonesian;
                yield return Inuktitut;
                yield return Interlingua;
                yield return Irish;
                yield return Italian;
                yield return Japanese;
                yield return Javanese;
                yield return Kabardian;
                yield return Kalasha;
                yield return Kannada;
                yield return Kashubian;
                yield return Khmer;
                yield return Kinyarwanda;
                yield return Korean;
                yield return Kurdish;
                yield return Ladin;
                yield return Latgalian;
                yield return Latin;
                yield return Lingala;
                yield return Livonian;
                yield return Lojban;
                yield return LowerSorbian;
                yield return LowGerman;
                yield return Macedonian;
                yield return Malay;
                yield return Malayalam;
                yield return Mandarin;
                yield return Manx;
                yield return Maori;
                yield return MauritianCreole;
                yield return MiddleEnglish;
                yield return MiddleLowGerman;
                yield return MinNan;
                yield return Mongolian;
                yield return Norwegian;
                yield return Occitan;
                yield return OldArmenian;
                yield return OldEnglish;
                yield return OldFrench;
                yield return OldJavanese;
                yield return OldNorse;
                yield return OldPrussian;
                yield return Oriya;
                yield return Pangasinan;
                yield return Papiamentu;
                yield return Pashto;
                yield return Persian;
                yield return Pitjantjatjara;
                yield return Polish;
                yield return Portuguese;
                yield return ProtoSlavic;
                yield return Quenya;
                yield return Rajasthani;
                yield return RapaNui;
                yield return Romanian;
                yield return Russian;
                yield return Sanskrit;
                yield return Scots;
                yield return ScottishGaelic;
                yield return Semai;
                yield return Serbian;
                yield return SerboCroatian;
                yield return Slovak;
                yield return Slovene;
                yield return Spanish;
                yield return Sinhalese;
                yield return Swahili;
                yield return Swedish;
                yield return Tagalog;
                yield return Tajik;
                yield return Tamil;
                yield return Tarantino;
                yield return Tatar;
                yield return Telugu;
                yield return Thai;
                yield return TokPisin;
                yield return Ukrainian;
                yield return UpperSorbian;
                yield return Urdu;
                yield return Uyghur;
                yield return Uzbek;
                yield return Venetian;
                yield return Vietnamese;
                yield return Vilamovian;
                yield return Volapuk;
                yield return Voro;
                yield return Welsh;
                yield return Xhosa;
                yield return Yiddish;
                yield return Zahaki;
                yield return Zulu;
            }
        }


        #endregion

        public static Language Unknown { get; } = new Language()
        {
            ISOCode = "",
            ISOCode2 = "",
            ISOCode2B = "",
            ISOCode3 = "",
            Name = "Unknown",
            NativeName = "Unknown"
        };

        public static Language English { get; } = new Language()
        {
            ISOCode = "en",
            ISOCode2 = "eng",
            ISOCode2B = "eng",
            ISOCode3 = "eng",
            Name = "English"
        };

        public static Language Abkhazian { get; } = new Language()
        {
            ISOCode = "ab",
            ISOCode2 = "abk",
            Name = "Abkhazian",
            NativeName = "аҧсуа бызшәа"
        };

        public static Language Adyghe { get; } = new Language()
        {
            ISOCode = "ady",
            Name = "Adyghe"
        };

        public static Language Afar { get; } = new Language()
        {
            ISOCode = "aa",
            ISOCode2 = "aar",
            Name = "Afar"
        };

        public static Language Afrikaans { get; } = new Language()
        {
            ISOCode = "af",
            Name = "Afrikaans",
            LanguageIdentifier = 0x0436
        };

        public static Language Akan { get; } = new Language()
        {
            ISOCode = "ak",
            Name = "Akan"
        };

        public static Language Albanian { get; } = new Language()
        {
            ISOCode = "sq",
            Name = "Albanian",
            LanguageIdentifier = 0x041C
        };

        public static Language AmericanSignLanguage { get; } = new Language()
        {
            ISOCode = "ase",
            Name = "American Sign Language",
            NativeName = "American Sign Language"
        };

        public static Language Amharic { get; } = new Language()
        {
            ISOCode = "am",
            Name = "",
            LanguageIdentifier = 0x045E
        };

        public static Language Arabic { get; } = new Language()
        {
            ISOCode = "ar",
            Name = "Arabic"
        };

        public static Language Aragonese { get; } = new Language()
        {
            ISOCode = "an",
            Name = "Aragonese"
        };

        public static Language Aramaic { get; } = new Language()
        {
            ISOCode = "arc",
            Name = "Aramaic"
        };

        public static Language Armenian { get; } = new Language()
        {
            ISOCode = "hy",
            Name = "Armenian"
        };

        public static Language Assamese { get; } = new Language()
        {
            ISOCode = "as",
            Name = "Assamese"
        };

        public static Language Aymara { get; } = new Language()
        {
            ISOCode = "ay",
            Name = "Aymara"
        };

        public static Language Balinese { get; } = new Language()
        {
            ISOCode = "ban",
            Name = "Balinese"
        };

        public static Language Basque { get; } = new Language()
        {
            ISOCode = "eu",
            Name = "Basque"
        };

        public static Language Bengali { get; } = new Language()
        {
            ISOCode = "bn",
            ISOCode2 = "ben",
            ISOCode3 = "ben",
            Name = "Bengali"
        };

        public static Language Betawi { get; } = new Language()
        {
            ISOCode = "bew",
            Name = "Betawi"
        };

        public static Language Bosnian { get; } = new Language()
        {
            ISOCode = "sh",
            Name = "Bosnian"
        };

        public static Language Breton { get; } = new Language()
        {
            ISOCode = "br",
            Name = "Breton"
        };

        public static Language Bulgarian { get; } = new Language()
        {
            ISOCode = "bg",
            Name = "Bulgarian"
        };

        public static Language Cantonese { get; } = new Language()
        {
            ISOCode = "yue",
            Name = "Cantonese"
        };

        public static Language Catalan { get; } = new Language()
        {
            ISOCode = "ca",
            Name = "Catalan"
        };

        public static Language Cherokee { get; } = new Language()
        {
            ISOCode = "chr",
            Name = "Cherokee"
        };

        public static Language Chickasaw { get; } = new Language()
        {
            ISOCode = "clc",
            Name = "Chickasaw"
        };

        public static Language Chinese { get; } = new Language()
        {
            ISOCode = "zh",
            Name = "Chinese"
        };

        public static Language Coptic { get; } = new Language()
        {
            ISOCode = "cop",
            Name = "Coptic"
        };

        public static Language Cornish { get; } = new Language()
        {
            ISOCode = "kw",
            Name = "Cornish"
        };

        public static Language Corsican { get; } = new Language()
        {
            ISOCode = "co",
            Name = "Corsican"
        };

        public static Language CrimeanTatar { get; } = new Language()
        {
            ISOCode = "",
            ISOCode2 = "crh",
            ISOCode2B = "crh",
            ISOCode3 = "crh",
            Name = "Crimean Tatar" //Krimtartarisch
        };

        public static Language Croatian { get; } = new Language()
        {
            ISOCode = "sh",
            Name = "Croatian"
        };

        public static Language Czech { get; } = new Language()
        {
            ISOCode = "cs",
            Name = "Czech"
        };

        public static Language Danish { get; } = new Language()
        {
            ISOCode = "da",
            Name = "Danish"
        };

        public static Language Dutch { get; } = new Language()
        {
            ISOCode = "nl",
            Name = "Dutch"
        };

        public static Language Dawro { get; } = new Language()
        {
            ISOCode = "dwr",
            Name = "Dawro"
        };

        public static Language Esperanto { get; } = new Language()
        {
            ISOCode = "eo",
            Name = "Esperanto"
        };

        public static Language Estonian { get; } = new Language()
        {
            ISOCode = "et",
            Name = "Estonian"
        };

        public static Language Ewe { get; } = new Language()
        {
            ISOCode = "ee",
            Name = "Ewe"
        };

        public static Language Fijian { get; } = new Language()
        {
            ISOCode = "fj",
            ISOCode2 = "fij",
            Name = "Fijian",
            NativeName = "vosa Vakaviti"
        };

        public static Language FijiHindi { get; } = new Language()
        {
            ISOCode = "hif",
            Name = "Fiji Hindi"
        };

        public static Language Filipino { get; } = new Language()
        {
            ISOCode = "fil",
            Name = "Filipino"
        };

        public static Language Finnish { get; } = new Language()
        {
            ISOCode = "fi",
            Name = "Finnish"
        };

        public static Language French { get; } = new Language()
        {
            ISOCode = "fr",
            ISOCode2 = "fra",
            Name = "French",
            NativeName = "française"
        };

        public static Language Galician { get; } = new Language()
        {
            ISOCode = "gl",
            Name = "Galician"
        };

        public static Language Georgian { get; } = new Language()
        {
            ISOCode = "ka",
            Name = "Georgian"
        };

        public static Language German { get; } = new Language()
        {
            ISOCode = "de",
            Name = "German",
            NativeName = "Deutsch"
        };

        public static Language GreekModern { get; } = new Language()
        {
            ISOCode = "el",
            ISOCode2 = "ell",
            Name = "Greek, Modern",
            NativeName = "ελληνικά"
        };

        public static Language AncientGreek { get; } = new Language()
        {
            ISOCode = "grc",
            Name = "Ancient Greek"
        };

        public static Language Greenlandic { get; } = new Language()
        {
            ISOCode = "	kl",
            Name = "Greenlandic"
        };

        public static Language HaitianCreole { get; } = new Language()
        {
            ISOCode = "ht",
            Name = "Haitian Creole"
        };

        public static Language Hawaiian { get; } = new Language()
        {
            ISOCode = "haw",
            Name = "Hawaiian"
        };

        public static Language Hebrew { get; } = new Language()
        {
            ISOCode = "he",
            Name = "Hebrew"
        };

        public static Language Hindi { get; } = new Language()
        {
            ISOCode = "hi",
            Name = "Hindi"
        };

        public static Language Hungarian { get; } = new Language()
        {
            ISOCode = "hu",
            Name = "Hungarian"
        };

        public static Language Icelandic { get; } = new Language()
        {
            ISOCode = "is",
            Name = "Icelandic"
        };

        public static Language Indonesian { get; } = new Language()
        {
            ISOCode = "id",
            Name = "Indonesian"
        };

        public static Language Inuktitut { get; } = new Language()
        {
            ISOCode = "iu",
            Name = "Inuktitut"
        };

        public static Language Interlingua { get; } = new Language()
        {
            ISOCode = "ia",
            Name = "Interlingua"
        };

        public static Language Irish { get; } = new Language()
        {
            ISOCode = "ga",
            Name = "Irish",
            LanguageIdentifier = 0x083C
        };

        public static Language Italian { get; } = new Language()
        {
            ISOCode = "it",
            Name = "Italian"
        };


        public static Language Japanese { get; } = new Language()
        {
            ISOCode = "ja",
            Name = "Japanese"
        };

        public static Language Javanese { get; } = new Language()
        {
            ISOCode = "jv",
            Name = "Javanese"
        };

        public static Language Kabardian { get; } = new Language()
        {
            ISOCode = "kbd",
            Name = "Kabardian"
        };

        public static Language Kalasha { get; } = new Language()
        {
            ISOCode = "kls",
            Name = "Kalasha"
        };

        public static Language Kannada { get; } = new Language()
        {
            ISOCode = "kn",
            Name = "Kannada"
        };

        public static Language Kashubian { get; } = new Language()
        {
            ISOCode = "csb",
            Name = "Kashubian"
        };

        public static Language Khmer { get; } = new Language()
        {
            ISOCode = "km",
            Name = "Khmer"
        };

        public static Language Kinyarwanda { get; } = new Language()
        {
            ISOCode = "rw",
            Name = "Kinyarwanda"
        };

        public static Language Korean { get; } = new Language()
        {
            ISOCode = "ko",
            Name = "Korean"
        };

        public static Language Kurdish { get; } = new Language()
        {
            ISOCode = "ku",
            Name = "Kurdish", //also: Kurdî
            NativeName = "کوردی"
        };

        public static Language Ladin { get; } = new Language()
        {
            ISOCode = "lld",
            Name = "Ladin"
        };

        public static Language Latgalian { get; } = new Language()
        {
            ISOCode = "ltg",
            Name = "Latgalian"
        };

        public static Language Latin { get; } = new Language()
        {
            ISOCode = "la",
            Name = "Latin"
        };

        public static Language Lingala { get; } = new Language()
        {
            ISOCode = "ln",
            Name = "Lingala"
        };

        public static Language Livonian { get; } = new Language()
        {
            ISOCode = "liv",
            Name = "Livonian"
        };

        public static Language Lojban { get; } = new Language()
        {
            ISOCode = "jbo",
            Name = "Lojban"
        };

        public static Language LowerSorbian { get; } = new Language()
        {
            ISOCode = "dsb",
            Name = "dsb"
        };

        public static Language LowGerman { get; } = new Language()
        {
            ISOCode = "nds-de",
            Name = "Low German",
            NativeName = "Niederdeutsch"
        };

        public static Language Macedonian { get; } = new Language()
        {
            ISOCode = "mk",
            Name = "Macedonian"
        };

        public static Language Malay { get; } = new Language()
        {
            ISOCode = "ms",
            Name = "Malay"
        };

        public static Language Malayalam { get; } = new Language()
        {
            ISOCode = "ml",
            Name = "Malayalam"
        };

        public static Language Mandarin { get; } = new Language()
        {
            ISOCode = "cmn",
            Name = "Mandarin"
        };

        public static Language Manx { get; } = new Language()
        {
            ISOCode = "gv",
            Name = "Manx"
        };

        public static Language Maori { get; } = new Language()
        {
            ISOCode = "mi",
            Name = "Maori"
        };

        public static Language MauritianCreole { get; } = new Language()
        {
            ISOCode = "mfe",
            Name = "Mauritian Creole"
        };

        public static Language MiddleEnglish { get; } = new Language()
        {
            ISOCode = "enm",
            Name = "Middle English"
        };

        public static Language MiddleLowGerman { get; } = new Language()
        {
            ISOCode = "gml",
            Name = "Middle Low German",
            NativeName = "Mittelniederdeutsch"
        };

        public static Language MinNan { get; } = new Language()
        {
            ISOCode = "nan",
            Name = "Min Nan"
        };

        public static Language Mongolian { get; } = new Language()
        {
            ISOCode = "mn",
            Name = "Mongolian"
        };

        public static Language Norwegian { get; } = new Language()
        {
            ISOCode = "no",
            Name = "Norwegian"
        };

        public static Language Occitan { get; } = new Language()
        {
            ISOCode = "oc",
            ISOCode2 = "oci",
            Name = "Occitan",
            LanguageIdentifier = 0
        };

        public static Language OldArmenian { get; } = new Language()
        {
            ISOCode = "xcl",
            Name = "Old Armenian"
        };

        public static Language OldEnglish { get; } = new Language()
        {
            ISOCode = "ang",
            Name = "Old English"
        };

        public static Language OldFrench { get; } = new Language()
        {
            ISOCode = "fro",
            Name = "Old French"
        };

        public static Language OldJavanese { get; } = new Language()
        {
            ISOCode = "kaw",
            Name = "Old Javanese"
        };

        public static Language OldNorse { get; } = new Language()
        {
            ISOCode = "non",
            Name = "Old Norse"
        };

        public static Language OldPrussian { get; } = new Language()
        {
            ISOCode = "prg",
            Name = "Old Prussian"
        };

        public static Language Oriya { get; } = new Language()
        {
            ISOCode = "or",
            Name = "Oriya"
        };

        public static Language Pangasinan { get; } = new Language()
        {
            ISOCode = "pag",
            Name = "Pangasian"
        };

        public static Language Panjabi { get; } = new Language()
        {
            ISOCode = "pa",
            ISOCode2 = "pan",
            Name = "Panjabi" //synonym>: "Punjabi"
        };

        public static Language Papiamentu { get; } = new Language()
        {
            ISOCode = "pap",
            Name = "Papiamentu"
        };

        public static Language Pashto { get; } = new Language()
        {
            ISOCode = "ps",
            Name = "Pashto"
        };

        public static Language Persian { get; } = new Language()
        {
            ISOCode = "fa",
            Name = "Persian"
        };

        public static Language Pitjantjatjara { get; } = new Language()
        {
            ISOCode = "pjt",
            Name = "Pitjantjatjara"
        };

        public static Language Polish { get; } = new Language()
        {
            ISOCode = "pl",
            Name = "Polish"
        };

        public static Language Portuguese { get; } = new Language()
        {
            ISOCode = "pt",
            Name = "Portuguese"
        };

        public static Language ProtoSlavic { get; } = new Language()
        {
            ISOCode = "sla-pro",
            Name = "Proto-Slavic"
        };

        public static Language Quenya { get; } = new Language()
        {
            ISOCode = "qya",
            Name = "Quenya"
        };

        public static Language Rajasthani { get; } = new Language()
        {
            ISOCode = "raj",
            Name = "Rajasthani"
        };

        public static Language RapaNui { get; } = new Language()
        {
            ISOCode = "rap",
            Name = "Rapa Nui"
        };

        public static Language Romanian { get; } = new Language()
        {
            ISOCode = "ro",
            Name = "Romanian"
        };

        public static Language Russian { get; } = new Language()
        {
            ISOCode = "ru",
            Name = "Russian"
        };

        public static Language Sanskrit { get; } = new Language()
        {
            ISOCode = "sa",
            Name = "Sanskrit"
        };

        public static Language Scots { get; } = new Language()
        {
            ISOCode = "sco",
            Name = "Scots"
        };

        public static Language ScottishGaelic { get; } = new Language()
        {
            ISOCode = "gd",
            Name = "Scottish Gaelic"
        };

        public static Language Semai { get; } = new Language()
        {
            ISOCode = "sea",
            Name = "Semai"
        };

        public static Language Serbian { get; } = new Language()
        {
            ISOCode = "sh",
            Name = "Serbian"
        };

        public static Language SerboCroatian { get; } = new Language()
        {
            ISOCode = "sh",
            Name = "Serbo-Croatian"
        };

        public static Language Slovak { get; } = new Language()
        {
            ISOCode = "sk",
            Name = "Slovak"
        };

        public static Language Slovene { get; } = new Language()
        {
            ISOCode = "sl",
            Name = "Slovene"
        };

        public static Language Spanish { get; } = new Language()
        {
            ISOCode = "es",
            Name = "Spanish"
        };

        public static Language Sinhalese { get; } = new Language()
        {
            ISOCode = "si",
            Name = "Sinhalese"
        };

        public static Language Swahili { get; } = new Language()
        {
            ISOCode = "sw",
            Name = "Swahili"
        };

        public static Language Swedish { get; } = new Language()
        {
            ISOCode = "se",
            Name = "Swedish"
        };

        public static Language Tagalog { get; } = new Language()
        {
            ISOCode = "ti",
            Name = "Tagalog"
        };

        public static Language Tajik { get; } = new Language()
        {
            ISOCode = "tg",
            Name = "Tajik"
        };

        public static Language Tamil { get; } = new Language()
        {
            ISOCode = "ta",
            Name = "Tamil"
        };

        public static Language Tarantino { get; } = new Language()
        {
            ISOCode = "roa-tar",
            Name = "Tarantino"
        };

        public static Language Tatar { get; } = new Language()
        {
            ISOCode = "tt",
            ISOCode2 = "tat",
            ISOCode2B = "tat",
            ISOCode3 = "tat",
            Name = "Tatar",
            NativeName = "татар теле"
        };

        public static Language Telugu { get; } = new Language()
        {
            ISOCode = "te",
            Name = "Telugu"
        };

        public static Language Thai { get; } = new Language()
        {
            ISOCode = "th",
            Name = "Thai"
        };

        public static Language TokPisin { get; } = new Language()
        {
            ISOCode = "tpi",
            Name = "Tok Pisin"
        };

        public static Language Ukrainian { get; } = new Language()
        {
            ISOCode = "uk",
            Name = "Ukrainian"
        };

        public static Language UpperSorbian { get; } = new Language()
        {
            ISOCode = "hsb",
            Name = "Upper Sorbian"
        };

        public static Language Urdu { get; } = new Language()
        {
            ISOCode = "ur",
            Name = "Urdu"
        };

        public static Language Uyghur { get; } = new Language()
        {
            ISOCode = "ug",
            Name = "Uyghur"
        };

        public static Language Uzbek { get; } = new Language()
        {
            ISOCode = "uz",
            Name = "Uzbek"
        };

        public static Language Venetian { get; } = new Language()
        {
            ISOCode = "vec",
            Name = "Venetian"
        };

        public static Language Vietnamese { get; } = new Language()
        {
            ISOCode = "vi",
            Name = "Vietnamese"
        };

        public static Language Vilamovian { get; } = new Language()
        {
            ISOCode = "wym",
            Name = "Vilamovian"
        };

        public static Language Volapuk { get; } = new Language()
        {
            ISOCode = "vo",
            Name = "Volapük"
        };

        public static Language Voro { get; } = new Language()
        {
            ISOCode = "vro",
            Name = "Võro"
        };

        public static Language Welsh { get; } = new Language()
        {
            ISOCode = "cy",
            Name = "Welsh"
        };

        public static Language Xhosa { get; } = new Language()
        {
            ISOCode = "xh",
            Name = "Xhosa"
        };

        public static Language Yiddish { get; } = new Language()
        {
            ISOCode = "yi",
            Name = "Yiddish"
        };

        public static Language Zahaki { get; } = new Language()
        {
            ISOCode = "zza",
            Name = "Zazaki"
        };

        //https://www.loc.gov/standards/iso639-2/php/langcodes_name.php?iso_639_1=zu
        public static Language Zulu { get; } = new Language()
        {
            ISOCode = "zu",
            Name = "Zulu"
        };


        #endregion
    }
}
