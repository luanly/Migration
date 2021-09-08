using SwissAcademic;
using SwissAcademic.Security;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace System.Data.SqlClient
{
    public class SqlCSBuilder
    {
        #region Regex

        public static readonly Regex DataSourceRegex = new Regex(@"Data\sSource\s?=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        public static readonly Regex InitialCatalogRegex = new Regex(@"Initial\sCatalog\s?=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        public static readonly Regex SecurityRegex = new Regex(@"(Integrated\sSecurity|User\sID)\s?=", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        #endregion

        #region Fields

        SqlConnectionStringBuilder _builder = new SqlConnectionStringBuilder();

        #endregion

        #region Constructors

        public SqlCSBuilder()
        {
        }

        public SqlCSBuilder(string connectionStringWithSchema)
        {
            Parse(connectionStringWithSchema);
        }

        public SqlCSBuilder(string connectionString, string schema)
        {
            Parse(connectionString, schema);
        }

        #endregion

        #region Properties

        #region ConnectionStringWithSchema

#if !Web
        public string ConnectionStringWithSchema
        {
            get { return ToString(); }
            set { Parse(value); }
        }
#endif

        #endregion

        #region Identifier

        public string Identifier
        {
            get { return string.Join(";", DataSource, InitialCatalog, Schema); }
        }

        #endregion

        #region IdentifierWithUserId

        public string IdentifierWithUserId
        {
            get { return string.Join(";", DataSource, InitialCatalog, IntegratedSecurity, UserID); }
        }

        #endregion

        #region Impersonate

#if !Web

        public bool Impersonate
        {
            get
            {
                return
                    IntegratedSecurity &&
                    !string.IsNullOrEmpty(ImpersonationDomain) &&
                    !string.IsNullOrEmpty(ImpersonationUserName) &&
                    !string.IsNullOrEmpty(Password) &&
                    (!System.Environment.UserDomainName.Equals(ImpersonationDomain, StringComparison.OrdinalIgnoreCase));
            }
        }
#endif
        #endregion

        #region ImpersonationDomain

        public string ImpersonationDomain { get; private set; }

        #endregion

        #region ImpersonationUserName

        public string ImpersonationUserName { get; private set; }

        #endregion

        #region IsPasswordMasked

        public bool IsPasswordMasked
        {
            get
            {
                if (string.IsNullOrEmpty(Password)) return false;
                return Password.All(c => c == '*');
            }
        }

        #endregion

        #region Schema

        public string Schema { get; set; }

        #endregion

        #region ServerDomain

        public string ServerDomain
        {
            get
            {
                var split = from item in UserID.Split('\\')
                            let trimmed = item.Trim()
                            where !string.IsNullOrWhiteSpace(trimmed)
                            select trimmed;

                if (split.Count() == 2) return split.First();

                var serverName = ServerName;

                if (ServerName.IndexOf('.') != -1)
                {
                    var hosts = (from item in System.Net.Dns.GetHostEntry(serverName).HostName.Split('.')
                                 let trimmed = item.Trim()
                                 where !string.IsNullOrEmpty(trimmed) && !trimmed.IsNumeric()
                                 select trimmed).ToList();

                    return string.Join(".", hosts.Skip(1).Take(hosts.Count - 2));
                }
                else
                {
                    return System.Environment.MachineName;
                }
            }
        }

        #endregion

        #region ServerName

        public string ServerName
        {
            get { return DataSource.Split(',', '\\')[0].Trim(); }
        }

        #endregion

        #region SqlConnectionStringBuilder Properties

        public ApplicationIntent ApplicationIntent
        {
            get { return _builder.ApplicationIntent; }
            set { _builder.ApplicationIntent = value; }
        }

        public string ApplicationName
        {
            get { return _builder.ApplicationName; }
            set { _builder.ApplicationName = value; }
        }

        public string AttachDBFilename
        {
            get { return _builder.AttachDBFilename; }
            set { _builder.AttachDBFilename = value; }
        }

        public string ConnectionString
        {
            get { return _builder.ConnectionString; }
            set
            {
                _builder.ConnectionString = value;
#if !Web
                if (_builder.Password != null && _builder.Password.Length > 128)
                {
                    _builder.Password = SwissAcademic.Environment.ServiceProvider.GetService<IDataProtectionService>().UnprotectForCurrentUser(Convert.FromBase64String(_builder.Password));
                }
#endif
            }
        }

        public int ConnectRetryCount
        {
            get { return _builder.ConnectRetryCount; }
            set { _builder.ConnectRetryCount = value; }
        }

        public int ConnectRetryInterval
        {
            get { return _builder.ConnectRetryInterval; }
            set { _builder.ConnectRetryInterval = value; }
        }

        public int ConnectTimeout
        {
            get { return _builder.ConnectTimeout; }
            set { _builder.ConnectTimeout = value; }
        }

        public int Count
        {
            get { return _builder.Count; }
        }

        public string CurrentLanguage
        {
            get { return _builder.CurrentLanguage; }
            set { _builder.CurrentLanguage = value; }
        }

        public string DataSource
        {
            get { return _builder.DataSource; }
            set { _builder.DataSource = value; }
        }

        public bool Encrypt
        {
            get { return _builder.Encrypt; }
            set { _builder.Encrypt = value; }
        }

        public bool Enlist
        {
            get { return _builder.Enlist; }
            set { _builder.Enlist = value; }
        }

        public string FailoverPartner
        {
            get { return _builder.FailoverPartner; }
            set { _builder.FailoverPartner = value; }
        }

        public string InitialCatalog
        {
            get { return _builder.InitialCatalog; }
            set { _builder.InitialCatalog = value; }
        }

        public bool IntegratedSecurity
        {
            get { return _builder.IntegratedSecurity; }
            set { _builder.IntegratedSecurity = value; }
        }

        public bool IsFixedSize
        {
            get { return _builder.IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return _builder.IsReadOnly; }
        }

        public int LoadBalanceTimeout
        {
            get { return _builder.LoadBalanceTimeout; }
            set { _builder.LoadBalanceTimeout = value; }
        }

        public Collections.ICollection Keys
        {
            get { return _builder.Keys; }
        }

        public int MaxPoolSize
        {
            get { return _builder.MaxPoolSize; }
            set { _builder.MaxPoolSize = value; }
        }

        public int MinPoolSize
        {
            get { return _builder.MinPoolSize; }
            set { _builder.MinPoolSize = value; }
        }

        public bool MultipleActiveResultSets
        {
            get { return _builder.MultipleActiveResultSets; }
            set { _builder.MultipleActiveResultSets = value; }
        }

        public bool MultiSubnetFailover
        {
            get { return _builder.MultiSubnetFailover; }
            set { _builder.MultiSubnetFailover = value; }
        }

        public int PacketSize
        {
            get { return _builder.PacketSize; }
            set { _builder.PacketSize = value; }
        }

        public string Password
        {
            get { return _builder.Password; }
            set
            {
#if !Web
                if (value != null && value.Length > 128)
                {
                    value = SwissAcademic.Environment.ServiceProvider.GetService<IDataProtectionService>().UnprotectForCurrentUser(Convert.FromBase64String(value));
                }
#endif

                _builder.Password = value;
            }
        }

        public bool PersistSecurityInfo
        {
            get { return _builder.PersistSecurityInfo; }
            set { _builder.PersistSecurityInfo = value; }
        }

        public bool Pooling
        {
            get { return _builder.Pooling; }
            set { _builder.Pooling = value; }
        }

        public bool Replication
        {
            get { return _builder.Replication; }
            set { _builder.Replication = value; }
        }

        public string TransactionBinding
        {
            get { return _builder.TransactionBinding; }
            set { _builder.TransactionBinding = value; }
        }

        public bool TrustServerCertificate
        {
            get { return _builder.TrustServerCertificate; }
            set { _builder.TrustServerCertificate = value; }
        }

        public string TypeSystemVersion
        {
            get { return _builder.TypeSystemVersion; }
            set { _builder.TypeSystemVersion = value; }
        }

        public string UserID
        {
            get { return _builder.UserID; }
            set { _builder.UserID = value; }
        }

        public bool UserInstance
        {
            get { return _builder.UserInstance; }
            set { _builder.UserInstance = value; }
        }

        public Collections.ICollection Values
        {
            get { return _builder.Values; }
        }

        public string WorkstationID
        {
            get { return _builder.WorkstationID; }
            set { _builder.WorkstationID = value; }
        }

        #endregion

        #endregion

        #region Methods

        #region Parse

        static Regex SchemaRegex = new Regex(";\\s*Schema\\s*Name\\s*=\\s*(?<SchemaName>(\\\"?.+\\\"?))", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant);


        void Parse(string connectionStringWithSchema)
        {
            if (string.IsNullOrEmpty(connectionStringWithSchema)) return;

            string schema = null;

            var match = SchemaRegex.Match(connectionStringWithSchema);
            if (match.Success)
            {
                schema = match.Groups["SchemaName"].Value.Trim('"');
                connectionStringWithSchema = SchemaRegex.Replace(connectionStringWithSchema, string.Empty);
            }

            Parse(connectionStringWithSchema, schema);
        }

        void Parse(string connectionString, string schema)
        {
            Schema = schema;
            _builder = new SqlConnectionStringBuilder(connectionString);
#if !Web
            if (Password != null && Password.Length > 128)
            {
                Password = SwissAcademic.Environment.ServiceProvider.GetService<IDataProtectionService>().UnprotectForCurrentUser(Convert.FromBase64String(Password));
            }
#endif

            if (IntegratedSecurity && !string.IsNullOrEmpty(UserID) && !string.IsNullOrEmpty(Password))
            {
                var split = from item in UserID.Split('\\')
                            let trimmed = item.Trim()
                            where !string.IsNullOrEmpty(trimmed)
                            select trimmed;

                if (split.Count() == 2)
                {
                    ImpersonationDomain = split.First();
                    ImpersonationUserName = split.Last();
                }
            }
        }



#endregion

#region ToString()

        public override string ToString()
        {
            return ToString(true, PasswordHandling.Clear);
        }

        static Regex NonAlphanumericRegex = new Regex(@"[\W]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public string ToString(bool includeSchema, PasswordHandling passwordHandling)
        {
            string connectionString;

            if (string.IsNullOrEmpty(Password))
            {
                connectionString = _builder.ConnectionString;
            }
            else
            {
                switch (passwordHandling)
                {
                    case PasswordHandling.Clear:
                        connectionString = _builder.ConnectionString;
                        break;

                    case PasswordHandling.Encrypted:
                        {
                            var newBuilder = new SqlConnectionStringBuilder(_builder.ConnectionString);
#if !Web
                            newBuilder.Password = Convert.ToBase64String(SwissAcademic.Environment.ServiceProvider.GetService<IDataProtectionService>().ProtectForCurrentUser(Password));
#endif
                            connectionString = newBuilder.ConnectionString;
                        }
                        break;

                    case PasswordHandling.Remove:
                        {
                            var newBuilder = new SqlConnectionStringBuilder(_builder.ConnectionString);
                            newBuilder.Remove("Password");
                            newBuilder.Remove("User ID");
                            newBuilder.IntegratedSecurity = true;
                            connectionString = newBuilder.ConnectionString;
                        }
                        break;

                    default:
                        {
                            var newBuilder = new SqlConnectionStringBuilder(_builder.ConnectionString);
                            newBuilder.Password = "******";
                            connectionString = newBuilder.ConnectionString;
                        }
                        break;
                }
            }

            if (includeSchema && !string.IsNullOrEmpty(Schema))
            {
                var schema = NonAlphanumericRegex.IsMatch(Schema) ?
                    $"\"{Schema}\"" :
                    Schema;
                return string.Join(";", connectionString, "Schema Name=" + schema);
            }

            return connectionString;
        }

#endregion

#endregion
    }

    public enum PasswordHandling
    {
        Masked,
        Encrypted,
        Clear,
        Remove
    }
}
