using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class EmailBounce
	{
		public long Created { get; set; }
		public DateTimeOffset CreatedOn
		{
			get
			{
				return DateTimeOffset.FromUnixTimeSeconds(Created);
			}
		}
		public string Email { get; set; }
		public string Reason { get; set; }
		public string Id { get; set; }
	}
}
