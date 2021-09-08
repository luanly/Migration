using Microsoft.WindowsAPICodePack.ExtendedLinguisticServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Globalization
{
    #region LinguisticServices

    public static class LinguisticServices
    {
        #region AnalyseDetectCultureResult

        static void AnalyseDetectCultureResult(IAsyncResult asyncResult, MappingRecognizeAsyncResult detectionResult, TaskCompletionSource<IEnumerable<CultureInfo>> tsc)
        {
            if (detectionResult == null)
            {
                tsc.SetResult(new List<CultureInfo>());
                return;
            }

            if (detectionResult.Succeeded)
            {
                var keys = FindLanguageKeys(detectionResult);

                if (!keys.Any())
                {
                    tsc.SetResult(new List<CultureInfo>());
                    return;
                }

#if DEBUG
                //http://tfs2012:8080/tfs/CITAVICollection/Citavi/_workitems/edit/13953
                keys = keys.Where(key => key != "Brai" && key != "Ogam");
#endif

                //var cultureInfos = keys.Select(key => new CultureInfo(key)).ToList();
                var cultureInfos = keys.Select(key =>
                {
                    CultureInfo ci = null;
                    if (CultureInfoUtility.TryGetCultureInfo(key, out ci)) return ci;
                    return null;

                }).Where(ci => ci != null).ToList();

                tsc.SetResult(cultureInfos);
                return;
            }

            tsc.SetException(new Exception(detectionResult.ResultState.ErrorMessage));
        }

        #endregion

        #region AnalyseDetectScriptResult

        static void AnalyseDetectScriptResult(IAsyncResult asyncResult, MappingRecognizeAsyncResult detectionResult, TaskCompletionSource<IEnumerable<ScriptIdentifier>> tsc)
        {
            if (detectionResult == null)
            {
                tsc.SetResult(new List<ScriptIdentifier>());
                return;
            }

            if (detectionResult.Succeeded)
            {
                var keys = FindScriptKeys(detectionResult);

                var extentedKeys = keys.ToList();
                extentedKeys.RemoveAll(key => key.Length == 0);

                tsc.SetResult(extentedKeys);
                return;
            }

            tsc.SetException(new Exception(detectionResult.ResultState.ErrorMessage));
        }

        #endregion

        #region DetectCulture

        public static Task<IEnumerable<CultureInfo>> DetectCulture(string analyseText)
        {
            var tsc = new TaskCompletionSource<IEnumerable<CultureInfo>>();

            try
            {
                var languageDetectionService = new MappingService(MappingAvailableServices.LanguageDetection);

                MappingRecognizeAsyncResult detectionResult = null;

                detectionResult = languageDetectionService.BeginRecognizeText(
                            analyseText,
                            new MappingOptions(),
                            new AsyncCallback((asyncResult) => AnalyseDetectCultureResult(asyncResult, detectionResult, tsc)),
                            null);

            }
            catch (Exception e)
            {
                tsc.SetException(e);
            }

            return tsc.Task;
        }

        #endregion

        #region DetectScript

        public static IEnumerable<ScriptIdentifier> DetectScript(string analyseText)
        {
            var languageDetectionService = new MappingService(MappingAvailableServices.ScriptDetection);

            var propertyBag = languageDetectionService.RecognizeText(analyseText, new MappingOptions());

            var keys = FindScriptKeys(propertyBag).ToList();
            keys.RemoveAll(key => key.Length == 0);

            return keys;
        }

        public static Task<IEnumerable<ScriptIdentifier>> DetectScriptAsync(string analyseText)
        {
            var tsc = new TaskCompletionSource<IEnumerable<ScriptIdentifier>>();

            try
            {
                var languageDetectionService = new MappingService(MappingAvailableServices.ScriptDetection);

                MappingRecognizeAsyncResult detectionResult = null;
                detectionResult = languageDetectionService.BeginRecognizeText(
                            analyseText,
                            new MappingOptions(),
                            new AsyncCallback((asyncResult) => AnalyseDetectScriptResult(asyncResult, detectionResult, tsc)),
                            null);

            }
            catch (Exception e)
            {
                tsc.SetException(e);
            }

            return tsc.Task;
        }

        #endregion

        #region FindLanguageKeys

        static IEnumerable<string> FindLanguageKeys(MappingRecognizeAsyncResult detectionResult)
        {
            if (detectionResult.PropertyBag == null) return null;

            var mappings = detectionResult.PropertyBag.FormatData(new LanguageFormatter());

            return mappings.SelectMany(mapping => mapping.Split(new string[] { "/", "\\", "\0" }, StringSplitOptions.RemoveEmptyEntries)).ToList();
        }

        #endregion

        #region FindScriptKeys

        static IEnumerable<ScriptIdentifier> FindScriptKeys(MappingRecognizeAsyncResult detectionResult)
        {
            if (detectionResult.PropertyBag == null) return null;

            var mappings = detectionResult.PropertyBag.FormatData(new ScriptFormatter());

            if (!mappings.Any()) return new List<ScriptIdentifier>();

            return mappings.ToList();
        }

        static IEnumerable<ScriptIdentifier> FindScriptKeys(MappingPropertyBag propertyBag)
        {
            if (propertyBag == null) return null;
            var mappings = propertyBag.FormatData(new ScriptFormatter());
            if (!mappings.Any()) return new List<ScriptIdentifier>();
            return mappings.ToList();
        }

        #endregion
    }

    #endregion

    #region ScriptIdentifier

    public class ScriptIdentifier
    {
        #region Constructors

        public ScriptIdentifier(string key, int start, int end)
        {
            Key = key;
            StartIndex = start;
            EndIndex = end;
        }

        #endregion

        #region Properties

        public string Key { get; private set; }

        public int StartIndex { get; private set; }

        public int EndIndex { get; private set; }

        public int Length { get { return EndIndex - StartIndex; } }

        #endregion
    }

    #endregion

    #region ScriptFormatter

    public class ScriptFormatter : IMappingFormatter<ScriptIdentifier>
    {
        #region Format

        public ScriptIdentifier Format(MappingDataRange dataRange)
        {
            if (dataRange == null)
                throw new ArgumentNullException("dataRange");

            var key = Encoding.Unicode.GetString(dataRange.GetData());

            key = key.Replace("\0", string.Empty);

            return new ScriptIdentifier(key, dataRange.StartIndex, dataRange.EndIndex);
        }

        #endregion

        #region FormatAll

        public ScriptIdentifier[] FormatAll(MappingPropertyBag bag)
        {
            if (bag == null)
                throw new ArgumentNullException("bag");

            return (from dataRange in bag.GetResultRanges()
                    select Format(dataRange)).ToArray();
        }

        #endregion
    }

    #endregion

    #region LanguageFormatter

    public class LanguageFormatter : IMappingFormatter<string>
    {
        #region Format

        public string Format(MappingDataRange dataRange)
        {
            if (dataRange == null)
                throw new ArgumentNullException("dataRange");

            return Encoding.Unicode.GetString(dataRange.GetData());
        }

        #endregion

        #region FormatAll

        public string[] FormatAll(MappingPropertyBag bag)
        {
            if (bag == null)
                throw new ArgumentNullException("bag");

            return (from dataRange in bag.GetResultRanges()
                    select Format(dataRange)).ToArray();
        }

        #endregion
    }

    #endregion
}
