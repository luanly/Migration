namespace System.ComponentModel
{
    public static class BackgroundWorkerUtility
    {
        public static void ReportProgressSafe(this BackgroundWorker backgroundWorker, int value)
        {
            if (backgroundWorker == null || !backgroundWorker.WorkerReportsProgress) return;

            // No lower limit of 0, sometimes we use -1 as a status value
            value = Math.Min(100, value);
            backgroundWorker.ReportProgress(value);
        }

        public static void ReportProgressSafe(this BackgroundWorker backgroundWorker, int value, object userState)
        {
            if (backgroundWorker == null || !backgroundWorker.WorkerReportsProgress) return;

            // No lower limit of 0, sometimes we use -1 as a status value
            value = Math.Min(100, value);
            backgroundWorker.ReportProgress(value, userState);
        }
    }
}
