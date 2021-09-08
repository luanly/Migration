using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class UluruEligibilityCheckResult
	{
		public string CitaviId { get; set; }
		public List<UluruEligibilityCheckResultItem> Products { get; } = new List<UluruEligibilityCheckResultItem>();
	}
}
