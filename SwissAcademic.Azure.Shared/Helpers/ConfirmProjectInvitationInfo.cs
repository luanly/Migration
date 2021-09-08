namespace SwissAcademic.Azure
{
    public class ConfirmProjectInvitationInfo
    {
        public string ContactEmail { get; set; }
        public string ContactKey { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }
        public string ProjectRoleKey { get; set; }
        public ConfirmProjectInvitationResult Result { get; set; }
    }
}
