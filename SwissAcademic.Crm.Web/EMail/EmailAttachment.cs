using System;
using System.IO;

namespace SwissAcademic.Crm.Web
{
    public class EmailAttachment
    {
        public EmailAttachment(MemoryStream stream, string filename, string mimeType)
            : this(stream.ToArray(), filename, mimeType) { }

        public EmailAttachment(byte[] stream, string filename, string mimeType)
        {
            Content = stream;
            BodyBase64 = Convert.ToBase64String(stream);
            FileName = filename;
            Subject = filename;
            MimeType = mimeType;
        }

        internal EmailAttachment()
        {

        }

        internal string BodyBase64 { get; set; }
        internal byte[] Content { get; set; }
        internal string FileName { get; set; }
        internal string MimeType { get; set; }
        internal string Subject { get; set; }
    }
}
