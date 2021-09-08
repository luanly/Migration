
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Xml
{
    public class XmlWriterSafe
        :
        IDisposable
    {
        #region Felder

        XmlWriter _innerWriter;

        #endregion

        #region Konstruktoren

        private XmlWriterSafe(Stream output, XmlWriterSettings settings)
        {
            _innerWriter = XmlWriter.Create(output, settings);
        }

        private XmlWriterSafe(string outputFileName, XmlWriterSettings settings)
        {
            _innerWriter = XmlWriter.Create(outputFileName, settings);
        }

        private XmlWriterSafe(StringBuilder output, XmlWriterSettings settings)
        {
            _innerWriter = XmlWriter.Create(output, settings);
        }

        private XmlWriterSafe(TextWriter output, XmlWriterSettings settings)
        {
            _innerWriter = XmlWriter.Create(output, settings);
        }

        private XmlWriterSafe(XmlWriter innerWriter)
        {
            _innerWriter = innerWriter;
        }

        public static XmlWriterSafe Create(Stream output, XmlWriterSettings settings = null)
        {
            return new XmlWriterSafe(output, settings);
        }

        public static XmlWriterSafe Create(string outputFileName, XmlWriterSettings settings = null)
        {
            return new XmlWriterSafe(outputFileName, settings);
        }

        public static XmlWriterSafe Create(StringBuilder output, XmlWriterSettings settings = null)
        {
            return new XmlWriterSafe(output, settings);
        }

        public static XmlWriterSafe Create(TextWriter output, XmlWriterSettings settings = null)
        {
            return new XmlWriterSafe(output, settings);
        }

        public static XmlWriterSafe Create(XmlWriter innerWriter)
        {
            return new XmlWriterSafe(innerWriter);
        }

        #endregion

        #region Eigenschaften

        #region Settings

        public XmlWriterSettings Settings
        {
            get { return _innerWriter.Settings; }
        }

        #endregion

        #region WriteState

        public WriteState WriteState
        {
            get { return _innerWriter.WriteState; }
        }

        #endregion

        #region XmlLang

        public string XmlLang
        {
            get { return _innerWriter.XmlLang; }
        }

        #endregion

        #region XmlSpace

        public XmlSpace XmlSpace
        {
            get { return _innerWriter.XmlSpace; }
        }

        #endregion

        #endregion

        #region Methoden

        #region Close

        public void Close()
        {
            Dispose();
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) _innerWriter.Dispose();
        }

        #endregion

        #region Flush

        public void Flush()
        {
            _innerWriter.Flush();
        }

        #endregion

        #region LookupPrefix

        public string LookupPrefix(string ns)
        {
            return _innerWriter.LookupPrefix(ns);
        }

        #endregion


        #region WriteAttributes

        public void WriteAttributes(XmlReader reader, bool defattr)
        {
            _innerWriter.WriteAttributes(reader, defattr);
        }

        public Task WriteAttributesAsync(XmlReader reader, bool defattr)
        {
            return _innerWriter.WriteAttributesAsync(reader, defattr);
        }

        #endregion

        #region WriteAttributeString

        public void WriteAttributeString(string localName, string value)
        {
            try
            {
                _innerWriter.WriteAttributeString(localName, value.Clean(false));
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.Fail(exception.Message);
            }
        }

        public Task WriteAttributeStringAsync(string localName, string value)
        {
            return WriteAttributeStringAsync(null, localName, null, value);
        }

        public void WriteAttributeString(string localName, string ns, string value)
        {
            _innerWriter.WriteAttributeString(localName, ns, value.Clean(false));
        }

        public Task WriteAttributeStringAsync(string localName, string ns, string value)
        {
            return WriteAttributeStringAsync(null, localName, ns, value);
        }

        public void WriteAttributeString(string prefix, string localName, string ns, string value)
        {
            _innerWriter.WriteAttributeString(prefix, localName, ns, value.Clean(false));
        }

        public Task WriteAttributeStringAsync(string prefix, string localName, string ns, string value)
        {
            return _innerWriter.WriteAttributeStringAsync(prefix, localName, ns, value.Clean(false));
        }

        #endregion

        #region WriteBase64

        public void WriteBase64(byte[] buffer, int index, int count)
        {
            _innerWriter.WriteBase64(buffer, index, count);
        }

        public Task WriteBase64Async(byte[] buffer, int index, int count)
        {
            return _innerWriter.WriteBase64Async(buffer, index, count);
        }

        #endregion

        #region WriteBinHex

        public void WriteBinHex(byte[] buffer, int index, int count)
        {
            _innerWriter.WriteBinHex(buffer, index, count);
        }

        public Task WriteBinHexAsync(byte[] buffer, int index, int count)
        {
            return _innerWriter.WriteBinHexAsync(buffer, index, count);
        }

        #endregion

        #region WriteCData(string text)

        public void WriteCData(string text)
        {
            _innerWriter.WriteCData(text.Clean(false));
        }

        public Task WriteCDataAsync(string text)
        {
            return _innerWriter.WriteCDataAsync(text.Clean(false));
        }

        #endregion

        #region WriteCharEntity

        public void WriteCharEntity(char c)
        {
            _innerWriter.WriteCharEntity(c);
        }

        public Task WriteCharEntityAsync(char c)
        {
            return _innerWriter.WriteCharEntityAsync(c);
        }

        #endregion

        #region WriteChars

        public void WriteChars(char[] buffer, int index, int count)
        {
            _innerWriter.WriteChars(buffer, index, count);
        }

        public Task WriteCharsAsync(char[] buffer, int index, int count)
        {
            return _innerWriter.WriteCharsAsync(buffer, index, count);
        }

        #endregion

        #region WriteComment

        public void WriteComment(string text)
        {
            _innerWriter.WriteComment(text.Clean(false));
        }

        public Task WriteCommentAsync(string text)
        {
            return _innerWriter.WriteCommentAsync(text.Clean(false));
        }

        #endregion

        #region WriteDocType

        public void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            _innerWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public Task WriteDocTypeAsync(string name, string pubid, string sysid, string subset)
        {
            return _innerWriter.WriteDocTypeAsync(name, pubid, sysid, subset);
        }

        #endregion

        #region WriteElementString

        public void WriteElementString(string localName, string value)
        {
            _innerWriter.WriteElementString(localName, value.Clean(false));
        }

        public Task WriteElementStringAsync(string localName, string value)
        {
            return WriteElementStringAsync(null, localName, null, value);
        }

        public void WriteElementString(string localName, string ns, string value)
        {
            _innerWriter.WriteElementString(localName, ns, value.Clean(false));
        }

        public Task WriteElementStringAsync(string localName, string ns, string value)
        {
            return WriteElementStringAsync(null, localName, ns, value);
        }

        public void WriteElementString(string prefix, string localName, string ns, string value)
        {
            _innerWriter.WriteElementString(prefix, localName, ns, value.Clean(false));
        }

        public Task WriteElementStringAsync(string prefix, string localName, string ns, string value)
        {
            return _innerWriter.WriteElementStringAsync(prefix, localName, ns, value.Clean(false));
        }

        #endregion

        #region WriteElementValue

        public void WriteElementValue(string localName, bool value)
        {
            _innerWriter.WriteStartElement(localName);
            _innerWriter.WriteValue(value);
            _innerWriter.WriteEndElement();
        }

        public async Task WriteElementValueAsync(string localName, bool value)
        {
            await WriteStartElementAsync(localName);
            await WriteValueAsync(value);
            await WriteEndElementAsync();
        }

        public void WriteElementValue(string localName, DateTime value)
        {
            _innerWriter.WriteStartElement(localName);
            _innerWriter.WriteValue(value);
            _innerWriter.WriteEndElement();
        }

        public async Task WriteElementValueAsync(string localName, DateTime value)
        {
            await WriteStartElementAsync(localName);
            await WriteValueAsync(value);
            await WriteEndElementAsync();
        }

        public void WriteElementValue(string localName, decimal value)
        {
            _innerWriter.WriteStartElement(localName);
            _innerWriter.WriteValue(value);
            _innerWriter.WriteEndElement();
        }

        public async Task WriteElementValueAsync(string localName, decimal value)
        {
            await WriteStartElementAsync(localName);
            await WriteValueAsync(value);
            await WriteEndElementAsync();
        }

        public void WriteElementValue(string localName, double value)
        {
            _innerWriter.WriteStartElement(localName);
            _innerWriter.WriteValue(value);
            _innerWriter.WriteEndElement();
        }

        public async Task WriteElementValueAsync(string localName, double value)
        {
            await WriteStartElementAsync(localName);
            await WriteValueAsync(value);
            await WriteEndElementAsync();
        }

        public void WriteElementValue(string localName, float value)
        {
            _innerWriter.WriteStartElement(localName);
            _innerWriter.WriteValue(value);
            _innerWriter.WriteEndElement();
        }

        public async Task WriteElementValueAsync(string localName, float value)
        {
            await WriteStartElementAsync(localName);
            await WriteValueAsync(value);
            await WriteEndElementAsync();
        }

        public void WriteElementValue(string localName, int value)
        {
            _innerWriter.WriteStartElement(localName);
            _innerWriter.WriteValue(value);
            _innerWriter.WriteEndElement();
        }

        public async Task WriteElementValueAsync(string localName, int value)
        {
            await WriteStartElementAsync(localName);
            await WriteValueAsync(value);
            await WriteEndElementAsync();
        }

        public void WriteElementValue(string localName, long value)
        {
            _innerWriter.WriteStartElement(localName);
            _innerWriter.WriteValue(value);
            _innerWriter.WriteEndElement();
        }

        public async Task WriteElementValueAsync(string localName, long value)
        {
            await WriteStartElementAsync(localName);
            await WriteValueAsync(value);
            await WriteEndElementAsync();
        }

        public void WriteElementValue(string localName, object value)
        {
            _innerWriter.WriteStartElement(localName);
            _innerWriter.WriteValue(value);
            _innerWriter.WriteEndElement();
        }

        public async Task WriteElementValueAsync(string localName, object value)
        {
            await WriteStartElementAsync(localName);
            await WriteValueAsync(value);
            await WriteEndElementAsync();
        }

        public void WriteElementValue(string localName, string value)
        {
            _innerWriter.WriteStartElement(localName);
            _innerWriter.WriteValue(value);
            _innerWriter.WriteEndElement();
        }

        public async Task WriteElementValueAsync(string localName, string value)
        {
            await WriteStartElementAsync(localName);
            await WriteValueAsync(value);
            await WriteEndElementAsync();
        }

        #endregion

        #region WriteEndAttribute

        public void WriteEndAttribute()
        {
            _innerWriter.WriteEndAttribute();
        }

        #endregion

        #region WriteEndDocument

        public void WriteEndDocument()
        {
            _innerWriter.WriteEndDocument();
        }

        public Task WriteEndDocumentAsync()
        {
            return _innerWriter.WriteEndDocumentAsync();
        }

        #endregion

        #region WriteEndElement

        public void WriteEndElement()
        {
            _innerWriter.WriteEndElement();
        }

        public Task WriteEndElementAsync()
        {
            return _innerWriter.WriteEndElementAsync();
        }

        #endregion

        #region WriteEntityRef

        public void WriteEntityRef(string name)
        {
            _innerWriter.WriteEntityRef(name);
        }

        public Task WriteEntityRefAsync(string name)
        {
            return _innerWriter.WriteEntityRefAsync(name);
        }

        #endregion

        #region WriteFullEndElement

        public void WriteFullEndElement()
        {
            _innerWriter.WriteFullEndElement();
        }

        public Task WriteFullEndElementAsync()
        {
            return _innerWriter.WriteFullEndElementAsync();
        }

        #endregion

        #region WriteName

        public void WriteName(string name)
        {
            _innerWriter.WriteName(name);
        }

        public Task WriteNameAsync(string name)
        {
            return _innerWriter.WriteNameAsync(name);
        }

        #endregion

        #region WriteNmToken

        public void WriteNmToken(string name)
        {
            _innerWriter.WriteNmToken(name);
        }

        public Task WriteNmTokenAsync(string name)
        {
            return _innerWriter.WriteNmTokenAsync(name);
        }

        #endregion

        #region WriteNode

        public void WriteNode(XmlReader reader, bool defattr)
        {
            _innerWriter.WriteNode(reader, defattr);
        }

        public Task WiteNodeAsync(XmlReader reader, bool defattr)
        {
            return _innerWriter.WriteNodeAsync(reader, defattr);
        }

        public void WriteNode(XPath.XPathNavigator navigator, bool defattr)
        {
            _innerWriter.WriteNode(navigator, defattr);
        }

        public Task WriteNodeAsync(XPath.XPathNavigator navigator, bool defattr)
        {
            return _innerWriter.WriteNodeAsync(navigator, defattr);
        }

        #endregion

        #region WriteProcessingInstruction

        public void WriteProcessingInstruction(string name, string text)
        {
            _innerWriter.WriteProcessingInstruction(name, text);
        }

        public Task WriteProcessingInstructionAsync(string name, string text)
        {
            return _innerWriter.WriteProcessingInstructionAsync(name, text);
        }

        #endregion

        #region WriteQualifiedName

        public void WriteQualifiedName(string localName, string ns)
        {
            _innerWriter.WriteQualifiedName(localName, ns);
        }

        public Task WriteQualifiedNameAsync(string localName, string ns)
        {
            return _innerWriter.WriteQualifiedNameAsync(localName, ns);
        }

        #endregion

        #region WriteRaw

        public void WriteRaw(string data)
        {
            _innerWriter.WriteRaw(data.Clean(false));
        }

        public Task WriteRawAsync(string data)
        {
            return _innerWriter.WriteRawAsync(data.Clean(false));
        }

        public void WriteRaw(char[] buffer, int index, int count)
        {
            _innerWriter.WriteRaw(buffer, index, count);
        }

        public Task WriteRawAsync(char[] buffer, int index, int count)
        {
            return _innerWriter.WriteRawAsync(buffer, index, count);
        }

        #endregion

        #region WriteStartAttribute

        public void WriteStartAttribute(string localName)
        {
            _innerWriter.WriteStartAttribute(localName);
        }

        public void WriteStartAttribute(string localName, string ns)
        {
            _innerWriter.WriteStartAttribute(localName, ns);
        }

        public void WriteStartAttribute(string prefix, string localName, string ns)
        {
            _innerWriter.WriteStartAttribute(prefix, localName, ns);
        }

        #endregion

        #region WriteStartDocument

        public void WriteStartDocument()
        {
            _innerWriter.WriteStartDocument();
        }

        public Task WriteStartDocumentAsync()
        {
            return _innerWriter.WriteStartDocumentAsync();
        }

        public void WriteStartDocument(bool standalone)
        {
            _innerWriter.WriteStartDocument(standalone);
        }

        public Task WriteStartDocumentAsync(bool standalone)
        {
            return _innerWriter.WriteStartDocumentAsync(standalone);
        }

        #endregion

        #region WriteStartElement

        public void WriteStartElement(string localName)
        {
            _innerWriter.WriteStartElement(localName);
        }

        public Task WriteStartElementAsync(string localName)
        {
            return WriteStartElementAsync(null, localName, null);
        }

        public void WriteStartElement(string localName, string ns)
        {
            _innerWriter.WriteStartElement(localName, ns);
        }

        public Task WriteStartElementAsync(string localName, string ns)
        {
            return WriteStartElementAsync(null, localName, ns);
        }

        public void WriteStartElement(string prefix, string localName, string ns)
        {
            _innerWriter.WriteStartElement(prefix, localName, ns);
        }

        public Task WriteStartElementAsync(string prefix, string localName, string ns)
        {
            return _innerWriter.WriteStartElementAsync(prefix, localName, ns);
        }

        #endregion

        #region WriteString

        public void WriteString(string text)
        {
            _innerWriter.WriteString(text.Clean(false));
        }

        public Task WriteStringAsync(string text)
        {
            return _innerWriter.WriteStringAsync(text.Clean(false));
        }

        #endregion

        #region WriteSurrogateCharEntity

        public void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            _innerWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public Task WriteSurrogateCharEntityAsync(char lowChar, char highChar)
        {
            return _innerWriter.WriteSurrogateCharEntityAsync(lowChar, highChar);
        }

        #endregion

        #region WriteValue

        //For asynchronous operations, convert the return value of WriteValue to a string and use the WriteStringAsync method.
        //https://msdn.microsoft.com/en-us/library/system.xml.xmlwriter.writevalue(v=vs.110).aspx

        public void WriteValue(bool value)
        {
            _innerWriter.WriteValue(value);
        }

        public Task WriteValueAsync(bool value)
        {
            return _innerWriter.WriteStringAsync(value.ToString());
        }

        public void WriteValue(DateTime value)
        {
            _innerWriter.WriteValue(value);
        }

        public Task WriteValueAsync(DateTime value)
        {
            return _innerWriter.WriteStringAsync(value.ToString());
        }

        public void WriteValue(decimal value)
        {
            _innerWriter.WriteValue(value);
        }

        public Task WriteValueAsync(decimal value)
        {
            return _innerWriter.WriteStringAsync(value.ToString());
        }

        public void WriteValue(double value)
        {
            _innerWriter.WriteValue(value);
        }

        public Task WriteValueAsync(double value)
        {
            return _innerWriter.WriteStringAsync(value.ToString());
        }

        public void WriteValue(float value)
        {
            _innerWriter.WriteValue(value);
        }

        public Task WriteValueAsync(float value)
        {
            return _innerWriter.WriteStringAsync(value.ToString());
        }

        public void WriteValue(int value)
        {
            _innerWriter.WriteValue(value);
        }

        public Task WriteValueAsync(int value)
        {
            return _innerWriter.WriteStringAsync(value.ToString());
        }

        public void WriteValue(long value)
        {
            _innerWriter.WriteValue(value);
        }

        public Task WriteValueAsync(long value)
        {
            return _innerWriter.WriteStringAsync(value.ToString());
        }

        public void WriteValue(Guid value)
        {
            _innerWriter.WriteValue(value.ToString());
        }

        public Task WriteValueAsync(Guid value)
        {
            return _innerWriter.WriteStringAsync(value.ToString());
        }

        public void WriteValue(object value)
        {
            if (value is string)
            {
                WriteValue((string)value);
            }
            else if (value is Guid)
            {
                WriteValue((Guid)value);
            }
            else
            {
                _innerWriter.WriteValue(value);
            }
        }

        public Task WriteValueAsync(object value)
        {
            if (value is string)
            {
                return WriteValueAsync((string)value);
            }
            else if (value is Guid)
            {
                return WriteValueAsync((Guid)value);
            }
            else
            {
                return _innerWriter.WriteStringAsync(value.ToString());
            }
        }

        public void WriteValue(string value)
        {
            _innerWriter.WriteValue(value.Clean(false));
        }

        public Task WriteValueAsycn(string value)
        {
            return _innerWriter.WriteStringAsync(value.Clean(false));
        }

        #endregion

        #region WriteWhitespace

        public void WriteWhitespace(string ws)
        {
            _innerWriter.WriteWhitespace(ws);
        }

        public Task WriteWhitespaceAsync(string ws)
        {
            return _innerWriter.WriteWhitespaceAsync(ws);
        }

        #endregion

        #endregion
    }
}
