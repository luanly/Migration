using Newtonsoft.Json.Linq;
using SwissAcademic.Security;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    internal static class SignatureUtility
    {
        #region LinkedEmailAccount

        internal static async Task<string> CreateLinkedEmailAccountSignature(CrmUser user, string email)
        {
            var si = CreateLinkedEmailAccountSignatureInfo(user, email, PasswordGenerator.LicenseKey.Generate());

            await SignatureCache.AddAsync(si, DateTimeOffset.UtcNow.AddMinutes(30));

            return si.Signature;
        }

        static SignatureInfo CreateLinkedEmailAccountSignatureInfo(CrmUser user, string email, string signature)
        {
            var token = string.Concat(user.Id.ToString(), email, signature).SignWithCertificate(KnownCertificate.CitaviWeb, HashAlgorithmName.SHA256);

            return new SignatureInfo
            {
                Token = token,
                Signature = signature,
            };
        }

        internal static async Task<bool> VerifyLinkedEmailAccountSignature(CrmUser user, string email, string signature)
        {
            var si = await SignatureCache.GetAndDeleteAsync(signature);

            if(si == null)
			{
                return false;
			}

            var expectedSignatureInfo = CreateLinkedEmailAccountSignatureInfo(user, email, signature);
            return expectedSignatureInfo.Token.Equals(si.Token, StringComparison.InvariantCulture);
        }


    #endregion

}
}
