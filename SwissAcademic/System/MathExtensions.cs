using System.Collections.Generic;
using System.Linq;

namespace System
{
    public static class MathExtensions
    {
        public static double Variance(this IEnumerable<int> values)
        {
            var mean = values.Average();

            var variance = (from value in values
                            select Math.Pow((value - mean), 2)).Sum();

            return variance / values.Count();
        }

        public static double Variance(this IEnumerable<float> values)
        {
            var mean = values.Average();

            var variance = (from value in values
                            select Math.Pow((value - mean), 2)).Sum();

            return variance / values.Count();
        }

        public static double Variance(this IEnumerable<double> values)
        {
            var mean = values.Average();

            var variance = (from value in values
                            select Math.Pow((value - mean), 2)).Sum();

            return variance / values.Count();
        }

        public static double StandardDeviation(this IEnumerable<int> values)
        {
            return Math.Sqrt(Variance(values));
        }

        public static double StandardDeviation(this IEnumerable<float> values)
        {
            return Math.Sqrt(Variance(values));
        }

        public static double StandardDeviation(this IEnumerable<double> values)
        {
            return Math.Sqrt(Variance(values));
        }

        public static double Median(this IEnumerable<int> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.OrderBy(n => n).ToArray();
            if (data.Length == 0)
                throw new InvalidOperationException();
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
            return data[data.Length / 2];
        }

        public static double? Median(this IEnumerable<int?> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
            if (data.Length == 0)
                return null;
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
            return data[data.Length / 2];
        }

        public static double Median(this IEnumerable<long> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.OrderBy(n => n).ToArray();
            if (data.Length == 0)
                throw new InvalidOperationException();
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
            return data[data.Length / 2];
        }

        public static double? Median(this IEnumerable<long?> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
            if (data.Length == 0)
                return null;
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
            return data[data.Length / 2];
        }

        public static float Median(this IEnumerable<float> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.OrderBy(n => n).ToArray();
            if (data.Length == 0)
                throw new InvalidOperationException();
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0f;
            return data[data.Length / 2];
        }

        public static float? Median(this IEnumerable<float?> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
            if (data.Length == 0)
                return null;
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0f;
            return data[data.Length / 2];
        }

        public static double Median(this IEnumerable<double> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.OrderBy(n => n).ToArray();
            if (data.Length == 0)
                throw new InvalidOperationException();
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
            return data[data.Length / 2];
        }

        public static double? Median(this IEnumerable<double?> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
            if (data.Length == 0)
                return null;
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
            return data[data.Length / 2];
        }

        public static decimal Median(this IEnumerable<decimal> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.OrderBy(n => n).ToArray();
            if (data.Length == 0)
                throw new InvalidOperationException();
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0m;
            return data[data.Length / 2];
        }

        public static decimal? Median(this IEnumerable<decimal?> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
            if (data.Length == 0)
                return null;
            if (data.Length % 2 == 0)
                return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0m;
            return data[data.Length / 2];
        }
    }
}
