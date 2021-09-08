namespace System
{
    public static class LongUtility
    {
        public static double BytesToGigabytes(this long val)
        {
            return (double)val / 1024f / 1024f / 1024f;
        }
        public static double BytesToMegabytes(this long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public static double KilobytesToMegabytes(this long kilobytes)
        {
            return kilobytes / 1024f;
        }

        public static double MegabytesToGigabytes(this double megabytes)
        {
            return megabytes / 1024f;
        }

        public static long MegabytesToBytes(this double megabytes)
        {
            return (long)(megabytes * 1024) * 1024;
        }

        public static long MegabytesToBytes(this int megabytes)
        {
            return (long)(megabytes * 1024) * 1024;
        }

        public static int MegabytesToGigabyte(this int gigabytes)
        {
            return gigabytes / 1024;
        }

        public static long GigabytesToBytes(this int megabytes)
        {
            return (long)(megabytes * 1024) * 1024 * 1024;
        }
    }
}
