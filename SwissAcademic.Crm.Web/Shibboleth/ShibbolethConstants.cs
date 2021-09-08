using System.Collections.Generic;

namespace SwissAcademic.Crm.Web
{
    public static class ShibbolethConstants
    {
        internal static Dictionary<string, string> MetadataUrls_Production = new Dictionary<string, string>
        {
            ["https://www.aai.dfn.de/fileadmin/metadata/dfn-aai-metadata.xml"] = "de-de",
            ["http://metadata.aai.switch.ch/metadata.switchaai.xml"] = "de-ch",
            ["http://eduid.at/md/aconet-registered.xml"] = "de-at",
            ["https://mds.edugain.org"] = "en-us",
        };

        internal static Dictionary<string, string> MetadataUrls_Test = new Dictionary<string, string>
        {
            ["https://www.aai.dfn.de/fileadmin/metadata/dfn-aai-test-metadata.xml"] = "de-de",
#if !DEBUG
            
            ["http://eduid.at/md/aconet-registered.xml"] = "de-at",
#endif
            ["http://metadata.aai.switch.ch/metadata.aaitest.xml"] = "de-ch",
        };
    }
}
