using SwissAcademic;
using SwissAcademic.Security;
using System.ComponentModel;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace System.Data.SqlClient
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SasSqlConnection
        :
        DbConnection
    {
        #region Fields
#if !Web
        IImpersonation _impersonation;
#endif

        #endregion

        #region Constructors

#if !Web

        private SasSqlConnection(SqlConnection connection, IImpersonation impersonation)
        {
            Connection = connection;
            _impersonation = impersonation;
        }

#else
        private SasSqlConnection(SqlConnection connection)
        {
            Connection = connection;
        }

#endif

        #endregion

        #region Properties

        #region Connection

        public SqlConnection Connection { get; private set; }

        #endregion

        #region ConnectionString

        public override string ConnectionString
        {
            get
            {
                return Connection.ConnectionString;
            }
            set
            {
                throw new NotSupportedException();
            }
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

        #region IsImpersonated

#if !Web

        public bool IsImpersonated
        {
            get { return _impersonation != null; }
        }

#endif

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

        #region IsDisposed

        public bool IsDisposed { get; private set; }

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
            Dispose();
        }

        #endregion

        #region Create

        public static SasSqlConnection Create(string connectionString)
        {
            var builder = new SqlCSBuilder(connectionString);

#if !Web
            IImpersonation impersonation = null;

            if (builder.Impersonate)
            {
                impersonation = SwissAcademic.Environment.ServiceProvider.GetService<IImpersonationService>().Create();
                impersonation.LogonUser(builder.ImpersonationDomain, builder.ImpersonationUserName, builder.Password);
            }
            var connection = new SqlConnection(builder.ToString());
            return new SasSqlConnection(connection, impersonation);
#else
            var connection = new SqlConnection(builder.ToString());
            return new SasSqlConnection(connection);
#endif

        }

        #endregion

        #region CreateAndOpen

        public static SasSqlConnection CreateAndOpen(string connectionString)
        {
            var builder = new SqlCSBuilder(connectionString);

#if !Web

            IImpersonation impersonation = null;
            if (builder.Impersonate)
            {
                impersonation = SwissAcademic.Environment.ServiceProvider.GetService<IImpersonationService>().Create();
                impersonation.LogonUser(builder.ImpersonationDomain, builder.ImpersonationUserName, builder.Password);
            }

            var connection = new SqlConnection(builder.ToString());

            connection.Open();

            return new SasSqlConnection(connection, impersonation);
#else
            var connection = new SqlConnection(builder.ToString());
            connection.Open();
            return new SasSqlConnection(connection);
#endif

        }

        #endregion

        #region CreateAndOpenAsync

        public static async Task<SasSqlConnection> CreateAndOpenAsync(string connectionString, CancellationToken cancellationToken)
        {
            var builder = new SqlCSBuilder(connectionString);

#if !Web
            IImpersonation impersonation = null;
            if (builder.Impersonate)
            {
                impersonation = SwissAcademic.Environment.ServiceProvider.GetService<IImpersonationService>().Create();
                impersonation.LogonUser(builder.ImpersonationDomain, builder.ImpersonationUserName, builder.Password);
            }

            var connection = new SqlConnection(builder.ToString());
            await connection.OpenAsync(cancellationToken);

            return new SasSqlConnection(connection, impersonation);

#else
            var connection = new SqlConnection(builder.ToString());
            await connection.OpenAsync(cancellationToken);

            return new SasSqlConnection(connection);
#endif


        }

        #endregion

        #region CreateDbCommand

        protected override DbCommand CreateDbCommand()
        {
            return Connection.CreateCommand();
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                if (Connection != null) Connection.Dispose();
#if !Web
                if (_impersonation != null) _impersonation.Dispose();
#endif
            }

            base.Dispose(disposing);

            IsDisposed = true;
        }

        #endregion

        #region Open

        public override void Open()
        {
            Connection.Open();
        }

        #endregion

        #endregion

        public static implicit operator SqlConnection(SasSqlConnection connection)
        {
            return connection.Connection;
        }

    }
}
