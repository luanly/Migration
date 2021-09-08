using System;
using System.Runtime.Serialization;

namespace SwissAcademic.Azure
{
    [DataContract]
    public class CloudProjectInfo
    {
        static readonly Version CloudProjectVersion = new Version(6, 0, 0, 0);

        [DataMember]
        public Version DBVersion => CloudProjectVersion;

        [DataMember]
        public Version MinClientVersion { get; set; }
        [DataMember]
        public bool IsProjectSaveLocked { get; set; }
        [DataMember]
        public string ProjectSaveLockedBy { get; set; }
        [DataMember]
        public double? ProjectSizeInMb { get; set; }
        [DataMember]
        public long Changeset { get; set; }
        [DataMember]
        public string CloudDbName { get; set; }
    }
}
