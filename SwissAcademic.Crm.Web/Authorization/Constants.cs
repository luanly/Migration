namespace SwissAcademic.Crm.Web.Authorization
{
    public class AuthAction
        :
        SwissAcademic.Authorization.AuthAction
    {
        #region Constructors

        private AuthAction(string key, string value)
            : base(key, value)
        {
        }

        #endregion

        public static AuthAction Create { get; } = new AuthAction("ActionType", "Create");
        public static AuthAction Delete { get; } = new AuthAction("ActionType", "Delete");
        public static AuthAction Merge { get; } = new AuthAction("ActionType", "Merge");
        public static AuthAction Read { get; } = new AuthAction("ActionType", "Read");
        public static AuthAction Update { get; } = new AuthAction("ActionType", "Update");
    }

    public class AuthResource
        :
        SwissAcademic.Authorization.AuthResource
    {
        #region Constructors

        private AuthResource(string key, string value)
            : base(key, value)
        {
        }

        #endregion

        #region Methods

        public static AuthResource ContainerName(string value)
        {
            return new AuthResource("ContainerName", value);
        }

        public static AuthResource CampusContractKey(string value)
        {
            return new AuthResource("CampusContractKey", value);
        }

        public static AuthResource ContactKey(string value)
        {
            return new AuthResource("ContactKey", value);
        }

        public static AuthResource ClientVersion(string value)
        {
            return new AuthResource(MessageKey.ClientVersion, value);
        }

        public static AuthResource ProjectKey(string value)
        {
            return new AuthResource("ProjectKey", value);
        }

        public static AuthResource ProjectRoleKey(string value)
        {
            return new AuthResource("ProjectRoleKey", value);
        }

        public static AuthResource LinkedEmailAccount(string email)
        {
            return new AuthResource("CrmLinkedEmailAccount", email);
        }

        public static AuthResource LinkedAccount(string value)
        {
            return new AuthResource("CrmLinkedAccount", value);
        }

        public static AuthResource Subscription(string subscriptionKey)
        {
            return new AuthResource("CrmSubscription", subscriptionKey);
        }

        public static AuthResource CrmContact { get; } = new AuthResource("ResourceType", "CrmContact");
        public static AuthResource CrmProjectEntry { get; } = new AuthResource("ResourceType", "CrmProjectEntry");
        public static AuthResource CrmProjectRole { get; } = new AuthResource("ResourceType", "CrmProjectRole");
        public static AuthResource CrmLinkedEmailAccount { get; } = new AuthResource("ResourceType", "CrmLinkedEmailAccount");
        public static AuthResource CrmLinkedAccount { get; } = new AuthResource("ResourceType", "CrmLinkedAccount");
        public static AuthResource CrmSubscription { get; } = new AuthResource("ResourceType", "CrmSubscription");
        public static AuthResource CrmCampusOrganizationSetting { get; } = new AuthResource("ResourceType", "CrmCampusOrganizationSetting");
        public static AuthResource ProjectContainer { get; } = new AuthResource("ResourceType", "ProjectContainer");
        public static AuthResource ProjectData { get; } = new AuthResource("ResourceType", "ProjectData");
        public static AuthResource ProjectSettings { get; } = new AuthResource("ResourceType", "ProjectSettings");
        public static AuthResource ProjectUserSettings { get; } = new AuthResource("ResourceType", "ProjectUserSettings");
        public static AuthResource TemporaryContainerRead { get; } = new AuthResource("ResourceType", "TemporaryContainerRead");
        public static AuthResource TemporaryContainerWrite { get; } = new AuthResource("ResourceType", "TemporaryContainerWrite");
        public static AuthResource UserContainer { get; } = new AuthResource("ResourceType", "UserContainer");
        public static AuthResource UserSettings { get; } = new AuthResource("ResourceType", "UserSettings");

        #endregion
    }
}
