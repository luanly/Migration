using SwissAcademic;
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Security;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SQLite;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace System.Data.SQLite
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SasSQLiteConnection
        :
        DbConnection
    {
        #region Constructors

        private SasSQLiteConnection(SQLiteConnection connection, bool isEncrypted)
        {
            Connection = connection;
            IsEncrypted = isEncrypted;
        }

        #endregion

        #region Properties

        #region Connection

        public SQLiteConnection Connection { get; private set; }

        #endregion

        #region ConnectionString

        public override string ConnectionString
        {
            get { return Connection.ConnectionString; }
            set { Connection.ConnectionString = value; }
        }

        #endregion

        #region Database

        public override string Database
        {
            get { return Connection.Database; }
        }

        #endregion

        #region DataSource

        public override string DataSource
        {
            get { return Connection.DataSource; }
        }

        #endregion

        #region IsEncrypted

        public bool IsEncrypted { get; private set; }

        #endregion

        #region ServerVersion

        public override string ServerVersion
        {
            get { return Connection.ServerVersion; }
        }

        #endregion

        #region State

        public override ConnectionState State
        {
            get { return Connection.State; }
        }

        #endregion

        #endregion

        #region Methods

        #region BeginDbTransaction

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return Connection.BeginTransaction(isolationLevel);
        }

        #endregion

        #region ChangeDatabase

        public override void ChangeDatabase(string databaseName)
        {
            Connection.ChangeDatabase(databaseName);
        }

        #endregion

        #region Close

        public override void Close()
        {
            Connection.Close();
        }

        #endregion

        #region CreateDbCommand

        protected override DbCommand CreateDbCommand()
        {
            return Connection.CreateCommand();
        }

        #endregion

        #region CreateAndOpen

#if !Web
        static byte[] _pb = new byte[] { 70, 68, 51, 67, 53, 55, 55, 69, 48, 65, 51, 57, 52, 54, 56, 69, 52, 65, 65, 54, 65, 53, 55, 68, 70, 70, 57, 68, 66, 53, 48, 57 };
        static byte[] _p;

        /// <summary>
        /// Creates an SQLiteConnection and opens it. The connectionString argument may be a file path or a full connection string.
        /// </summary>
        /// <param name="connectionString">A file path or a full connection string.</param>
        /// <returns>An opened connection to the specified database.</returns>
        // TODO http://tfs2012:8080/tfs/CITAVICollection/Citavi/_workitems/edit/16607: Remove old sync code
        public static SasSQLiteConnection CreateAndOpen(string connectionString)
        {
            if (!connectionString.StartsWith("data source", StringComparison.OrdinalIgnoreCase))
            {
                connectionString = SQLiteCSBuilder.Build(connectionString);
            }

            var connection = new SQLiteConnection(connectionString);

            var isEncrypted = false;

            try
            {
                //Telemetry.Verbose(string.Empty, "DB Connection will be opened.", "Connection details: {0}; Database: {1}; Timeout: {2}; State: {3}", connection.ConnectionString, connection.Database, connection.ConnectionTimeout, connection.State);

                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT Count(*) FROM sqlite_master";
                    command.ExecuteScalar();
                }

                //Telemetry.Verbose(string.Empty, "DB Connection has been opened.", null);
            }

            catch (SQLiteException exception)
            {
                if (exception.ErrorCode == (int)SQLiteErrorCode.NotADb)
                {
                    connection.Close();
                    connection.Dispose();

                    var protectionService = SwissAcademic.Environment.ServiceProvider.GetService<IDataProtectionService>();
                    if (_p == null)
                    {
                        _p = protectionService.ProtectForSameProcess(Encoding.ASCII.GetString(_pb));
                    }

                    var csBuilder = new SQLiteCSBuilder(connectionString);
                    csBuilder.Password = protectionService.UnprotectForSameProcess(_p);
                    connection = new SQLiteConnection(csBuilder.ConnectionString);
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT Count(*) FROM sqlite_master";
                        command.ExecuteScalar();
                    }

                    Telemetry.TrackTrace("DB Connection has been opened.");
                    isEncrypted = true;
                }
                else
                {
                    throw;
                }
            }

            return new SasSQLiteConnection(connection, isEncrypted);
        }
#endif

        #endregion

        #region CreateAndOpenAsync

#if !Web
        /// <summary>
        /// Creates an SQLiteConnection and opens it asynchronously. The connectionString argument may be a file path or a full connection string.
        /// </summary>
        /// <param name="connectionString">A file path or a full connection string.</param>
        /// <returns>An opened connection to the specified database.</returns>
        public static async Task<SasSQLiteConnection> CreateAndOpenAsync(string connectionString, CancellationToken cancellationToken)
        {
            if (!connectionString.StartsWith("data source", StringComparison.OrdinalIgnoreCase))
            {
                connectionString = SQLiteCSBuilder.Build(connectionString);
            }

            var connection = new SQLiteConnection(connectionString);
            var isEncrypted = false;

            //Telemetry.Verbose(string.Empty, "DB Connection will be opened.", "Connection details: {0}; Database: {1}; Timeout: {2}; State: {3}", connection.ConnectionString, connection.Database, connection.ConnectionTimeout, connection.State);

            await connection.OpenAsync(cancellationToken);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Count(*) FROM sqlite_master";
                command.ExecuteScalar();
            }


            return new SasSQLiteConnection(connection, isEncrypted);
        }
#endif

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connection != null)
                {
                    Connection.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Open

        public override void Open()
        {
            Connection.Open();
        }

        #endregion

        #endregion
    }
}
