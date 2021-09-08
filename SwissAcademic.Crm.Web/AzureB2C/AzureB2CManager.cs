using Microsoft.Graph;
using Microsoft.Graph.Auth;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public class AzureB2CManager
	{
		private CrmDbContext _dbContext;
		private IGraphServiceClient _client;
		private string _tenantId;

		public AzureB2CManager(CrmDbContext dbContext, IGraphServiceClient client)
        {
			_dbContext = dbContext;
			_client = client;
			_tenantId = (client.AuthenticationProvider as ClientCredentialProvider).ClientApplication.AppConfig.TenantId;
		}

		public async Task MigrateUser(string email)
		{
			if (string.IsNullOrEmpty(email))
            {
				var azure_user = new User
				{
					GivenName = "Long",
					Surname = "Nguyen",
					DisplayName = "Long Nguyen",
					PreferredLanguage = "en",
					PasswordProfile = new PasswordProfile
					{
						Password = Guid.NewGuid().ToString(),
						ForceChangePasswordNextSignIn = false
					},
					PasswordPolicies = "DisablePasswordExpiration, DisableStrongPassword"
				};

				var identities = new List<ObjectIdentity>();
				identities.Add(new ObjectIdentity
				{
					SignInType = "emailAddress",
					Issuer = "qsrulurudev.onmicrosoft.com",
					IssuerAssignedId = "longtestctv01@yopmail.com"
				});
				identities.Add(new ObjectIdentity
				{
					SignInType = "emailAddress",
					Issuer = "qsrulurudev.onmicrosoft.com",
					IssuerAssignedId = "longtestctv02@yopmail.com"
				});

				azure_user.Identities = identities;

				azure_user.AdditionalData = new Dictionary<string, object>();
				//azure_user.AdditionalData[$"extension_19b3474580974454b3ecd8540746161f_citavikey"] = user.Key;
				azure_user.AdditionalData[$"extension_c8de0fa3d9124ac089b9c14272f8c000_requiresMigration"] = true;

				try
				{
					await _client.Users.Request().AddAsync(azure_user);
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
            else
            {
				var user = await _dbContext.GetByEmailAsync(email);
				await MigrateUser(user);
			}
		}
		public async Task MigrateUser(CrmUser user)
		{
			var azure_user = new User
			{
				GivenName = user.Contact.FirstName,
				Surname = user.Contact.LastName,
				DisplayName = user.Contact.FullName,
				PasswordProfile = new PasswordProfile
				{
					Password = Guid.NewGuid().ToString(),
					ForceChangePasswordNextSignIn = false
				},
				PasswordPolicies = "DisablePasswordExpiration, DisableStrongPassword"
			};

			var identities = new List<ObjectIdentity>();
			foreach (var linkedEmailAccount in user.CrmLinkedEMailAccounts)
			{
				identities.Add(new ObjectIdentity
				{
					SignInType = "emailAddress",
					Issuer = _tenantId,
					IssuerAssignedId = linkedEmailAccount.Email,
				});
			}
			
			foreach (var linkedAccount in user.CrmLinkedAccounts)
			{
				if (IdentityProviders.IsShibboleth(linkedAccount.IdentityProviderId))
				{
					continue;
				}

				identities.Add(new ObjectIdentity
				{
					SignInType = "federated",
					Issuer = linkedAccount.IdentityProviderId,
					IssuerAssignedId = linkedAccount.NameIdentifier
				});
			}

			azure_user.Identities = identities;

			//azure_user.AdditionalData = new Dictionary<string, object>();
			//azure_user.AdditionalData[$"extension_19b3474580974454b3ecd8540746161f_citavikey"] = user.Key;
			//azure_user.AdditionalData[$"extension_19b3474580974454b3ecd8540746161f_requiresMigration"] = true;

			try
            {
				await _client.Users.Request().AddAsync(azure_user);
			}
			catch(Exception ex)
            {
				throw ex;
            }
		}

		public async Task AddLinkEmail(string userId, string email)
        {
			var objectIdentity = new ObjectIdentity
			{
				SignInType = "emailAddress",
				Issuer = _tenantId,
				IssuerAssignedId = email
			};
			await AddIdentity(userId, objectIdentity);
        }

		public async Task AddLinkAccount(string userId, string issuer, string issuerAssignedId)
		{
			var objectIdentity = new ObjectIdentity
			{
				SignInType = "federated",
				Issuer = GetIssuerName(issuer),
				IssuerAssignedId = issuerAssignedId
			};
			await AddIdentity(userId, objectIdentity);
		}

		public async Task RemoveLinkEmail(string userId, string email)
		{
			var objectIdentity = new ObjectIdentity
			{
				IssuerAssignedId = email
			};
			await RemoveIdentity(userId, objectIdentity);
		}

		public async Task RemoveLinkAccount(string userId, string issuer, string issuerAssignedId)
		{
			var objectIdentity = new ObjectIdentity
			{
				Issuer = GetIssuerName(issuer),
				IssuerAssignedId = issuerAssignedId
			};
			await RemoveIdentity(userId, objectIdentity);
		}

		private async Task RemoveIdentity(string userId, ObjectIdentity objectIdentity)
		{
			if (objectIdentity == null) return;

			var identities = await GetUserUserIdentities(userId);
			if (identities == null) return;

			identities = identities.Where(i => i.Issuer != objectIdentity.Issuer && i.IssuerAssignedId != objectIdentity.IssuerAssignedId).ToList();

			await UpdateUserIdentities(userId, identities);
		}

		private async Task AddIdentity(string userId, ObjectIdentity objectIdentity)
        {
			if (objectIdentity == null) return;

			var identities = await GetUserUserIdentities(userId);
			if (identities == null) return;

			identities.Add(objectIdentity);

			await UpdateUserIdentities(userId, identities);
		}

		private async Task UpdateUserIdentities(string userId, IEnumerable<ObjectIdentity> identities)
        {
			var updatedUser = new User
			{
				Identities = identities
			};
			await _client.Users[userId].Request().UpdateAsync(updatedUser);
		}

		private async Task<IList<ObjectIdentity>> GetUserUserIdentities(string userId)
        {
			var user = await _client.Users[userId].Request()
						.Select(o => new { o.Id, o.Identities })
						.GetAsync();
			if (user == null || user.Identities == null) return null;
			// Need to re-create the existing identities since Graph client rejects existing items in requests
			return user.Identities.Select(o => new ObjectIdentity
			{
				SignInType = o.SignInType,
				Issuer = o.Issuer,
				IssuerAssignedId = o.IssuerAssignedId
			}).ToList();
		}

		private string GetIssuerName(string issuer)
        {
			if (issuer.Equals("facebook", StringComparison.OrdinalIgnoreCase)) return "facebook.com";
			if (issuer.Equals("google", StringComparison.OrdinalIgnoreCase)) return "google.com";
			return issuer;
		}
	}
}
