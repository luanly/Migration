using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Xml
{
	public static class XmlDocumentExtensions
	{
		public static void LoadXmlSafe(this XmlDocument document, string xml)
		{
			//https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca3075
			using (var stringReader = new System.IO.StringReader(xml))
			{
				using (var reader = XmlReader.Create(stringReader, new XmlReaderSettings() { XmlResolver = null }))
				{
					document.Load(reader);
				}
			}
		}

		public static void LoadXmlSafe(this XmlDocument document, StreamReader streamReader)
		{
			using (var reader = XmlReader.Create(streamReader, new XmlReaderSettings() { XmlResolver = null }))
			{
				document.Load(reader);
			}
		}

		public static void LoadXmlSafe(this XmlDocument document, Stream stream)
		{
			using (var reader = XmlReader.Create(stream, new XmlReaderSettings() { XmlResolver = null }))
			{
				document.Load(reader);
			}
		}
	}
}
