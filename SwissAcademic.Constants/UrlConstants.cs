using System.ComponentModel;
using System.Configuration;

namespace SwissAcademic
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UrlConstants
    {
        // Base
        public static string Authority => ConfigurationManager.AppSettings["Authority"];
        public static string BackOffice => ConfigurationManager.AppSettings["BackOfficeUrl"];

        public static string CitaviPortal => ConfigurationManager.AppSettings["CitaviPortalUrl"];
        public static string CitaviPortalCheckout => ConfigurationManager.AppSettings["CitaviPortalCheckoutUrl"];


        public const string Api = "api";

        public const string MobileAppScheme = "com.citavi.citavi://";

        // Identity
        public const string AblyAuth = "ablyauth";
        public const string Account = "account";
        public const string Login = "login";
        public const string Authorize = "authorize";
        public const string DeviceAuthorization = "deviceauthorization";
        public const string Campus = "campus";
        public const string EndLogin = "endlogin";
        public const string EmailSentConfirmation = "emailSentConfirmation";
        public const string Connect = "connect";
        public const string Web = "p";
        public const string Identity = "identity";
        public const string SilentRenew = "_silentrenew.html";
        public const string Token = "token";
        public const string IdentityProviderLogin = "external";
        public const string IdentityProviderCallback = "signin-";
        public const string Registration = "register";
        public const string PasswordReset = "passwordReset";
        public const string Shop = "shop";
        public const string CitaviSpace = "citavispace";
        public const string ShibbolethLogin = "shibbolethlogin";
        public const string ShibbolethLoginIdp = "shibbolethloginidp";

        public const string DesktopClientCustomURILoginRedirect = "citavi://oauthlogincallback";


        public const string ShibbolethPersonIdMissing = "/p/Error?isShibbolethError=true";
        public const string Start = "start";

        public const string HasCampusBenifit = "HasCampusBenifit";

        public const string ExternalClientStartPage = "ExternalClient";

        // Project
        public const string ChangeProjectRoles = "ChangeProjectRoles";
        public const string InviteUsersToProject = "InviteUsersToProject";
        public const string Projects = "projects";
        public const string ProjectInfo = "GetProjectInfo";
        public const string GetProjectMinClientVersion = "GetProjectMinClientVersion";
        public const string GetProjectDetails = "GetProjectDetails";
        public const string ProjectRole = "GetProjectRole";
        public const string ProjectRoles = "GetProjectRoles";
        public const string GetUser = "getUser";
        public const string GetContactInfos = "GetContactInfos";
        public const string GetProjectMembers = "GetProjectMembers";
        public const string GetProjectMember = "GetProjectMember";
        public const string GetProjectSize = "GetProjectSize";
        public const string GetProjectDataCenter = "GetProjectDataCenter";
        public const string RemoveUserFromProject = "RemoveUserFromProject";
        public const string RemoveUsersFromProject = "RemoveUsersFromProject";
        public const string ResendProjectInvitationEMails = "ResendProjectInvitationEMails";
        public const string UpdateProjectDetails = "UpdateProjectDetails";


        public const string AddCloudProjectUser = "addCloudProjectUser";
        public const string CancelWorkerRoleJob = "cancelWorkerRoleJob";
        public const string CheckCloudSearchVersion = "CheckCloudSearchVersion";
        public const string CopyAttachmentBlob = "copyAttachmentBlob";
        public const string CreateProject = "createProject";
        public const string CopyProject = "CopyProject";
        public const string CopyOrMove = "CopyOrMove";
        public const string ConvertToUtc = "ConvertToUtc";
        public const string DeleteProject = "deleteProject";
        public const string DeleteRecycleBinItems = "DeleteRecycleBinItems";
        public const string DesktopCloudUserLookups = "DesktopCloudUserLookups";
        public const string DownloadFullText = "DownloadFullText";
        public const string FixStaticIds = "FixStaticIds";
        public const string GetActiveUsers = "GetActiveUsers";
        public const string GetFullTextLocations = "GetFullTextLocations";
        public const string GetFullTextExtractionStatus = "GetFullTextExtractionStatus";
        public const string GetIsFirstDesktopLogin = "GetIsFirstDesktopLogin";
        public const string GetTemporaryContainer = "getTemporaryContainer";
        public const string GetRecycleBinInfos = "GetRecycleBinInfos";
        public const string ProjectAccessTokens = "ProjectAccessTokens";
        public const string ProjectDownload = "download";
        public const string Upload = "upload";
        public const string UploadFullText = "UploadFullText";
        public const string ProjectUploadEnd = "uploadEnd";
        public const string RemoveCloudProjectUser = "removeCloudProjectUser";
        public const string ResetProfileCache = "ResetProfileCache";
        public const string RestoreRecycleBinItems = "RestoreRecycleBinItems";
        public const string SaveChanges = "saveChanges";
        public const string SendChatMessage = "sendChatMessage";
        public const string SetIsFirstDesktopLogin = "SetIsFirstDesktopLogin";
        public const string StartFullTextExtraction = "StartFullTextExtraction";
        public const string GetLicensesWithExpiryDate = "GetLicensesWithExpiryDate";
        public const string GetDbServerOfflineDate = "GetDbServerOfflineDate";
        public const string SendMailConcurrentUsersExceeded = "SendMailConcurrentUsersExceeded";
        public const string SaveLargeChangeset = "SaveLargeChangeset";
        public const string OrganizationSettingsUpload = "UploadOrganizationSettingsFile";
        public const string OrganizationSettingsGetAll = "GetOrganizationSettings";
        public const string OrganizationSettingsGetByKey = "GetOrganizationSetting";
        public const string OrganizationSettingsDelete = "DeleteOrganizationSetting";
        public const string QueryLocations = "QueryLocations";
        public const string ValidateCache = "validateCache";
        public const string ValidateAccessToken = "validateAccessToken";
        public const string RevokeAccessToken = "RevokeAccessToken";

        public const string SignalR = "signalr";

        public const string ZenDeskLoginRedirect = "ZenDeskLoginSuccess";
        public const string DesktopLoginRedirect = "loginSuccess";
        public const string DesktopLoginRedirectFull = "/p/loginSuccess";
        public const string MobileClientLoginRedirect = "loginSuccess";

        //EMail
        public const string ConfirmEMailAddress = "confirmEMail/";
        public const string ConfirmEMailAddressCampusLicense = "confirmEMailCampusLicense/";
        public const string ConfirmPasswordReset = "confirmPasswordReset/";
        public const string CancelEMailAddress = "cancelEMailAddress/";

        //ProjectRole
        public const string ConfirmProjectInvitation = "confirmProjectInvitation/";
        public const string RecoverDeletedProject = "recoverDeletedProject/";

        //IdS-Endpoints
        public const string TokenRevocationEndpoint = "revocation";
        public const string UserInfoEndpoint = "userinfo";


        public const string CitaviWebsite = "https://www.citavi.com/";
        public const string CitaviEULA = "https://www.citavi.com/eula";
        public const string CitaviPrivacy = "https://www.citavi.com/privacy";
        public const string CitaviShop = "https://www.citavi.com/shop";
        public const string CitaviSupport = "https://www.citavi.com/support";
        public const string CitaviAccountInfo = "https://www.citavi.com/account";
        public const string CitaviStatus = "https://www.citavi.com/status";

        public const string CitaviDesktopAccountRegistrationFailed = "https://www.citavi.com/account-registration-failed";
        public const string CitaviDesktopAccountRegistration = "https://www.citavi.com/account-registration"; //Kommt noch Prefix -buildtype hinzu
        public const string CitaviManual = "https://www.citavi.com/manual";
        public const string CitaviCloudAlert = "https://www.citavi.com/cloud-alert";

        public const string Citavi6Redirect = "c6prg";

        public const string ImportReferences = "ImportReferences";
        public const string RecognizeText = "RecognizeText";
        public const string RecognizeReferences = "RecognizeReferences";
        public const string SendFormattedReferencesByEMail = "SendFormattedReferencesByEMail";

        public const string UseResponse = "https://citavi.useresponse.com";

        public const string ContainerRedirect = "containerRedirect";
        public const string GetRedirectUrl = "getRedirectUrl";
    }
}
