
using SwissAcademic.ApplicationInsights;
using SwissAcademic.Azure;
using SwissAcademic.Crm.Web.Authorization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class ProjectManager
    {
        #region Felder



        #endregion

        #region Konstruktor

        public ProjectManager(CrmDbContext context)
        {
            DbContext = context;
        }

        #endregion

        #region Eigenschaften

        #region DbContext

        CrmDbContext DbContext { get; set; }

        #endregion

        #endregion

        #region Methoden

        #region ChangeProjectRole

        public async Task<Dictionary<string, ChangeProjectRoleResult>> ChangeProjectRolesAsync(IPrincipal principal, IEnumerable<string> projectRoleKeys, ProjectRoleType newProjectRoleType)
        {
            var result = new Dictionary<string, ChangeProjectRoleResult>();
            foreach (var projectRoleKey in projectRoleKeys)
            {
                result.Add(projectRoleKey, await ChangeProjectRoleAsync(principal, projectRoleKey, newProjectRoleType));
            }
            return result;
        }

        public async Task<ChangeProjectRoleResult> ChangeProjectRoleAsync(IPrincipal principal, string projectRoleKey, ProjectRoleType newProjectRoleType)
        {
            var ownerOrManager = await principal.GetCrmUserAsync();
            if (ownerOrManager == null)
            {
                throw new NotSupportedException("User must be authenticated");
            }
            if (string.IsNullOrEmpty(projectRoleKey))
            {
                throw new ArgumentNullException(nameof(projectRoleKey));
            }
            if (newProjectRoleType == ProjectRoleType.Owner)
            {
                return ChangeProjectRoleResult.Denied;
            }
            try
            {
                var projectRole = await ProjectRole.Get(projectRoleKey, DbContext);
                if (projectRole == null)
                {
                    return ChangeProjectRoleResult.ProjectRoleNotExists;
                }


                var contactKey = projectRole.DataContractContactKey;
                var userToUpdate = await DbContext.GetByKeyAsync(contactKey);
                if (userToUpdate == null)
                {
                    return ChangeProjectRoleResult.ProjectRoleNotExists;
                }

                if (projectRole.ProjectRoleType == ProjectRoleType.Owner)
                {
                    return ChangeProjectRoleResult.CannotChangeOwnerProjectRole;
                }

                var ownerOrManagerProjectRole = ownerOrManager.ProjectRoles.FirstOrDefault(i => i.DataContractProjectKey == projectRole.DataContractProjectKey);
                if (ownerOrManagerProjectRole == null)
                {
                    return ChangeProjectRoleResult.ProjectRoleNotExists;
                }

                if (ownerOrManagerProjectRole.ProjectRoleType < ProjectRoleType.Manager)
                {
                    return ChangeProjectRoleResult.Denied;
                }

                projectRole = userToUpdate.ProjectRoles.FirstOrDefault(i => i.Key == projectRoleKey);

                if (projectRole == null)
                {
                    return ChangeProjectRoleResult.ProjectRoleNotExists;
                }

                projectRole.ProjectRoleType = newProjectRoleType;

                await DbContext.SaveAndUpdateUserCacheAsync(userToUpdate);
                await EmailService.SendProjectRoleChangedMailAsync(userToUpdate, projectRole, ownerOrManager);

                if (AzureHelper.Ably != null)
                {
                    await AzureHelper.Ably.Invoke(MessageKey.ProjectRoleChanged,
                        CollectionUtility.ToDictionary(
                            MessageKey.ProjectKey, projectRole.DataContractProjectKey,
                            MessageKey.ProjectRoleType, newProjectRoleType.ToString(),
                            MessageKey.ContactKey, userToUpdate.Key));
                }

                return ChangeProjectRoleResult.Ok;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                return ChangeProjectRoleResult.Error;
            }
        }

        #endregion

        #region ConfirmInvitation

        public async Task<ConfirmProjectInvitationInfo> ConfirmInvitationAsync(string confirmationKey)
         => await ConfirmInvitationAsync(null, confirmationKey);

        public async Task<ConfirmProjectInvitationInfo> ConfirmInvitationAsync(CrmUser user, string confirmationKey)
        {
            if (string.IsNullOrEmpty(confirmationKey))
            {
                throw new ArgumentNullException(nameof(confirmationKey));
            }

            await using (var tslock = new TableStorageLock(confirmationKey))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return new ConfirmProjectInvitationInfo { Result = ConfirmProjectInvitationResult.Error };
                    }
                    throw new RateLimitException();
                }

                if (user == null)
                {
                    var projectRole = await ProjectRole.GetByConfirmationKey(confirmationKey, DbContext);
                    if (projectRole == null)
                    {
                        return new ConfirmProjectInvitationInfo { Result = ConfirmProjectInvitationResult.ProjectInvitationNotFound };
                    }

                    user = await DbContext.GetByKeyAsync(projectRole.DataContractContactKey);
                }

                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user));
                }

                try
                {
                    if (!user.IsAccountVerified || !user.IsLoginAllowed)
                    {
                        return new ConfirmProjectInvitationInfo
                        {
                            ContactEmail = user.Email,
                            Result = ConfirmProjectInvitationResult.UserIsNotVerified
                        };
                    }

                    var projectRole = user.ProjectRoles.FirstOrDefault(i => i.ConfirmationKey == confirmationKey);
                    if (projectRole == null)
                    {
                        return new ConfirmProjectInvitationInfo { Result = ConfirmProjectInvitationResult.ProjectInvitationNotFound };
                    }

                    projectRole.Confirmed = true;
                    projectRole.ConfirmationKey = null;
                    projectRole.ConfirmationKeySent = null;
                    projectRole.ConfirmationKeyStorage = null;

                    await DbContext.SaveAndUpdateUserCacheAsync(user);

                    if (AzureHelper.Ably != null)
                    {
                        await AzureHelper.Ably.Invoke(MessageKey.ProjectMemberAdded,
                            CollectionUtility.ToDictionary(
                                MessageKey.ProjectKey, projectRole.DataContractProjectKey,
                                MessageKey.ContactKey, user.Key));
                    }


                    return new ConfirmProjectInvitationInfo
                    {
                        ContactKey = user.Key,
                        ProjectKey = projectRole.DataContractProjectKey,
                        ProjectName = projectRole.DataContractProjectName,
                        ProjectRoleKey = projectRole.Key,
                        Result = ConfirmProjectInvitationResult.OK
                    };
                }
                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                    return new ConfirmProjectInvitationInfo { Result = ConfirmProjectInvitationResult.Error };
                }
            }
        }

        #endregion

        #region ConfirmInvitationAfterAccountCreation

        public void ConfirmInvitationAfterAccountCreation(CrmUser invitee)
        {
            if (invitee == null)
            {
                throw new ArgumentNullException(nameof(invitee));
            }

            if (!invitee.IsAccountVerified)
            {
                return;
            }

            if (!invitee.IsLoginAllowed)
            {
                return;
            }

            var projectRoles = invitee.ProjectRoles.Where(role => !role.Confirmed).ToList();
            foreach (var projectRole in projectRoles)
            {
                projectRole.Confirmed = true;
                projectRole.ConfirmationKey = null;
                projectRole.ConfirmationKeySent = null;
                projectRole.ConfirmationKeyStorage = null;

                if (AzureHelper.Ably != null)
                {
                    AzureHelper.Ably.Invoke(MessageKey.ProjectMemberAdded,
                        CollectionUtility.ToDictionary(
                            MessageKey.ProjectKey, projectRole.DataContractProjectKey,
                            MessageKey.ContactKey, invitee.Key));
                }
            }
        }

        #endregion

        #region Create

        public async Task<ProjectEntry> CreateAsync(ProjectEntryCreationOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.ContactKey))
            {
                throw new ArgumentNullException(nameof(options.ContactKey));
            }

            if (string.IsNullOrEmpty(options.ProjectKey))
            {
                throw new ArgumentNullException(nameof(options.ProjectKey));
            }

            if (string.IsNullOrEmpty(options.ProjectName))
            {
                throw new ArgumentNullException(nameof(options.ProjectName));
            }

            var user = await DbContext.GetByKeyAsync(options.ContactKey);
            var contact = user.Contact;

            if(user.ProjectRoles.Count() > RateLimitConstants.MaxProjectsPerUser)
			{
                throw new RateLimitException();
			}

            await RateLimits.CreateProject.ExceededAsync(user);

            try
            {
                var project = DbContext.Create<ProjectEntry>();
                project.DataCenter = options.DataCenter;
                project.Key = options.ProjectKey;
                project.DataSource = AzureRegionResolver.Instance.GetDataSourceForNewProjects(project.DataCenter);
                project.Name = options.ProjectName;
                project.InitialCatalog = ConfigurationManager.AppSettings[$"InitialCatalogForNewProjects_{AzureRegionResolver.Instance.GetShortName(project.DataCenter)}"];

                var projectRole = DbContext.Create<ProjectRole>();
                projectRole.ProjectRoleType = ProjectRoleType.Owner;
                projectRole.Confirmed = true;
                projectRole.Project.Set(project);
                projectRole.Contact.Set(contact);
                user.ProjectRoles.Add(projectRole);

                await DbContext.SaveAndUpdateUserCacheAsync(user);
                await CrmCache.Projects.AddOrUpdateAsync(project);
                return project;
            }
            catch (Exception exception)
            {
                Telemetry.TrackException(exception, property1: (nameof(options.ProjectKey), options.ProjectKey + "/" + options.ContactKey + "/" + options.DataCenter));
            }
            return null;
        }

        #endregion

        #region DismissInvitation

        public async Task<bool> DismissInvitationAsync(ClaimsPrincipal principal, string confirmationKey)
         => await DismissInvitationAsync((IPrincipal)principal, confirmationKey);

        public async Task<bool> DismissInvitationAsync(IPrincipal principal, string confirmationKey)
        {
            if (string.IsNullOrEmpty(confirmationKey))
            {
                throw new ArgumentNullException(nameof(confirmationKey));
            }

            try
            {
                var projectRole = await ProjectRole.GetByConfirmationKey(confirmationKey, DbContext);
                if (projectRole == null)
                {
                    return false;
                }

                return await RemoveUserAsync(principal, projectRole.DataContractProjectKey, projectRole.DataContractContactKey) == RemoveUserFromProjectResult.Ok;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                return false;
            }
        }

        #endregion

        #region GetDeletedProjectsByContactKeyAsync

        public async Task<IEnumerable<ProjectRole>> GetDeletedProjectsByContactKeyAsync(string contactKey)
        {
            var fetchXml = FetchXmlExpression.Create<ProjectRole>(new Query.FetchXml.GetDeletedProjectByContactKey(contactKey).TransformText());
            return await DbContext.Fetch<ProjectRole>(fetchXml);
        }

        #endregion

        #region GetPendingProjectDeletions

        public async Task<IEnumerable<ProjectEntry>> GetPendingProjectDeletionsAsync()
        {
            var fetchXml = FetchXmlExpression.Create<ProjectEntry>(new Query.FetchXml.GetPendingDeleteProjects(CrmConfig.CleanUpDeletedProjectsAfter).TransformText());
            var projects = await DbContext.Fetch<ProjectEntry>(fetchXml);
            if (projects == null)
            {
                return Enumerable.Empty<ProjectEntry>();
            }

            foreach (var project in projects)
            {
                if (project.DeletedOn == null)
                {
                    throw new NotSupportedException("DeletedOn must not be null");
                }
            }
            return projects;
        }

        #endregion

        #region GetProjectMembers

        async Task<IEnumerable<(Contact, ProjectRoleType)>> GetProjectMembers(string projectKey)
        {
            var query = new Query.FetchXml.GetProjectMembers(projectKey).TransformText();
            var result = await DbContext.Fetch(FetchXmlExpression.Create<ProjectRole>(query));
            var crmSet = new CrmSet(result);
            var members = new List<(Contact, ProjectRoleType)>();

            foreach (var contact in crmSet.Contacts)
            {
                var projectRole = crmSet.ProjectRoles.Where(i => i.DataContractContactKey == contact.Key).First();
                members.Add((contact, projectRole.ProjectRoleType.GetValueOrDefault()));
            }

            return members;
        }

        #endregion

        #region InviteUser

        public async Task<IEnumerable<InviteUserToProjectInfo>> InviteUsersAsync(string projectKey, IEnumerable<string> invitee_emails, ProjectRoleType role, CrmUser inviter, string description = null)
        {
            var invites = new List<InviteUserToProjectInfo>();
            foreach (var invitee_email in invitee_emails)
            {
                invites.Add(await InviteUserAsync(projectKey, invitee_email, role, inviter, isBatchMode: true, description: description));
            }
            return invites;
        }

        public async Task<InviteUserToProjectInfo> InviteUserAsync(string projectKey, string invitee_email, ProjectRoleType role, CrmUser inviter, bool isBatchMode = false, string description = null)
        {
            await using (var tslock = new TableStorageLock(invitee_email))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return new InviteUserToProjectInfo
                        {
                            ContactEmailAddress = invitee_email,
                            Result = InviteUserToProjectResult.Denied
                        };
                    }
                    throw new RateLimitException();
                }

                if (string.IsNullOrEmpty(projectKey))
                {
                    throw new ArgumentNullException(nameof(projectKey));
                }

                if (string.IsNullOrEmpty(invitee_email))
                {
                    throw new ArgumentNullException(nameof(invitee_email));
                }

                if (inviter == null)
                {
                    throw new ArgumentNullException(nameof(inviter));
                }

                if (role == ProjectRoleType.Owner)
                {
                    throw new NotSupportedException("'Owner' is not supported");
                }

                if (!inviter.Contact.IsVerified.GetValueOrDefault())
                {
                    throw new NotSupportedException("inviter is not verified");
                }

                invitee_email = invitee_email.RemoveNonStandardWhitespace();
                invitee_email = invitee_email.ReplaceNonStandardEmailChars();

                if (!RegexUtility.IsValidEmailAddress(invitee_email))
                {
                    return new InviteUserToProjectInfo
                    {
                        ContactEmailAddress = invitee_email,
                        Result = InviteUserToProjectResult.InvalidEmailAddress
                    };
                }

                if (!await AuthorizationManager.Instance.CheckAccessAsync(inviter.Principal, AuthAction.Create, AuthResource.CrmProjectRole, AuthResource.ProjectKey(projectKey)))
                {
                    return new InviteUserToProjectInfo
                    {
                        ContactEmailAddress = invitee_email,
                        Result = InviteUserToProjectResult.Denied
                    };
                }

                var result = new InviteUserToProjectInfo();
                var project = await DbContext.Get<ProjectEntry>(projectKey);

                await RateLimits.CreateProjectRole.ExceededAsync(project);

                var invitee = await DbContext.GetByEmailAsync(invitee_email);
                if (invitee == null)
                {
                    var contact = DbContext.Create<Contact>();
                    invitee = new CrmUser(contact);
                    invitee.AddLinkedEMailAccount(invitee_email);
                }

                var existingProjectRole = invitee.ProjectRoles.FirstOrDefault(i => i.DataContractProjectKey == projectKey);
                if (existingProjectRole != null)
                {
                    if (!existingProjectRole.Confirmed)
                    {
                        var invokeSignalR = false;
                        if (existingProjectRole.ProjectRoleType != role)
                        {
                            existingProjectRole.ProjectRoleType = role;

                            if (!isBatchMode &&
                                AzureHelper.Ably != null)
                            {
                                invokeSignalR = true;
                            }
                        }

                        await SendConfirmProjectInvitationEMailAsync(invitee, existingProjectRole, project, inviter, description);
                        await DbContext.SaveAsync(invitee);

                        if (invokeSignalR)
                        {
                            await AzureHelper.Ably.Invoke(MessageKey.ProjectMemberInvited,
                                    CollectionUtility.ToDictionary(
                                        MessageKey.ProjectKey, existingProjectRole.DataContractProjectKey,
                                        MessageKey.ContactKey, invitee.Key));
                        }

                        return new InviteUserToProjectInfo
                        {
                            ContactKey = invitee.Key,
                            ContactName = invitee.Contact.FullName,
                            ContactEmailAddress = invitee_email,
                            Result = InviteUserToProjectResult.Ok,
                            ProjectRole = role
                        };
                    }
                    else
                    {
                        return new InviteUserToProjectInfo
                        {
                            ContactEmailAddress = invitee_email,
                            Result = InviteUserToProjectResult.ProjectRoleAlreadyExists
                        };
                    }
                }

                var projectRole = DbContext.Create<ProjectRole>();
                projectRole.ProjectRoleType = role;
                projectRole.Contact.Set(invitee.Contact);
                projectRole.Project.Set(project);

                invitee.ProjectRoles.Add(projectRole);

                InviteUserToProjectResult status;
                string name;
                if (invitee.Contact.EntityState == EntityState.Created)
                {
                    await SendConfirmProjectInvitationEMailForNewUserAsync(invitee, projectRole, project, inviter, description);
                    status = InviteUserToProjectResult.Ok;
                    name = invitee.Email;
                }
                else
                {
                    await SendConfirmProjectInvitationEMailAsync(invitee, projectRole, project, inviter, description);
                    status = InviteUserToProjectResult.Ok;
                    name = invitee.Contact.FullName;
                }

                await DbContext.SaveAsync(invitee);

                if (!isBatchMode &&
                    AzureHelper.Ably != null)
                {
                    await AzureHelper.Ably.Invoke(MessageKey.ProjectMemberInvited,
                        CollectionUtility.ToDictionary(
                            MessageKey.ProjectKey, projectRole.DataContractProjectKey,
                            MessageKey.ContactKey, invitee.Key));
                }

                return new InviteUserToProjectInfo
                {
                    ContactKey = invitee.Key,
                    ContactName = name,
                    ContactEmailAddress = invitee_email,
                    Result = status,
                    ProjectRole = role
                };
            }
        }

        #endregion

        #region RecoverDeletedProject

        public async Task<ProjectEntry> RecoverDeletedProjectAsync(string recoveryKey)
        {
            await using (var tslock = new TableStorageLock(recoveryKey))
            {
                if (!await tslock.TryEnter())
                {
                    if (CrmConfig.IsUnittest)
                    {
                        return null;
                    }
                    throw new RateLimitException();
                }

                var projectEntry = await DbContext.Get<ProjectEntry>(ProjectEntryPropertyId.RecoveryKey, recoveryKey);

                if (projectEntry == null)
                {
                    return null;
                }

                projectEntry.DeletedOn = null;
                projectEntry.RecoveryKey = null;
                await DbContext.SaveAsync();
                await CrmCache.Projects.RemoveAsync(projectEntry);

                return projectEntry;
            }
        }

        #endregion

        #region RemoveUser
        public async Task<Dictionary<string, RemoveUserFromProjectResult>> RemoveUsersAsync(IPrincipal principal, string projectKey, IEnumerable<string> contactKeys)
        {
            var remover = await principal.GetCrmUserAsync();
            return await RemoveUsersAsync(remover, projectKey, contactKeys);
        }

        public async Task<Dictionary<string, RemoveUserFromProjectResult>> RemoveUsersAsync(CrmUser remover, string projectKey, IEnumerable<string> contactKeys)
        {
            var result = new Dictionary<string, RemoveUserFromProjectResult>();
            foreach (var contactKey in contactKeys)
            {
                result.Add(contactKey, await RemoveUserAsync(remover, projectKey, contactKey));
            }
            return result;
        }

        public async Task<RemoveUserFromProjectResult> RemoveUserAsync(IPrincipal principal, string projectKey, string contactKey)
        {
            var user = await principal.GetCrmUserAsync();
            return await RemoveUserAsync(user, projectKey, contactKey);
        }

        public async Task<RemoveUserFromProjectResult> RemoveUserAsync(CrmUser remover, string projectKey, string contactKey)
        {
            if (string.IsNullOrEmpty(projectKey))
            {
                throw new ArgumentNullException(nameof(projectKey));
            }

            if (string.IsNullOrEmpty(contactKey))
            {
                throw new ArgumentNullException(nameof(contactKey));
            }

            if (remover == null)
            {
                throw new NotSupportedException("User must be authenticated");
            }

            try
            {
                var userToRemove = await DbContext.GetByKeyAsync(contactKey);
                if (userToRemove == null)
                {
                    return RemoveUserFromProjectResult.ProjectRoleNotExists;
                }

                var projectRole = userToRemove.ProjectRoles.FirstOrDefault(i => i.DataContractProjectKey == projectKey);
                if (projectRole == null)
                {
                    return RemoveUserFromProjectResult.ProjectRoleNotExists;
                }

                var removerProjectRole = remover.ProjectRoles.FirstOrDefault(i => i.DataContractProjectKey == projectKey);
                if (projectRole == null)
                {
                    return RemoveUserFromProjectResult.ProjectRoleNotExists;
                }

                if (projectRole.ProjectRoleType == ProjectRoleType.Owner)
                {
                    return RemoveUserFromProjectResult.CannotRemoveOwnerProjectRole;
                }

                if (removerProjectRole.ProjectRoleType < ProjectRoleType.Manager && remover.Key != userToRemove.Key)
                {
                    return RemoveUserFromProjectResult.Denied;
                }

                if (AzureHelper.Ably != null)
                {
                    await AzureHelper.Ably.Invoke(MessageKey.ProjectMemberRemoved,
                        CollectionUtility.ToDictionary(
                            MessageKey.ProjectKey, projectRole.DataContractProjectKey,
                            MessageKey.ContactKey, userToRemove.Key));
                }

                userToRemove.ProjectRoles.Remove(projectRole);
                DbContext.Delete(projectRole);
                await DbContext.SaveAndUpdateUserCacheAsync(userToRemove);

                await SendRemovedFromProjectEMailAsync(userToRemove, projectRole, remover);

                return RemoveUserFromProjectResult.Ok;
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                return RemoveUserFromProjectResult.Error;
            }
        }

        #endregion

        #region Rename

        public async Task<bool> Rename(IPrincipal principal, string projectKey, string name, bool checkAccess = true)
        {
            var user = await principal.GetCrmUserAsync();
            return await Rename(user, projectKey, name, checkAccess);
        }

        public async Task<bool> Rename(CrmUser user, string projectKey, string name, bool checkAccess = true)
        {
            if (string.IsNullOrEmpty(projectKey))
            {
                throw new ArgumentNullException(nameof(projectKey));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var existingProjectRole = user.ProjectRoles.Where(i => i.DataContractProjectKey == projectKey).FirstOrDefault();
            var projectEntry = await DbContext.Get<ProjectEntry>(projectKey, ProjectEntryPropertyId.Name);

            if (checkAccess &&
                !await AuthorizationManager.Instance.CheckAccessAsync(user, AuthAction.Update, AuthResource.CrmProjectEntry, AuthResource.ProjectKey(projectKey)))
            {
                return false;
            }

            projectEntry.Name = name;
            existingProjectRole.DataContractProjectName = name;
            await DbContext.SaveAndUpdateUserCacheAsync(user);
            await CrmCache.Projects.RemoveAsync(projectEntry);

            return true;
        }

        #endregion

        #region ResendProjectInvitationEMail

        public async Task<InviteUserToProjectInfo> ResendProjectInvitationEMailAsync(IPrincipal principal, string projectKey, string contactKey)
        {
            var inviter = await principal.GetCrmUserAsync();
            if (inviter == null)
            {
                throw new NotSupportedException("User must be authenticated");
            }

            var inviterProjectRole = inviter.ProjectRoles.Where(i => i.DataContractProjectKey == projectKey).FirstOrDefault();
            if (inviterProjectRole == null)
            {
                return new InviteUserToProjectInfo { Result = InviteUserToProjectResult.Denied, ContactKey = contactKey };
            }
            if (!inviterProjectRole.Confirmed)
            {
                return new InviteUserToProjectInfo { Result = InviteUserToProjectResult.Denied, ContactKey = contactKey };
            }

            if (inviterProjectRole.ProjectRoleType < ProjectRoleType.Manager)
            {
                return new InviteUserToProjectInfo { Result = InviteUserToProjectResult.Denied, ContactKey = contactKey };
            }

            var invitee = await DbContext.GetByKeyAsync(contactKey);
            if (invitee == null)
            {
                return new InviteUserToProjectInfo { Result = InviteUserToProjectResult.Error, ContactKey = contactKey };
            }

            var projectEntry = await DbContext.Get<ProjectEntry>(projectKey);
            if (projectEntry == null)
            {
                return new InviteUserToProjectInfo { Result = InviteUserToProjectResult.ProjectNotExists, ContactKey = contactKey };
            }

            var projectRole = invitee.ProjectRoles.FirstOrDefault(i => i.DataContractProjectKey == projectKey);
            if (projectRole == null)
            {
                return new InviteUserToProjectInfo { Result = InviteUserToProjectResult.Error, ContactKey = contactKey };
            }
            if (projectRole.Confirmed)
            {
                return new InviteUserToProjectInfo { Result = InviteUserToProjectResult.ProjectRoleAlreadyExists, ContactKey = contactKey };
            }

            if (invitee.IsAccountVerified)
            {
                await SendConfirmProjectInvitationEMailAsync(invitee, projectRole, projectEntry, inviter);
            }
            else
            {
                await SendConfirmProjectInvitationEMailForNewUserAsync(invitee, projectRole, projectEntry, inviter);
            }

            await DbContext.SaveAsync(invitee);

            return new InviteUserToProjectInfo
            {
                ContactKey = invitee.Key,
                ContactName = invitee.Contact.FullName,
                ContactEmailAddress = invitee.Email,
                Result = InviteUserToProjectResult.Ok,
                ProjectRole = projectRole.ProjectRoleType.Value
            };
        }

        #endregion

        #region SendRemovedFromProjectEMailAsync

        internal Task SendRemovedFromProjectEMailAsync(CrmUser removedUser, ProjectRole projectRole, CrmUser remover)
        {
            if (removedUser == null)
            {
                throw new ArgumentNullException(nameof(removedUser));
            }

            if (projectRole == null)
            {
                throw new ArgumentNullException(nameof(projectRole));
            }

            if (remover == null)
            {
                throw new ArgumentNullException(nameof(remover));
            }

            return EmailService.SendRemovedFromProjectEMailAsync(removedUser, projectRole, remover);
        }

        #endregion

        #region SendConfirmProjectInvitationEMail

        internal Task SendConfirmProjectInvitationEMailAsync(CrmUser invitee, ProjectRole projectRole, ProjectEntry projectEntry, CrmUser inviter, string description = null)
        {
            if (invitee == null)
            {
                throw new ArgumentNullException(nameof(invitee));
            }

            if (projectRole == null)
            {
                throw new ArgumentNullException(nameof(projectRole));
            }

            if (projectEntry == null)
            {
                throw new ArgumentNullException(nameof(projectEntry));
            }

            if (inviter == null)
            {
                throw new ArgumentNullException(nameof(inviter));
            }

            projectRole.ConfirmationKeyStorage = invitee.Email;
            projectRole.ConfirmationKeySent = DateTime.UtcNow;
            projectRole.ConfirmationKey = Security.PasswordGenerator.WebKey.Generate();
            if (!string.IsNullOrEmpty(description))
            {
                description = $"{EmailFormatter.GetLanguageSpecificText(nameof(Resources.Strings.CRM_Email_ConfirmProjectInvitationDescriptionPrefix), invitee.Contact.Language)} \"{description}\"";
            }
            return EmailService.SendConfirmProjectInvitationMailAsync(invitee, inviter, projectRole, projectEntry, description);
        }

        #endregion

        #region SendConfirmProjectInvitationEMailForNewUser

        async Task SendConfirmProjectInvitationEMailForNewUserAsync(CrmUser invitee, ProjectRole projectRole, ProjectEntry projectEntry, CrmUser inviter, string description = null)
        {
            if (invitee == null)
            {
                throw new ArgumentNullException(nameof(invitee));
            }

            if (projectRole == null)
            {
                throw new ArgumentNullException(nameof(projectRole));
            }

            if (projectEntry == null)
            {
                throw new ArgumentNullException(nameof(projectEntry));
            }

            if (inviter == null)
            {
                throw new ArgumentNullException(nameof(inviter));
            }

            invitee.Contact.ChangeLanguage(inviter.Contact.Language);

            var verificationKey = await new CrmUserManager(DbContext).SetVerificationKeyForNewUserAsync(invitee);
            projectRole.ConfirmationKeyStorage = invitee.Email;
            projectRole.ConfirmationKeySent = DateTime.UtcNow;
            projectRole.ConfirmationKey = Security.PasswordGenerator.WebKey.Generate();
            if (!string.IsNullOrEmpty(description))
            {
                description = $"{EmailFormatter.GetLanguageSpecificText(nameof(Resources.Strings.CRM_Email_ConfirmProjectInvitationDescriptionPrefix), invitee.Contact.Language)} \"{description}\"";
            }
            await EmailService.SendConfirmProjectInvitationMailNewUser(invitee, inviter, projectRole, projectEntry, verificationKey, description);
        }

        #endregion

        #region SendProjectRecoveryKeyEMail

        internal Task SendProjectRecoveryKeyEMail(CrmUser owner, ProjectEntry projectEntry)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (projectEntry == null)
            {
                throw new ArgumentNullException(nameof(projectEntry));
            }

            return EmailService.SendProjectRecoveryKeyAsync(owner, projectEntry);
        }

        #endregion

        #region SetRecoveryKeyAndSendMail

        public async Task SetRecoveryKeyAndSendMail(CrmUser owner, ProjectEntry projectEntry)
        {
            projectEntry.SetRecoveryKey();
            await DbContext.SaveAsync();
            await SendProjectRecoveryKeyEMail(owner, projectEntry);
            await CrmUserCache.RemoveAsync(owner);
            await CrmCache.Projects.RemoveAsync(projectEntry);
        }

        #endregion

        #region SetDeleted

        public async Task<bool> SetDeletedAsync(IPrincipal principal, string projectKey, bool checkAccess = true)
        {
            var user = await principal.GetCrmUserAsync();
            return await SetDeletedAsync(user, projectKey, checkAccess);
        }

        public async Task<bool> SetDeletedAsync(CrmUser user, string projectKey, bool checkAccess = true)
        {
            if (user == null)
            {
                throw new NotSupportedException("User must be authenticated");
            }
            if (string.IsNullOrEmpty(projectKey))
            {
                throw new NotSupportedException($"{nameof(projectKey)} must not be null");
            }

            if (checkAccess &&
                !await AuthorizationManager.Instance.CheckAccessAsync(user, AuthAction.Delete, AuthResource.CrmProjectEntry, AuthResource.ProjectKey(projectKey)))
            {
                Telemetry.TrackTrace("ProjectManager.SetDeletedAsync CheckAccess failed.", SeverityLevel.Error);
                return false;
            }

            var projectEntry = await DbContext.Get<ProjectEntry>(projectKey);
            if (projectEntry == null)
            {
                throw new NotSupportedException($"ProjectEntry not found: {projectKey}");
            }

            foreach (var member in await GetProjectMembers(projectKey))
            {
                if (member.Item2 == ProjectRoleType.Owner)
                {
                    await CitaviSpaceCache.QueueUpdateCitaviSpace(member.Item1.Key);

                    await AzureHelper.Ably.Invoke(MessageKey.ProjectDeleted,
                           CollectionUtility.ToDictionary(
                               MessageKey.ContactKey, member.Item1.Key,
                               MessageKey.ProjectKey, projectKey,
                               MessageKey.TaskStatus, TaskStatus.RanToCompletion.ToString()));

                    continue;
                }
                try
                {
                    await EmailService.SendProjectDeletedMailAsync(user, member.Item1, projectEntry.Name);
                    if (AzureHelper.Ably != null)
                    {
                        await AzureHelper.Ably.Invoke(MessageKey.ProjectDeleted,
                            CollectionUtility.ToDictionary(
                                MessageKey.ContactKey, member.Item1.Key,
                                MessageKey.ProjectKey, projectKey));
                    }
                    await CrmUserCache.RemoveAsync(member.Item1.Key);
                }
                catch (Exception ignored)
                {
                    Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                }
            }

            await SetRecoveryKeyAndSendMail(user, projectEntry);

            return true;
        }

        #endregion

        #endregion
    }
}
