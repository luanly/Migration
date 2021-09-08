namespace SwissAcademic.Crm.Web
{
    public abstract class ExpressionBase
    {
        #region Konstanten

        public const int MaxResultsCount = 5000;

        #endregion

        #region Eigenschaften

        public bool HasMoreResults => !string.IsNullOrEmpty(NextLink);

        /// <summary>
        /// odata.maxpagesize
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// odata.nextLink
        /// </summary>
        public string NextLink { get; set; }

        public virtual string EntityName { get; }

        #endregion

        #region Methoden

        /// <summary>
        /// Reset von NextLink
        /// </summary>
        public virtual void Reset()
        {
            NextLink = null;
        }

        public abstract string ToOData();

        #endregion
    }
}
