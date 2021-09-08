using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using SwissAcademic.Security;
using System.Threading.Tasks;
using SwissAcademic.KeyVaultUtils;

namespace SwissAcademic.Crm.Web
{
	//https://support.zendesk.com/hc/de/articles/203663816

	public static class ZenDeskHelper
	{
		static  SymmetricSecurityKey SecurityKey;
		public static string RedirectUrl { get; private set; }

		static string PartialLoginUrl;
		public static readonly string ZenAutority = "https://citavi.zendesk.com/";
		static readonly string ZenDeskJWTUrl = $"{ZenAutority}access/jwt?jwt=";
		public const string ZenDeskLoginCookie = "ZenDesk_Login";

		public static async Task InitializeAsync()
		{
			var key = await AzureHelper.KeyVaultClient.GetSecretAsync(KeyVaultSecrets.Secrets.ZenDesk);
			SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key));

			RedirectUrl = UrlBuilder.Combine(UrlConstants.Authority, UrlConstants.Web, UrlConstants.ZenDeskLoginRedirect);
			PartialLoginUrl = UrlBuilder.Combine(UrlConstants.Authority,
											  UrlConstants.Identity,
											  UrlConstants.Connect,
											  UrlConstants.Authorize);

			PartialLoginUrl += $"?client_id={ClientIds.ZenDesk}";
			PartialLoginUrl += $"&scope=openid";
			PartialLoginUrl += $"&response_type=id_token";
			PartialLoginUrl += $"&response_mode=form_post";
			PartialLoginUrl += $"&redirect_uri={WebUtility.UrlEncode(RedirectUrl)}";
		}

		public static string BuildLoginUrl(IHttpContextAccessor httpContextAccessor)
		{
			var loginUrl = PartialLoginUrl;

			loginUrl += $"&state={Guid.NewGuid().ToString()}";
			loginUrl += $"&nonce={Guid.NewGuid().ToString()}";

			if (httpContextAccessor.HttpContext.Request.Query.TryGetValue("return_to", out var returnTo))
			{
				loginUrl += "&return_to=" + returnTo;
				httpContextAccessor.HttpContext.Response.Cookies.Append(ZenDeskLoginCookie, returnTo);
			}

			return loginUrl;
		}

		public static string BuildRedirectUrl(CrmUser user, IHttpContextAccessor httpContextAccessor)
		{
			var redirectUrl = $"{ZenDeskJWTUrl}{CreateToken(user)}";

			if(httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(ZenDeskLoginCookie, out var returnTo))
			{
				httpContextAccessor.HttpContext.Response.Cookies.Delete(ZenDeskLoginCookie);
				redirectUrl += "&return_to=" + WebUtility.UrlEncode(returnTo);
			}

			return redirectUrl;
		}

		internal static string CreateToken(CrmUser user, bool encyrpt = true)
		{
			TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
			int timestamp = (int)t.TotalSeconds;

			var tokenHandler = new JwtSecurityTokenHandler();
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Claims = new Dictionary<string, object>
				{
					{ "jti", System.Guid.NewGuid().ToString() },
					{ "iat", timestamp },
					{ "name", user.Contact.FullName },
					{ "email", user.Email },
					{ "external_id", user.Key },
				},
				Issuer = UrlConstants.Authority,
				SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256Signature)
			};

			var token = tokenHandler.CreateToken(tokenDescriptor);
			if (!encyrpt)
			{
				return token.ToString();
			}
			return tokenHandler.WriteToken(token);
		}
	}
}
