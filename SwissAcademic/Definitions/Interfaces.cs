using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.Text;

namespace SwissAcademic
{
#if !Web
    public interface IImpersonation
        :
        IDisposable
    {
        void LogonUser(string domain, string username, string password);
    }
#endif

#if !Web
    public interface IImpersonationService
    {
        IImpersonation Create();
    }
#endif

#if !Web
    public interface IDataProtectionService
    {
        byte[] ProtectForSameProcess(string value);
        byte[] ProtectForCurrentUser(string value);

        string UnprotectForSameProcess(byte[] data);
        string UnprotectForCurrentUser(byte[] data);
    }
#endif

    public interface IGlobalizationService
    {
        CultureInfo GetDefaultCulture();
        void SetDefaultCulture(CultureInfo culture);
    }

#if !Web
    public interface IUserPrincipalService
    {
        string GetPrincipalDisplayName(string identityValue, string domainName = null, PrincipalContext ctx = null);
        string GetPrincipalDisplayName(UserPrincipal principal);
    }
#endif
}
