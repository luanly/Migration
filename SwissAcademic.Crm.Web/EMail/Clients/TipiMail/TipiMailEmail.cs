using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SwissAcademic.Crm.Web.EMail.Clients.TipiMail
{
	public class TipiMailEmail
	{
		const string Headers_Tracking = "X-TM-TRACKING";
		const string Headers_Bulk = "X-TM-BULK";
		const string Headers_Tags = "X-TM-TAGS";

		public TipiMailEmail()
		{
			DisableTracking();
		}

		[JsonProperty("to")]
		public List<TipiMailEmailAddress> To { get; } = new List<TipiMailEmailAddress>();

		[JsonProperty("msg")]
		public TipiMailMessage Message { get; } = new TipiMailMessage();

		[JsonProperty("headers")]
		public JObject Headers { get; } = new JObject();

		public void AddTag(string tag)
		{
			JArray tags;
			if (!Headers.ContainsKey(Headers_Tags))
			{
				Headers[Headers_Tags] = new JArray();
			}
			tags = Headers[Headers_Tags] as JArray;
			tags.Add(tag);
		}

		public void DisableTracking()
		{
			Headers[Headers_Tracking] = new JObject();
			Headers[Headers_Tracking]["html"] = new JObject();
			Headers[Headers_Tracking]["text"] = new JObject();

			Headers[Headers_Tracking]["html"]["open"] = 0;
			Headers[Headers_Tracking]["html"]["click"] = 0;
			Headers[Headers_Tracking]["text"]["click"] = 0;
		}

		public void EnableBulkMail()
		{
			Headers[Headers_Bulk] = 1;
		}
	}
}
