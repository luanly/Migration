using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SwissAcademic.Crm.Web.EMail.Clients.TipiMail
{
	[JsonObject]
	public class TipiMailMessage
	{
		[JsonProperty("from")]
		public TipiMailEmailAddress From { get; set; }

		[JsonProperty("replyTo")]
		public TipiMailEmailAddress ReplyTo { get; set; }

		[JsonProperty("subject")]
		public string Subject { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("html")]
		public string Html { get; set; }

		[JsonProperty("images")]
		public List<TipiMailContent> Images { get; } = new List<TipiMailContent>();

		[JsonProperty("attachments")]
		public List<TipiMailContent> Attachments { get; } = new List<TipiMailContent>();
	}
}
