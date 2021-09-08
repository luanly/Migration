using SwissAcademic.Azure;

namespace SwissAcademic.Crm.Web
{
    public class ProjectEntryCreationOptions
    {
        public string ContactKey { get; set; }
        public string ProjectKey { get; set; }
        public string ProjectName { get; set; }

        public DataCenter DataCenter { get; set; }
    }
}
