using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class SignatureInfo
	{
		public DateTimeOffset AbsoluteExpiration { get; set; }
		public string Signature { get; set; }
		public string Token { get; set; }
	}
}
