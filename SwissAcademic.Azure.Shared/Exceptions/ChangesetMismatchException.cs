using System;

namespace SwissAcademic.Azure
{
    public class ChangesetMismatchException
        :
        ApplicationException
    {
        #region Konstruktoren

        public ChangesetMismatchException()
            :
            base("Changeset mismatch")
        { }

        public ChangesetMismatchException(string message)
            :
            base(message)
        { }

        public ChangesetMismatchException(string message, Exception innerException)
            :
            base(message, innerException)
        { }

        #endregion
    }
}
