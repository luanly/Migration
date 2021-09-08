using SwissAcademic.ApplicationInsights;
using SwissAcademic.Authorization;
using SwissAcademic.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web.Authorization
{
    partial class AuthorizationManager
    {
        #region CheckCrmAccessAsync

        async Task<bool> CheckCrmAccessAsync(CrmDbContext dbContext, AuthorizationContext context)
        {
            var resource = context.Resources.First().Value;
            var access = false;

            switch (resource)
            {
                case nameof(AuthResource.CrmCampusOrganizationSetting):
                    {
                        access = await CheckCampusContractOrganizationSettingsAccessAsync(dbContext, context);
                    }
                    break;

                case nameof(AuthResource.CrmContact):
                    {
                        access = await CheckCrmContactAccessAsync(dbContext, context);
                    }
                    break;

                case nameof(AuthResource.CrmProjectEntry):
                    {
                        return await CheckCrmProjectEntryAccessAsync(context);
                    }

                case nameof(AuthResource.CrmProjectRole):
                    {
                        access = await CheckCrmProjectRoleAccessAsync(context);
                    }
                    break;

                case nameof(AuthResource.CrmLinkedAccount):
                    {
                        access = await CheckCrmLinkedAccountAccessAsync(dbContext, context);
                    }
                    break;

                case nameof(AuthResource.CrmLinkedEmailAccount):
                    {
                        access = await CheckCrmLinkedEmailAccountAccessAsync(dbContext, context);
                    }
                    break;
            }

            if (access)
            {
                return await CheckCrmKeyContactAccessAsync(dbContext, context);
            }
            else
            {
                Telemetry.TrackTrace($"CheckCrmAccessAsync failed: {resource}", SeverityLevel.Warning);
            }

            return access;
        }

        #endregion

        #region CheckCrmKeyAccountAccess

        async Task<bool> CheckCrmContactAccessAsync(CrmDbContext dbContext, AuthorizationContext context)
        {
            var action = context.Actions.First().Value;

            switch (action)
            {
                case nameof(AuthAction.Delete):
                    {
                        var contactKey = System.Security.Claims.ClaimsPrincipalExtensions.GetContactKey(context.Principal);
                        if (!contactKey.Equals(context.Resources[nameof(AuthResource.ContactKey)]))
                        {
                            return false;
                        }

                        var user = await dbContext.GetByKeyAsync(contactKey);
                        if (user == null)
                        {
                            return false;
                        }

                        return user.AllowDeletion;
                    }

                case nameof(AuthAction.Merge):
                    {
                        return await CheckCrmKeyContactAccessAsync(dbContext, context);
                    }

                default:
                    {
                        var contactKey = System.Security.Claims.ClaimsPrincipalExtensions.GetContactKey(context.Principal);
                        if (!contactKey.Equals(context.Resources[nameof(AuthResource.ContactKey)]))
                        {
                            return false;
                        }
                    }
                    break;
            }


            return true;
        }

        #endregion

        #region CheckCrmKeyContactAccess

        async Task<bool> CheckCrmKeyContactAccessAsync(CrmDbContext dbContext, AuthorizationContext context)
        {
            var resource = context.Resources.First().Value;
            var action = context.Actions.First().Value;
            if (action == nameof(AuthAction.Read))
            {
                return true;
            }

            string contactKey;
            if (context.Resources.ContainsKey(nameof(AuthResource.ContactKey)))
            {
                contactKey = context.Resources[nameof(AuthResource.ContactKey)];
            }
            else
            {
                contactKey = System.Security.Claims.ClaimsPrincipalExtensions.GetContactKey(context.Principal);
            }

            var user = await dbContext.GetByKeyAsync(contactKey, updateCacheIfMissing: true);
            if (user == null)
            {
                return false;
            }

            if (!user.Contact.IsKeyContact)
            {
                return true;
            }

            switch (resource)
            {
                //Key-Contact darf nicht ändern, hinzufügen, löschen
                case nameof(AuthResource.CrmContact):
                case nameof(AuthResource.CrmLinkedEmailAccount):
                case nameof(AuthResource.CrmLinkedAccount):
                    return Forbid(AuthenticateResultConstants.ContactUpdateFailedIsKeyContact);
            }

            return true;
        }

        #endregion

        #region CheckCrmProjectEntryAccess

        async Task<bool> CheckCrmProjectEntryAccessAsync(AuthorizationContext context)
        {
            var action = context.Actions.First().Value;
            if (action != nameof(AuthAction.Create) &&
                !(await CheckProjectCompatibilityAsync(context)).Ok)
            {
                return false;
            }

            var projectKey = context.Resources[nameof(AuthResource.ProjectKey)];

            switch (action)
            {
                case nameof(AuthAction.Create):
                    // Currently, all authenticated users have the permission to create a project.
                    return true;

                case nameof(AuthAction.Delete):
                    {
                        var projectRole = await GetProjectRoleAsync(context);
                        if (projectRole == null)
                        {
                            return NotFound();
                        }

                        if (!projectRole.Confirmed)
                        {
                            return ProjectRoleNotConfirmed();
                        }

                        return projectRole.ProjectRoleType == ProjectRoleType.Owner;
                    }

                case nameof(AuthAction.Update):
                    {
                        var projectRole = await GetProjectRoleAsync(context);
                        if (projectRole == null)
                        {
                            return NotFound();
                        }

                        if (!projectRole.Confirmed)
                        {
                            return ProjectRoleNotConfirmed();
                        }

                        switch (projectRole.ProjectRoleType)
                        {
                            case ProjectRoleType.Manager:
                            case ProjectRoleType.Owner:
                                return true;

                            default:
                                return false;
                        }
                    }

                case nameof(AuthAction.Read):
                    {
                        var projectRole = await GetProjectRoleAsync(context);
                        if (projectRole == null)
                        {
                            return NotFound();
                        }

                        if (!projectRole.Confirmed)
                        {
                            return ProjectRoleNotConfirmed();
                        }

                        switch (projectRole.ProjectRoleType)
                        {
                            case ProjectRoleType.Author:
                            case ProjectRoleType.Manager:
                            case ProjectRoleType.Owner:
                            case ProjectRoleType.Reader:
                                return true;

                            default:
                                return false;
                        }
                    }
            }

            return false;
        }

        #endregion

        #region CheckCrmProjectRoleAccess

        async Task<bool> CheckCrmProjectRoleAccessAsync(AuthorizationContext context)
        {
            var projectRole = await GetProjectRoleAsync(context);
            if (projectRole == null)
            {
                return NotFound();
            }

            switch (context.Actions.First().Value)
            {
                case nameof(AuthAction.Delete):
                    {
                        if (projectRole.ProjectRoleType == ProjectRoleType.Owner)
                        {
                            return false;
                        }

                        return true;
                    }

                case nameof(AuthAction.Create):
                case nameof(AuthAction.Update):
                    {
                        switch (projectRole.ProjectRoleType)
                        {
                            case ProjectRoleType.Manager:
                            case ProjectRoleType.Owner:
                                return true;
                        }
                    }
                    break;

                case nameof(AuthAction.Read):
                    {
                        switch (projectRole.ProjectRoleType)
                        {
                            case ProjectRoleType.Author:
                            case ProjectRoleType.Manager:
                            case ProjectRoleType.Owner:
                            case ProjectRoleType.Reader:
                                return true;
                        }
                    }
                    break;
            }

            return false;
        }

        #endregion

        #region CheckCrmLinkedEmailAccountAccess

        async Task<bool> CheckCrmLinkedEmailAccountAccessAsync(CrmDbContext dbContext, AuthorizationContext context)
        {
            var action = context.Actions.First().Value;
            var email = context.Resources[nameof(AuthResource.CrmLinkedEmailAccount)];
            var contactKey = System.Security.Claims.ClaimsPrincipalExtensions.GetContactKey(context.Principal);

            var user = await context.GetCrmUserAsync(dbContext);

            if (user == null)
            {
                return false;
            }

            switch (action)
            {
                case nameof(AuthAction.Delete):
                    {
                        var linkedAccount = user.GetLinkedEmailAccount(email);

                        if (linkedAccount == null)
                        {
                            Telemetry.TrackTrace($"{nameof(LinkedEmailAccount)} not found: {email}");
                            return false;
                        }

                        if (!user.CanRemoveLinkedEmailAddress(linkedAccount.Email))
                        {
                            Telemetry.TrackTrace($"Cant remove last linkedEmailAccount");
                            return false;
                        }

                        return true;
                    }

                case nameof(AuthAction.Create):
                    {
                        return !await new CrmUserManager(dbContext).EmailExistsAsync(email);
                    }

                default:
                    {
                        var linkedAccount = user.GetLinkedEmailAccount(email);
                        if (linkedAccount == null)
                        {
                            Telemetry.TrackTrace($"{nameof(LinkedEmailAccount)} not found: {email}");
                            return false;
                        }
                        return true;
                    }
            }
        }

        #endregion

        #region CheckCrmLinkedAccountAccess

        async Task<bool> CheckCrmLinkedAccountAccessAsync(CrmDbContext dbContext, AuthorizationContext context)
        {
            var linkedAccountKey = context.Resources[nameof(AuthResource.CrmLinkedAccount)];
            var contactKey = System.Security.Claims.ClaimsPrincipalExtensions.GetContactKey(context.Principal);
            var action = context.Actions.First().Value;

            var user = await context.GetCrmUserAsync(dbContext);

            if (user == null)
            {
                return false;
            }

            switch (action)
            {
                case nameof(AuthAction.Delete):
                    {
                        var linkedAccount = user.CrmLinkedAccounts.FirstOrDefault(i => i.Key == linkedAccountKey);
                        if (linkedAccount == null)
                        {
                            Telemetry.TrackTrace($"{nameof(LinkedAccount)} not found: {linkedAccountKey}");
                            return false;
                        }

                        if (!user.HasPassword() &&
                            user.CrmLinkedAccounts.Count == 1)
                        {
                            Telemetry.TrackTrace($"Cant remove last linkedAccount. Contactkey: {user.Key}");
                            return false;
                        }
                        return true;
                    }
            }
            return true;
        }

        #endregion

        #region CheckSubscriptionAccess

        public async Task<bool> CheckSubscriptionAccessAsync(CrmUser user, CrmDbContext dbContext, string subscriptionKey)
        {
            if (user == null)
            {
                return false;
            }
            if (!user.IsLoginAllowed)
            {
                return false;
            }

            var subscription = await dbContext.Get<Subscription>(subscriptionKey);
            if (subscription == null)
            {
                return false;
            }
            if (!subscription.AllowReorder)
            {
                return false;
            }

            var owner = await subscription.Owner.Get();
            return owner.Id == user.Id;
        }

        public async Task<bool> CheckSubscriptionLicenseAccessAsync(CrmUser user, CrmDbContext dbContext, string licenseKey)
        {
            if (user == null)
            {
                return false;
            }
            if (!user.IsLoginAllowed)
            {
                return false;
            }
            var license = await dbContext.Get<CitaviLicense>(licenseKey);
            if (license == null)
            {
                return false;
            }

            var owner = await license.Owner.Get();
            return owner.Id == user.Id;
        }

        #endregion

        #region CheckAssignCitaviLicenseAccess

        public bool CheckAssignCitaviLicenseAccess(CrmUser owner, string licenseKey)
        {
            var license = owner.Licenses.FirstOrDefault(i => i.Key == licenseKey);
            if (license == null)
            {
                Telemetry.TrackTrace($"{nameof(CheckAssignCitaviLicenseAccess)}: License not found: {owner.Contact.FullName} / {licenseKey}", SeverityLevel.Warning);
                return false;
            }
            return CheckAssignCitaviLicenseAccess(owner, license);
        }
        public bool CheckAssignCitaviLicenseAccess(CrmUser owner, CitaviLicense license)
        {
            if (!owner.Contact.IsVerified.GetValueOrDefault(false))
            {
                Telemetry.TrackTrace($"{nameof(CheckAssignCitaviLicenseAccess)}: User is not verified: {owner.Contact.FullName} / {license.Key}", SeverityLevel.Warning);
                return false;
            }
            if (owner.Licenses.FirstOrDefault(i => i.Key == license.Key) == null)
            {
                Telemetry.TrackTrace($"{nameof(CheckAssignCitaviLicenseAccess)}: License not found: {owner.Contact.FullName} / {license.Key}", SeverityLevel.Warning);
                return false;
            }
            if (license.DataContractOwnerContactKey != owner.Key)
            {
                Telemetry.TrackTrace($"{nameof(CheckAssignCitaviLicenseAccess)}: User is not license-owner: {owner.Contact.FullName} / {license.Key}", SeverityLevel.Warning);
                return false;
            }
            if (license.DataContractLicenseTypeKey != LicenseType.Purchase.Key &&
                license.DataContractLicenseTypeKey != LicenseType.Subscription.Key)
            {
                Telemetry.TrackTrace($"{nameof(CheckAssignCitaviLicenseAccess)}: Wrong licenseType: {owner.Contact.FullName} / {license.Key} / {license.DataContractLicenseTypeCode}", SeverityLevel.Warning);
                return false;
            }
            if (license.DataContractPricingKey != Pricing.AcademicNonprofit.Key &&
                license.DataContractPricingKey != Pricing.Standard.Key)
            {
                Telemetry.TrackTrace($"{nameof(CheckAssignCitaviLicenseAccess)}: Wrong pricing: {owner.Contact.FullName} / {license.Key} / {license.DataContractPricingCode}", SeverityLevel.Warning);
                return false;
            }
            return true;
        }

        #endregion

        #region CheckCampusContractOrganizationSettingsAccess

        public async Task<bool> CheckCampusContractOrganizationSettingsAccessAsync(CrmDbContext dbContext, AuthorizationContext context)
        {
            var user = await context.GetCrmUserAsync(dbContext);
            return CheckCampusContractOrganizationSettingsAccess(user, context);
        }

        bool CheckCampusContractOrganizationSettingsAccess(CrmUser user, AuthorizationContext context)
        {
            var campusContractKey = context.Resources[nameof(AuthResource.CampusContractKey)];

            if (user == null)
            {
                Telemetry.TrackTrace($"User is null", SeverityLevel.Warning);
                return false;
            }
            if (!user.Contact.IsLoginAllowed.GetValueOrDefault(false))
            {
                Telemetry.TrackTrace($"{nameof(CheckCampusContractOrganizationSettingsAccessAsync)}: User login allowed false: {user.Contact.FullName} ", SeverityLevel.Warning);
                return false;
            }
            if (!user.Contact.IsVerified.GetValueOrDefault(false))
            {
                Telemetry.TrackTrace($"{nameof(CheckCampusContractOrganizationSettingsAccessAsync)}: User is not verified: {user.Contact.FullName} ", SeverityLevel.Warning);
                return false;
            }

            var campusContract = CrmCache.CampusContracts.Where(i => i.Key == campusContractKey).FirstOrDefault();
            if (campusContract == null)
            {
                Telemetry.TrackTrace($"CampusContract not found: {campusContractKey}", SeverityLevel.Warning);
                return false;
            }

            var license = user.Licenses.FirstOrDefault(i => i.DataContractCampusContractKey == campusContractKey);
            if (license == null)
            {
                Telemetry.TrackTrace($"{nameof(CheckCampusContractOrganizationSettingsAccess)}: AllowOnlineCredentialsExport is false: Contact: {user.Key} / CampusContract: {campusContractKey}", SeverityLevel.Warning);
                return false;
            }
            if (!license.IsVerified)
            {
                Telemetry.TrackTrace($"{nameof(CheckCampusContractOrganizationSettingsAccess)}: License is not verified: Contact: {user.Key} / CampusContract: {campusContractKey} / License: {license.Key}", SeverityLevel.Warning);
                return false;
            }

            var action = context.Actions.First().Value;

            switch (action)
            {
                case nameof(AuthAction.Create):
                case nameof(AuthAction.Update):
                case nameof(AuthAction.Delete):
                    {
                        foreach (var lic in user.Licenses.Where(i => i.DataContractCampusContractKey == campusContractKey && i.IsOrganizationSettingsAdmin))
                        {
                            if (lic.IsVerified && lic.IsOrganizationSettingsAdmin)
                            {
                                return true;
                            }
                        }
                    }
                    return false;

                case nameof(AuthAction.Merge):
                    return false;

                case nameof(AuthAction.Read):
                    return true;

            }

            return false;
        }

        #endregion
    }
}
