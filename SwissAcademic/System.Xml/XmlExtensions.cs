using System.IO;
using System.Text;
using System.Threading.Tasks;
namespace System.Xml
{
    #region XmlUtility
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Provides utility methods for XmlReader. </summary>
    ///
    /// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public static class XmlReaderExtensions
    {
        #region FragmentSettings

        public static readonly XmlReaderSettings FragmentSettings = new XmlReaderSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment
        };

        #endregion
       
        #region ReadToElementAsync

        public static async Task<bool> ReadToElementAsync(this XmlReader r)
        {
            while (await r.ReadAsync()) if (r.NodeType == XmlNodeType.Element) return true;

            return false;
        }

        #endregion
    }

    public static class XmlWriterExtensions
    {
        #region FragmentSettings

        public static readonly XmlWriterSettings FragmentSettings = new XmlWriterSettings
        {
            ConformanceLevel = ConformanceLevel.Auto,
            Indent = true,
            OmitXmlDeclaration = true
        };

        #endregion
    }

    #endregion
}
