using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public static class SignatureCache
	{
		public static async Task AddAsync(SignatureInfo signature, DateTimeOffset absoluteExpiration)
		{
			await DistributedCacheStore.Instance.SetAsync(signature.Signature, signature.Token, new DistributedCacheEntryOptions
			{
				AbsoluteExpiration = absoluteExpiration
			});
		}

		public static async Task<SignatureInfo> GetAndDeleteAsync(string signature)
		{
			var salt = await DistributedCacheStore.Instance.GetAsync(signature, default);

			if(salt == null)
			{
				return null;
			}

			await DistributedCacheStore.Instance.RemoveAsync(signature);

			return new SignatureInfo
			{
				Token = Encoding.UTF8.GetString(salt),
				Signature = signature
			};
		}
	}
}
