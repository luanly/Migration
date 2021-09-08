
using System.IO;
using System.Text;
namespace System.Xml
{
	#region XmlUtility
	////////////////////////////////////////////////////////////////////////////////////////////////////
	/// <summary>	Provides utility methods for XmlReader. </summary>
	///
	/// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public static class XmlReaderUtility
	{
		#region LoadFromString

		public static void LoadFromString(this XmlDocument xmlDocument, string xml)
		{
			using (var stringReader = new StringReader(xml))
			{
				using (var xmlTextReader = new XmlTextReader(stringReader))
				{
					xmlDocument.Load(xmlTextReader);
				}
			}
		}

		#endregion

		#region ReadEnum

		////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Reads the current element and returns the contents as <typeparamref name="T"/>. </summary>
		///
		/// <remarks>	Thomas Schempp, 14.01.2010. </remarks>
		///
		/// <typeparam name="T">	The Enum type to which the contents are parsed. </typeparam>
		/// <param name="r">	The XmlReader instance. </param>
		///
		/// <returns>	The parsed enum value. </returns>
		////////////////////////////////////////////////////////////////////////////////////////////////////

		public static T ReadEnum<T>(this XmlReader r)
		{
			return (T)Enum.Parse(typeof(T), r.ReadElementContentAsString());
		} 

		#endregion	

		#region ReadToElement

		public static bool ReadToElement(this XmlReader r)
		{
			while (r.Read()) if (r.NodeType == XmlNodeType.Element) return true;

			return false;
		} 

		#endregion

		#region SaveToString

		public static string SaveToString(this XmlDocument xmlDocument)
		{
			var stringBuilder = new StringBuilder();
			using (var stringWriter = new StringWriter(stringBuilder))
			{
				using (var xmlTextWriter = new XmlTextWriter(stringWriter))
				{
					xmlDocument.Save(xmlTextWriter);
				}
			}

			return stringBuilder.ToString();
		}

		#endregion

		#region TryReadEnum

		public static bool TryReadEnum<T>(this XmlReader r, out T result)
			where T: struct
		{
			return Enum.TryParse(r.ReadElementContentAsString(), out result);
		}

		#endregion
	}

	public static class XmlWriterUtility
	{
		#region WriteElementValue

		public static void WriteElementValue(this XmlWriter w, string localName, bool value)
		{
			w.WriteStartElement(localName);
			w.WriteValue(value);
			w.WriteEndElement();
		}

		public static void WriteElementValue(this XmlWriter w, string localName, DateTime value)
		{
			w.WriteStartElement(localName);
			w.WriteValue(value);
			w.WriteEndElement();
		}

		public static void WriteElementValue(this XmlWriter w, string localName, decimal value)
		{
			w.WriteStartElement(localName);
			w.WriteValue(value);
			w.WriteEndElement();
		}

		public static void WriteElementValue(this XmlWriter w, string localName, double value)
		{
			w.WriteStartElement(localName);
			w.WriteValue(value);
			w.WriteEndElement();
		}

		public static void WriteElementValue(this XmlWriter w, string localName, float value)
		{
			w.WriteStartElement(localName);
			w.WriteValue(value);
			w.WriteEndElement();
		}

		public static void WriteElementValue(this XmlWriter w, string localName, int value)
		{
			w.WriteStartElement(localName);
			w.WriteValue(value);
			w.WriteEndElement();
		}

		public static void WriteElementValue(this XmlWriter w, string localName, long value)
		{
			w.WriteStartElement(localName);
			w.WriteValue(value);
			w.WriteEndElement();
		}

		public static void WriteElementValue(this XmlWriter w, string localName, object value)
		{
			w.WriteStartElement(localName);
			w.WriteValue(value);
			w.WriteEndElement();
		}

		public static void WriteElementValue(this XmlWriter w, string localName, string value)
		{
			w.WriteStartElement(localName);
			w.WriteValue(value);
			w.WriteEndElement();
		}

		#endregion
	}


	#endregion
}
