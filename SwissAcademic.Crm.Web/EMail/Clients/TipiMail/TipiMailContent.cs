using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SwissAcademic.Crm.Web.EMail.Clients.TipiMail
{
	[JsonObject]
	public class TipiMailContent
	{
		[JsonProperty("type")]
		public string ContentType { get; set; }

		[JsonProperty("filename")]
		public string FileName { get; set; }

		/// <summary>
		/// Encoded on base 64
		/// </summary>
		[JsonProperty("content")]
		public string Content { get; set; }
	}
}
