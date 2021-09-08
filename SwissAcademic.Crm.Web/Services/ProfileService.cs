using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class ProfileService
        :
        IProfileService
    {
		#region Fields

		IHttpContextAccessor _httpContextAccessor;

        #endregion

        #region Konstruktor

        public ProfileService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region GetProfileDataAsync

        public async Task GetProfileDataAsync(ProfileDataRequestContext request)
        {
            using (var context = new CrmDbContext())
            {
                var contactKey = request.Subject.GetContactKey();
                var user = _httpContextAccessor?.GetCrmUser();
                if (user == null)
                {
                    user = await context.GetByKeyAsync(contactKey, updateCacheIfMissing: true);
                }
                if (user == null)
                {
                    var exception = new ArgumentException($"Contact key {contactKey} not found");
                    Telemetry.TrackException(exception);
                }
                var claims = new List<Claim>();
                var userClaims = GetClaimsFromAccount(user).ToList();
                claims.AddRange(userClaims);

                if (request.RequestedClaimTypes.Any(claim => claim == CitaviClaimTypes.Project))
                {
                    claims.AddRange(user.ProjectRoles.Select((project) =>
                    {
                        return new Claim(CitaviClaimTypes.Project, CrmJsonConvert.SerializeObject(project));
                    }));
                }

                if(request.Client != null)
                {
                    if (request.Client.ClientId == ClientIds.UseResponse)
                    {
                        //email hinzufügen auch wenn nicht requested.
                        if (!claims.Any(c => c.Type == JwtClaimTypes.Email))
                        {
                            if (!string.IsNullOrWhiteSpace(user.Email))
                            {
                                claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
                            }
                        }
                    }
                    context.Attach(user);
                    if (request.Client.ClientId != ClientIds.Web)
                    {
                        if (user.SetLastLogin(request.Client.ClientId))
                        {
                            await context.SaveAndUpdateUserCacheAsync(user);
                        }
                    }
                }

                request.IssuedClaims = claims;
            }
        }

        #endregion

        #region GetClaimsFromAccount

        public IEnumerable<Claim> GetClaimsFromAccount(CrmUser user)
        {
            var claims = new List<Claim>{
                new Claim(JwtClaimTypes.Subject, user.Key),
                new Claim(JwtClaimTypes.PreferredUserName, user.Username ?? string.Empty),
            };

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                claims.Add(new Claim(JwtClaimTypes.Email, user.Email));
            }

            claims.Add(new Claim(JwtClaimTypes.Locale, user.Contact.LanguageTwoDigits));

            if (user.Contact.FirstName != null)
            {
                claims.Add(new Claim(JwtClaimTypes.GivenName, user.Contact.FirstName));
            }

            if (user.Contact.LastName != null)
            {
                claims.Add(new Claim(JwtClaimTypes.FamilyName, user.Contact.LastName));
            }

            if (!string.IsNullOrEmpty(user.Contact.NickName))
            {
                claims.Add(new Claim(JwtClaimTypes.NickName, user.Contact.NickName));
            }

            claims.Add(new Claim(nameof(Contact.HasUserSettingsWE), user.Contact.HasUserSettingsWE.ToString()));
            claims.Add(new Claim(CitaviClaimTypes.DataCenter, user.DataCenter.ToString()));
            claims.Add(new Claim(CitaviClaimTypes.DataCenterShortName, AzureRegionResolver.Instance.GetShortName(user.DataCenter)));

            return claims;
        }

        #endregion

        #region IsActiveAsync


        public async Task IsActiveAsync(IsActiveContext isActiveContext)
        {
            if (isActiveContext.Caller == "AccessTokenValidation")
            {
                //Es kann nie der Fall sein, dass wir AccessTokens haben und der CRMUser ist deaktiviert
                isActiveContext.IsActive = true;
                return;
            }
            using (var context = new CrmDbContext())
            {
                var user = await context.GetByKeyAsync(isActiveContext.Subject.GetContactKey(), updateCacheIfMissing: true);
                if (user == null)
                {
                    isActiveContext.IsActive = false;
                    return;
                }
                if (user != null &&
                   _httpContextAccessor != null)
                {
                    _httpContextAccessor.SetCrmUser(user);
                }
                isActiveContext.IsActive = user.IsLoginAllowed;
            }
        }

        #endregion
    }
}
