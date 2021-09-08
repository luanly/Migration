using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SwissAcademic.Crm.Web.EMail.Clients.TipiMail
{
	public class TipiMailEmailAddress
	{
		public TipiMailEmailAddress(string address, string name)
		{
			Address = address;
			PersonalName = name;
		}

		[JsonProperty("address")]
		public string Address { get; }

		[JsonProperty("personalName")]
		public string PersonalName { get; }
	}
}
