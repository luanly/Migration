using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public class ReferenceTokenStore
		:
		DefaultGrantStore<Token>,
		IReferenceTokenStore
	{
		#region Kostruktor

		public ReferenceTokenStore(
			IPersistedGrantStore store,
			IPersistentGrantSerializer serializer,
			IHandleGenerationService handleGenerationService,
			ILogger<ReferenceTokenStore> log)
			:
			base(IdentityServerConstants.PersistedGrantTypes.ReferenceToken, store, serializer, handleGenerationService, log)
		{
			
		}

		#endregion

		#region Methoden

		#region KeyToHash

		internal string KeyToHash(string value) => GetHashedKey(value);

		#endregion

		#region GetItemAsync

		protected override Task<Token> GetItemAsync(string key)
		{
			return GetReferenceTokenAsync(key);
		}

		#endregion

		#region GetReferenceTokenAsync

		public async Task<Token> GetReferenceTokenAsync(string handle)
		{
			return await base.GetItemAsync(handle);
		}

		#endregion

		#region RemoveReferenceTokenAsync

		public Task RemoveReferenceTokenAsync(string handle)
		{
			return RemoveItemAsync(handle);
		}

		#endregion

		#region RemoveReferenceTokensAsync

		public Task RemoveReferenceTokensAsync(string subjectId, string clientId)
		{
			return RemoveAllAsync(subjectId, clientId);
		}

		#endregion

		#region StoreReferenceTokenAsync

		public Task<string> StoreReferenceTokenAsync(Token token)
		{
			return CreateItemAsync(token, token.ClientId, token.SubjectId, token.SessionId, token.Description, token.CreationTime, token.Lifetime);
		}

		#endregion

		#endregion
	}
}
