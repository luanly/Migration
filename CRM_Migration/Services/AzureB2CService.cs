using CRM_Migration.Models;
using CRM_Migration.Utils;
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using SwissAcademic.Crm.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CRM_Migration.Services
{
    public class AzureB2CService
    {
        static GraphServiceClient Client;

        public AzureB2CService()
        {
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(Environment.GetEnvironmentVariable("ClientId"))
                                                                                                                   .WithTenantId(Environment.GetEnvironmentVariable("TenantId"))
                                                                                                                   .WithClientSecret(Environment.GetEnvironmentVariable("ClientSecret"))
                                                                                                                   .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            Client = new GraphServiceClient(authProvider);
            Client.HttpProvider.OverallTimeout = TimeSpan.FromHours(1);
        }
        public static void Initialize()
        {
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(Environment.GetEnvironmentVariable("ClientId"))
                                                                                                                   .WithTenantId(Environment.GetEnvironmentVariable("TenantId"))
                                                                                                                   .WithClientSecret(Environment.GetEnvironmentVariable("ClientSecret"))
                                                                                                                   .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider(confidentialClientApplication);
            Client = new GraphServiceClient(authProvider);
            Client.HttpProvider.OverallTimeout = TimeSpan.FromHours(1);
        }

        public static async Task MigrateUserToB2C(CRMUser user)
        {
            if (!StringUtil.IsValidEmail(user.Email) || string.IsNullOrEmpty(user.Key)) throw new ContactNotFoundException("No Contact found");
            else
            {
                var azureUser = CreateNewB2CUser(user);

                var newUser = await Client.Users.Request().AddAsync(azureUser);

                if (!user.Key.StartsWith("ath0"))
                {
                    user.AzureObjectId = newUser.Id;

                    // Get User from CRM and update AzureB2CId
                    var crmDbContext = new CrmDbContext();
                    var crmContact = await crmDbContext.GetCrmEntityByKeyAsync(user.Key);
                    var crmContactId = crmContact?.Id.ToString();

                    if (!string.IsNullOrEmpty(crmContactId))
                    {
                        await CrmWebApi.UpdateProperty("contacts", crmContactId, "new_azureb2cid", newUser.Id);
                    }
                }
            }
        }

        private static User CreateNewB2CUser(CRMUser user)
        {
            var azureUser = new User
            {
                Department = "Deletable", //Mark for bulk delete
                Id = user.Key,
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

            azureUser.AdditionalData = new Dictionary<string, object>();
            azureUser.AdditionalData[$"extension_c8de0fa3d9124ac089b9c14272f8c000_termsOfUse"] = "privacy_policy,term_and_conditions";
            azureUser.AdditionalData[$"extension_c8de0fa3d9124ac089b9c14272f8c000_requiresMigration"] = true;

            var identities = new List<ObjectIdentity>();

            if (user.LinkedEmailAccounts.Any())
            {
                foreach (var linkedEmailAccount in user.LinkedEmailAccounts)
                {
                    if (!identities.Select(i => i.IssuerAssignedId).Contains(linkedEmailAccount))
                    {
                        identities.Add(new ObjectIdentity
                        {
                            SignInType = "emailAddress",
                            Issuer = Environment.GetEnvironmentVariable("Issuer"),
                            IssuerAssignedId = linkedEmailAccount,
                        });
                    }
                }
            }

            if (user.LinkedAccounts.Any())
            {
                foreach (var linkedAccount in user.LinkedAccounts)
                {
                    if (string.IsNullOrEmpty(linkedAccount.IdentityProviderId) || string.IsNullOrEmpty(linkedAccount.NameIdentifier)) continue;

                    if (!identities.Where(i => i.SignInType == "federated").Select(i => i.IssuerAssignedId).Contains(linkedAccount.NameIdentifier))
                    {
                        identities.Add(new ObjectIdentity
                        {
                            SignInType = "federated",
                            Issuer = linkedAccount.IdentityProviderId,
                            IssuerAssignedId = linkedAccount.NameIdentifier
                        });
                    }
                }
            }

            azureUser.Identities = identities;
            return azureUser;
        }

    }
}
