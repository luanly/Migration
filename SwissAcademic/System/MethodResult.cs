namespace SwissAcademic
{
    public class MethodResult
    {
        #region Constructors

        private MethodResult(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }

        #endregion

        public bool IsSuccess { get; private set; }

        static MethodResult _success = new MethodResult(true);
        public static MethodResult Success
        {
            get { return _success; }
        }

        static MethodResult _noSuccess = new MethodResult(false);
        public static MethodResult NoSuccess
        {
            get { return _noSuccess; }
        }
    }

    public class CancelableMethodResult
    {
        #region Constructors

        private CancelableMethodResult(bool isCanceled)
        {
            IsCanceled = isCanceled;
        }

        #endregion

        public bool IsCanceled { get; }

        static CancelableMethodResult _canceled = new CancelableMethodResult(true);
        public static CancelableMethodResult Canceled => _canceled;

        static CancelableMethodResult _notCanceled = new CancelableMethodResult(false);
        public static CancelableMethodResult NotCanceled => _notCanceled;

    }
}
