using CRM_Migration.Models;
using CRM_Migration.Utils;
using CRM_Migration.ViewModels;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using SwissAcademic.Crm.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRM_Migration.Services
{
    public class AzureB2CService
    {
        static GraphServiceClient Client;
        public static List<CRMUserViewModel> ErrorUsers;
        public static int ErrorRecords;

        public static void Initialize()
        {
            ErrorRecords = 0;

            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(Environment.GetEnvironmentVariable("ClientId"))
                                                                                                                   .WithTenantId(Environment.GetEnvironmentVariable("TenantId"))
                                                                                                                   .WithClientSecret(Environment.GetEnvironmentVariable("ClientSecret"))
                                                                                                                   .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            Client = new GraphServiceClient(authProvider);
            Client.HttpProvider.OverallTimeout = TimeSpan.FromHours(1);
        }

        public static async Task MigrateUser(CRMUser user)
        {
            try
            {
                if (!StringUtil.IsValidEmail(user.Email)) throw new ContactNotFoundException("No Contact found");
                else
                {
                    var azure_user = new User
                    {
#if DEBUG
                        Department = "Deletable", //Mark for bulk delete
#endif
                        GivenName = user.FirstName,
                        Surname = user.LastName,
                        DisplayName = (string.IsNullOrEmpty(user.FirstName) && string.IsNullOrEmpty(user.LastName)) ? "(empty)" : $"{user.FirstName} {user.LastName}",
                        PasswordProfile = new PasswordProfile
                        {
                            Password = Guid.NewGuid().ToString(),
                            ForceChangePasswordNextSignIn = false
                        },
                        PasswordPolicies = "DisablePasswordExpiration, DisableStrongPassword",
                        PreferredLanguage = user.Language
                    };

                    azure_user.AdditionalData = new Dictionary<string, object>();
                    azure_user.AdditionalData[$"extension_c8de0fa3d9124ac089b9c14272f8c000_termsOfUse"] = "privacy_policy,term_and_conditions";
                    azure_user.AdditionalData[$"extension_c8de0fa3d9124ac089b9c14272f8c000_requiresMigration"] = true;

                    var identities = new List<ObjectIdentity>();

                    if (user.LinkedEmailAccounts != null)
                    {
                        foreach (var linkedEmailAccount in user.LinkedEmailAccounts)
                        {
                            //if (!identities.Select(i => i.IssuerAssignedId).Contains(linkedEmailAccount))
                            //{
                            identities.Add(new ObjectIdentity
                            {
                                SignInType = "emailAddress",
                                Issuer = Environment.GetEnvironmentVariable("Issuer"),
                                IssuerAssignedId = linkedEmailAccount,
                            });
                            //}
                        }
                    }

                    if (user.LinkedAccounts != null)
                    {
                        foreach (var linkedAccount in user.LinkedAccounts)
                        {
                            if (string.IsNullOrEmpty(linkedAccount.IdentityProviderId) || string.IsNullOrEmpty(linkedAccount.NameIdentifier)) continue;

                            //if (!identities.Where(i => i.SignInType == "federated").Select(i => i.IssuerAssignedId).Contains(linkedAccount.NameIdentifier))
                            //{
                            identities.Add(new ObjectIdentity
                            {
                                SignInType = "federated",
                                Issuer = linkedAccount.IdentityProviderId,
                                IssuerAssignedId = linkedAccount.NameIdentifier
                            });
                            //}
                        }
                    }

                    azure_user.Identities = identities;


                    var newUser = await Client.Users.Request().AddAsync(azure_user);
                    user.AzureObjectId = newUser.Id;

                    // Get User from CRM and update AzureB2CId
                    var crmDbContext = new CrmDbContext();
                    string crmContactId = null;

                    if (user.Key != null)
                    {
                        var crmContact = await crmDbContext.GetCrmEntityByKeyAsync(user.Key);
                        crmContactId = crmContact.Id.ToString();
                    }

                    if (crmContactId != null)
                    {
                        await CrmWebApi.UpdateProperty("contacts", crmContactId, "new_azureb2cid", newUser.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                user.ErrorMessage = ex.ToString();

                if (user.LinkedEmailAccounts != null) {
                    foreach (var linkedEmail in user.LinkedEmailAccounts)
                    {
                        ErrorUsers.Add(new CRMUserViewModel
                        {
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Key = user.Key,
                            Linked = linkedEmail,
                            ErrorMessage = user.ErrorMessage,
                            Language = user.Language
                        });
                        ErrorRecords++;
                    }
                }

                if(user.LinkedAccounts != null)
                {
                    foreach (var linkedAccount in user.LinkedAccounts)
                    {
                        ErrorUsers.Add(new CRMUserViewModel
                        {
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Key = user.Key,
                            Linked = linkedAccount.NameIdentifier,
                            Language = user.Language,
                            ErrorMessage = user.ErrorMessage,
                            Provider = linkedAccount.IdentityProviderId
                        });
                        ErrorRecords++;
                    }
                }
            }
        }
    }
}
