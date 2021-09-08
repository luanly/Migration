using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SwissAcademic.Authorization
{
    public abstract class AuthorizationManagerBase
    {
        #region Constructors

        protected AuthorizationManagerBase()
        {
        }

        #endregion

        #region Methods

        #region CheckAccessAsync

        public Task<bool> CheckAccessAsync(System.Security.Claims.ClaimsPrincipal principal, AuthAction action, params AuthResource[] resources)
            => CheckAccessAsync((IPrincipal)principal, action, resources);
        public Task<bool> CheckAccessAsync(IPrincipal principal, AuthAction action, params AuthResource[] resources)
            => CheckAccessAsync(principal, null, action, resources);

        public Task<bool> CheckAccessAsync(IPrincipal principal, IDictionary<object, object> items, AuthAction action, params AuthResource[] resources)
        {
            var context = new AuthorizationContext(principal,
                    items,
                    action,
                    resources);
            var result = CheckAccessAsync(context);
            return result;
        }

        public abstract Task<bool> CheckAccessAsync(AuthorizationContext context);

        #endregion

        #region ForbidAsync

        protected virtual bool Forbid()
        {
            var exceptionSetting = ConfigurationManager.AppSettings["ReturnAuthorizationExceptionAsBool"];
            if (!string.IsNullOrEmpty(exceptionSetting) && exceptionSetting.Equals("true", StringComparison.Ordinal)) return false;
            throw new AuthorizationException();
        }

        protected virtual bool Forbid(string msg)
        {
            var exceptionSetting = ConfigurationManager.AppSettings["ReturnAuthorizationExceptionAsBool"];
            if (!string.IsNullOrEmpty(exceptionSetting) && exceptionSetting.Equals("true", StringComparison.Ordinal)) return false;
            throw new AuthorizationException(msg);
        }

        #endregion

        #region Conflict

        protected virtual bool Conflict(string message)
        {
            var exceptionSetting = ConfigurationManager.AppSettings["ReturnAuthorizationExceptionAsBool"];
            if (!string.IsNullOrEmpty(exceptionSetting) && exceptionSetting.Equals("true", StringComparison.Ordinal)) return false;
            throw new ConflictException(message);
        }

        #endregion

        #region NotFound

        protected virtual bool NotFound()
        {
            var exceptionSetting = ConfigurationManager.AppSettings["ReturnAuthorizationExceptionAsBool"];
            if (!string.IsNullOrEmpty(exceptionSetting) && exceptionSetting.Equals("true", StringComparison.Ordinal)) return false;
            throw new NoLongerAvailableException();
        }

        #endregion

        #region ProjectRoleNotConfirmed

        protected virtual bool ProjectRoleNotConfirmed()
        {
            var exceptionSetting = ConfigurationManager.AppSettings["ReturnAuthorizationExceptionAsBool"];
            if (!string.IsNullOrEmpty(exceptionSetting) && exceptionSetting.Equals("true", StringComparison.Ordinal)) return false;
            throw new AuthorizationException();
        }

        #endregion

        #region ProjectMissingWritePermission

        protected virtual bool ProjectMissingWritePermission()
        {
            var exceptionSetting = ConfigurationManager.AppSettings["ReturnAuthorizationExceptionAsBool"];
            if (!string.IsNullOrEmpty(exceptionSetting) && exceptionSetting.Equals("true", StringComparison.Ordinal))
            {
                return false;
            }
            throw new AuthorizationException();
        }

        #endregion

        #region ProjectMissingReadPermission

        protected virtual bool ProjectMissingReadPermission()
        {
            var exceptionSetting = ConfigurationManager.AppSettings["ReturnAuthorizationExceptionAsBool"];
            if (!string.IsNullOrEmpty(exceptionSetting) && exceptionSetting.Equals("true", StringComparison.Ordinal))
            {
                return false;
            }
            throw new AuthorizationException();
        }

        #endregion

        #region RequireAuthentication

        protected virtual bool RequireAuthentication(AuthorizationContext context)
        {
            if (context.Principal.Identity?.IsAuthenticated == true) return true;

            var exceptionSetting = ConfigurationManager.AppSettings["ReturnAuthorizationExceptionAsBool"];
            if (!string.IsNullOrEmpty(exceptionSetting) && exceptionSetting.Equals("true", StringComparison.Ordinal)) return false;
            throw new AuthorizationException();
        }

        #endregion

        #endregion
    }
}
