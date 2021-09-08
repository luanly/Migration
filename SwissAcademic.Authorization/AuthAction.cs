namespace SwissAcademic.Authorization
{
    public class AuthAction
    {
        #region Constructors

        protected AuthAction(string key, string value)
        {
            Key = key;
            Value = value;
        }

        #endregion

        #region Properties

        public string Key { get; private set; }
        public string Value { get; private set; }

        #endregion

    }
}
