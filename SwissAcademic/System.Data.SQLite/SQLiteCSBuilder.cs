using System.Data.SqlClient;

namespace System.Data.SQLite
{
    public class SQLiteCSBuilder
    {
        #region Fields

        SQLiteConnectionStringBuilder _builder = new SQLiteConnectionStringBuilder();

        #endregion

        #region Constructors

        public SQLiteCSBuilder()
        {
        }

        /// <summary>
        /// Parses the connection string. The connectionString argument may be a file path or a full connection string.
        /// </summary>
        /// <param name="connectionString">A file path or a full connection string.</param>
        public SQLiteCSBuilder(string connectionString)
        {
            Parse(connectionString);
        }

        #endregion

        #region Properties

        public string BaseSchemaName
        {
            get { return _builder.BaseSchemaName; }
            set { _builder.BaseSchemaName = value; }
        }

        public bool BinaryGUID
        {
            get { return _builder.BinaryGUID; }
            set { _builder.BinaryGUID = value; }
        }

        public int CacheSize
        {
            get { return _builder.CacheSize; }
            set { _builder.CacheSize = value; }
        }

        public string ConnectionString
        {
            get { return _builder.ConnectionString; }
            set { _builder.ConnectionString = value; }
        }

        public int Count
        {
            get { return _builder.Count; }
        }

        public string DataSource
        {
            get { return _builder.DataSource; }
            set { _builder.DataSource = value; }
        }

        public SQLiteDateFormats DateTimeFormat
        {
            get { return _builder.DateTimeFormat; }
            set { _builder.DateTimeFormat = value; }
        }

        public string DateTimeFormatString
        {
            get { return _builder.DateTimeFormatString; }
            set { _builder.DateTimeFormatString = value; }
        }

        public DateTimeKind DateTimeKind
        {
            get { return _builder.DateTimeKind; }
            set { _builder.DateTimeKind = value; }
        }

        public DbType DefaultDbType
        {
            get { return _builder.DefaultDbType; }
            set { _builder.DefaultDbType = value; }
        }

        public IsolationLevel DefaultIsolationLevel
        {
            get { return _builder.DefaultIsolationLevel; }
            set { _builder.DefaultIsolationLevel = value; }
        }

        public int DefaultTimeout
        {
            get { return _builder.DefaultTimeout; }
            set { _builder.DefaultTimeout = value; }
        }

        public string DefaultTypeName
        {
            get { return _builder.DefaultTypeName; }
            set { _builder.DefaultTypeName = value; }
        }

        public bool Enlist
        {
            get { return _builder.Enlist; }
            set { _builder.Enlist = value; }
        }

        public bool FailIfMissing
        {
            get { return _builder.FailIfMissing; }
            set { _builder.FailIfMissing = value; }
        }

        public SQLiteConnectionFlags Flags
        {
            get { return _builder.Flags; }
            set { _builder.Flags = value; }
        }

        public bool ForeignKeys
        {
            get { return _builder.ForeignKeys; }
            set { _builder.ForeignKeys = value; }
        }

        public string FullUri
        {
            get { return _builder.FullUri; }
            set { _builder.FullUri = value; }
        }

        public byte[] HexPassword
        {
            get { return _builder.HexPassword; }
            set { _builder.HexPassword = value; }
        }

        public bool IsFixedSize
        {
            get { return _builder.IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return _builder.IsReadOnly; }
        }

        public SQLiteJournalModeEnum JournalMode
        {
            get { return _builder.JournalMode; }
            set { _builder.JournalMode = value; }
        }

        public Collections.ICollection Keys
        {
            get { return _builder.Keys; }
        }

        public bool LegacyFormat
        {
            get { return _builder.LegacyFormat; }
            set { _builder.LegacyFormat = value; }
        }

        public int MaxPageCount
        {
            get { return _builder.MaxPageCount; }
            set { _builder.MaxPageCount = value; }
        }

        public bool NoSharedFlags
        {
            get { return _builder.NoSharedFlags; }
            set { _builder.NoSharedFlags = value; }
        }

        public int PageSize
        {
            get { return _builder.PageSize; }
            set { _builder.PageSize = value; }
        }

        public string Password
        {
            get { return _builder.Password; }
            set { _builder.Password = value; }
        }

        public bool Pooling
        {
            get { return _builder.Pooling; }
            set { _builder.Pooling = value; }
        }

        public bool ReadOnly
        {
            get { return _builder.ReadOnly; }
            set { _builder.ReadOnly = value; }
        }

        public bool SetDefaults
        {
            get { return _builder.SetDefaults; }
            set { _builder.SetDefaults = value; }
        }

        public SynchronizationModes SyncMode
        {
            get { return _builder.SyncMode; }
            set { _builder.SyncMode = value; }
        }

        public bool ToFullPath
        {
            get { return _builder.ToFullPath; }
            set { _builder.ToFullPath = value; }
        }

        public string Uri
        {
            get { return _builder.Uri; }
            set { _builder.Uri = value; }
        }

        public bool UseUTF16Encoding
        {
            get { return _builder.UseUTF16Encoding; }
            set { _builder.UseUTF16Encoding = value; }
        }

        public Collections.ICollection Values
        {
            get { return _builder.Values; }
        }

        public int Version
        {
            get { return _builder.Version; }
            set { _builder.Version = value; }
        }

        #endregion

        #region Methods

        #region Build

        /// <summary>
        /// Parses the connection string. The connectionString argument may be a file path or a full connection string.
        /// </summary>
        /// <param name="connectionString">A file path or a full connection string.</param>
        public static string Build(string connectionString)
        {
            return new SQLiteCSBuilder(connectionString).ConnectionString;
        }

        #endregion

        #region Parse

        void Parse(string connectionString)
        {
            if (SqlCSBuilder.DataSourceRegex.IsMatch(connectionString))
            {
                _builder = new SQLiteConnectionStringBuilder(connectionString);
            }
            else
            {
                /* Due to the newly revised connection string parsing algorithm (which was 
                * revised to fix other issues), opening a database on a UNC path requires 
                * that the initial backslashes in the file name be doubled
                * http://sqlite.1065341.n5.nabble.com/System-Data-SQLite-and-UNC-Paths-td72920.html 
                * */
                if (connectionString.StartsWith(@"\\") && !connectionString.StartsWith(@"\\\\")) connectionString = @"\\" + connectionString;

                _builder = new SQLiteConnectionStringBuilder();
                _builder.DataSource = connectionString;
                _builder.Version = 3;
                _builder.FailIfMissing = true;
                _builder.UseUTF16Encoding = true;
            }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return _builder.ToString();
        }

        #endregion

        #endregion
    }
}
