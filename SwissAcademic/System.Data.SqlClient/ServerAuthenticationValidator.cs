
namespace System.Data.SqlClient
{
#if !Web
    public static class ServerAuthenticationValidator
    {
    #region Validate

        public static AuthenticationValidatorResult Validate(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return AuthenticationValidatorResult.Undefined;

            var csBuilder = new SqlCSBuilder(connectionString);

            return csBuilder.IntegratedSecurity
                    ? ValidateIntegratedSecurity(csBuilder)
                    : ValidateNonIntegratedSecurity(csBuilder);
        }

    #endregion

    #region ValidateIntegratedSecurity

        static AuthenticationValidatorResult ValidateIntegratedSecurity(SqlCSBuilder builder)
        {
            if (builder.Impersonate)
            {
                if (builder.IsPasswordMasked) return new AuthenticationValidatorResult(false, AuthentificationType.ImpersonateWindowsSecurity);
                return new AuthenticationValidatorResult(true, AuthentificationType.ImpersonateWindowsSecurity);
            }
            else
            {
                return new AuthenticationValidatorResult(true, AuthentificationType.ImpersonateWindowsSecurity);
            }
        }

    #endregion

    #region ValidateNonIntegratedSecurity

        static AuthenticationValidatorResult ValidateNonIntegratedSecurity(SqlCSBuilder builder)
        {
            if (string.IsNullOrEmpty(builder.UserID)) return new AuthenticationValidatorResult(false, AuthentificationType.SqlSecurity);

            if (string.IsNullOrEmpty(builder.Password)) return new AuthenticationValidatorResult(false, AuthentificationType.SqlSecurity);

            if (builder.IsPasswordMasked) return new AuthenticationValidatorResult(false, AuthentificationType.SqlSecurity);

            return new AuthenticationValidatorResult(true, AuthentificationType.SqlSecurity);
        }

    #endregion
    }
#endif

    public class AuthenticationValidatorResult
    {
        #region Constructors

        internal AuthenticationValidatorResult(bool isAuthenticated, AuthentificationType authentificationType)
        {
            IsAuthenticated = isAuthenticated;
            AuthentificationType = authentificationType;
        }

        #endregion

        #region Properties

        #region IsAuthenticated

        public bool IsAuthenticated { get; private set; }

        #endregion

        #region AuthentificationType

        public AuthentificationType AuthentificationType { get; private set; }

        #endregion

        #region Undefined

        public static AuthenticationValidatorResult Undefined { get { return new AuthenticationValidatorResult(false, AuthentificationType.Undefined); } }

        #endregion


        #endregion
    }

    public enum AuthentificationType
    {
        WindowsSecurity,
        SqlSecurity,
        ImpersonateWindowsSecurity,
        Undefined
    }
}
