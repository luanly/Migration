using System.ComponentModel;

namespace SwissAcademic.Azure
{
    #region ChangeProjectRoleResult

    public enum ChangeProjectRoleResult
    {
        Ok,
        Denied,
        Error,
        ProjectRoleNotExists,
        CannotChangeOwnerProjectRole,
    }

    #endregion

    #region ConfirmProjectInvitationResult

    public enum ConfirmProjectInvitationResult
    {
        Error,
        OK,
        ProjectInvitationNotFound,
        UserIsNotVerified
    }

    #endregion

    #region DataCenter

    public enum DataCenter
    {
        //GermanyCentral = 1,
        [Description("euw")]
        WestEurope = 2,
        //[Description("use")]
        //EastUS = 3,
        [Description("sea")]
        SoutheastAsia = 4,
        [Description("usc")]
        CentralUS = 5
    }

    #endregion

    #region InviteUserToProjectResult

    public enum InviteUserToProjectResult
    {
        Ok,
        Denied,
        InvalidEmailAddress,
        Error,
        ProjectNotExists,
        ProjectRoleAlreadyExists,
    }

    #endregion

    #region RemoveUserFromProjectResult

    public enum RemoveUserFromProjectResult
    {
        Ok,
        Denied,
        Error,
        ProjectRoleNotExists,
        CannotRemoveOwnerProjectRole,
    }

    #endregion

    #region ProjectRoleType

    public enum ProjectRoleType
    {
        Reader = 0,
        Author = 10,
        Manager = 20,
        Owner = 30
    }

    #endregion

    #region SaveErrorCode

    public enum SaveErrorCode
    {
        Unknown,
        ChangesetMismatch,
        ProjectLocked
    }

    #endregion
}
