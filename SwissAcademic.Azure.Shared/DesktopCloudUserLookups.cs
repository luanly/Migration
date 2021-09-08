using SwissAcademic.Azure.Storage;

namespace SwissAcademic.Azure
{
    public class DesktopCloudUserLookups
    {
        public AzureTableAccess UserSettingsAccess { get; set; }
        public byte[] UserImage { get; set; }
    }
}
