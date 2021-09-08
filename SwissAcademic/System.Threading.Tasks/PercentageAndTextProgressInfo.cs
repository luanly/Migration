using SwissAcademic;
using System.Collections.Generic;

namespace System.Threading.Tasks
{
    public class PercentageAndTextProgressInfo
    {
        public int Percentage { get; set; }

        public Enum Status { get; set; }

        string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public object Tag { get; set; }
    }

    public static class ProgressExtensions
    {
        public static void ReportSafe<T>(this IProgress<T> progress, T value)
        {
            if (progress == null) return;
            progress.Report(value);
        }

        public static void ReportSafe(this IProgress<int> progress, long value, long total)
        {
            if (progress == null) return;
            var percentage = Convert.ToInt32(value * 100 / total);
            ReportSafe(progress, percentage);
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, int percentage)
        {
            ReportSafe(progress, string.Empty, percentage);
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, long value, long total)
        {
            if (progress == null) return;
            var percentage = Convert.ToInt32(value * 100 / total);
            ReportSafe(progress, string.Empty, percentage);
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, string text)
        {
            if (progress == null) return;
            progress.Report(new PercentageAndTextProgressInfo { Text = text });
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, string text, int percentage)
        {
            if (progress == null) return;
            progress.Report(new PercentageAndTextProgressInfo { Text = text, Percentage = percentage });
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, string text, int percentage, object tag)
        {
            if (progress == null) return;
            progress.Report(new PercentageAndTextProgressInfo { Text = text, Percentage = percentage, Tag = tag });
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, string text, int percentage, Enum status, object tag)
        {
            if (progress == null) return;
            progress.Report(new PercentageAndTextProgressInfo { Text = text, Percentage = percentage, Status = status, Tag = tag });
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, Enum status, string text)
        {
            if (progress == null) return;
            progress.Report(new PercentageAndTextProgressInfo { Text = text, Status = status });
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, Enum status, int percentage)
        {
            if (progress == null) return;
            progress.Report(new PercentageAndTextProgressInfo { Percentage = percentage, Status = status });
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, IDictionary<string, string> properties)
        {
            ReportSafe(progress, null, properties);
        }

        public static void ReportSafe(this IProgress<PercentageAndTextProgressInfo> progress, Enum status, IDictionary<string, string> properties)
        {
            if (progress == null) return;
            if (properties == null) ReportSafe(progress, string.Empty, 0);

            string progressText;
            if (!properties.TryGetValue(MessageKey.ProgressText, out progressText))
            {
                progressText = string.Empty;
            }

            string progressPercentageString;
            int progressPercentage;
            if (!properties.TryGetValue(MessageKey.ProgressPercentage, out progressPercentageString) ||
                !int.TryParse(progressPercentageString, out progressPercentage))
            {
                progressPercentage = 0;
            }

            progress.Report(new PercentageAndTextProgressInfo { Text = progressText, Percentage = progressPercentage, Status = status });
        }

        public static void ReportSafe(this IProgress<string> progress, IDictionary<string, string> properties)
        {
            if (properties == null) ReportSafe(progress, string.Empty);

            string progressText;
            if (!properties.TryGetValue(MessageKey.ProgressText, out progressText))
            {
                progressText = string.Empty;
            }

            ReportSafe(progress, progressText);
        }

        public static void ReportSafe(this IProgress<int> progress, IDictionary<string, string> properties)
        {
            if (properties == null) ReportSafe(progress, 0);

            string progressPercentageString;
            int progressPercentage;
            if (!properties.TryGetValue(MessageKey.ProgressPercentage, out progressPercentageString) ||
                !int.TryParse(progressPercentageString, out progressPercentage))
            {
                progressPercentage = 0;
            }

            ReportSafe(progress, progressPercentage);
        }
    }
}
