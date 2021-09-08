using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public class SignatureValidationException
        :
        Exception
    {
        public SignatureValidationException(CrmUser user)
            :
            base("SignatureValidation failed")
        {
            Data.Add("ContactKey", user.Key);
        }
    }
}
