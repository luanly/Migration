namespace SwissAcademic.Azure.Storage
{
    public class AzureProjectAccess
    {
        public AzureBlobAccess Container { get; set; }
        public AzureTableAccess ProjectSettings { get; set; }
        public AzureTableAccess ProjectUserSettings { get; set; }
    }
}
