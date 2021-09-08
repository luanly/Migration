using System.Runtime.Serialization;

namespace SwissAcademic.Azure
{
    [DataContract]
    public class InviteUserToProjectInfo
    {
        [DataMember]
        public string ContactKey { get; set; }
        [DataMember]
        public string ContactName { get; set; }
        [DataMember]
        public string ContactEmailAddress { get; set; }
        [DataMember]
        public InviteUserToProjectResult Result { get; set; }
        [DataMember]
        public ProjectRoleType ProjectRole { get; set; }
    }
}
