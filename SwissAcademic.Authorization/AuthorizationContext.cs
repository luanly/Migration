using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace SwissAcademic.Authorization
{
    public class AuthorizationContext
    {
        #region Constructors

        public AuthorizationContext(ClaimsPrincipal principal, IDictionary<object, object> items, AuthAction action, params AuthResource[] resources)
         : this((IPrincipal)principal, items, action, resources) { }

        public AuthorizationContext(IPrincipal principal, IDictionary<object, object> items, AuthAction action, params AuthResource[] resources)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (resources == null || !resources.Any())
            {
                throw new ArgumentNullException(nameof(resources));
            }
            Actions = new Dictionary<string, string> { { action.Key, action.Value } };
            Resources = resources.Where(r => r != null).ToDictionary(item => item.Key, item => item.Value);
            Principal = principal;
            Items = items ?? new Dictionary<object, object>();
        }

        #endregion

        #region Properties

        #region Action

        public Dictionary<string, string> Actions { get; set; }

        #endregion

        #region Items

        public IDictionary<object, object> Items { get; set; }

        #endregion

        #region Principal

        public IPrincipal Principal { get; set; }

        #endregion

        #region Resource

        public Dictionary<string, string> Resources { get; set; }

        #endregion

        #endregion
    }
}
