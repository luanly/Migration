using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class UluruEligibilityCheckRequestItem
	{
		public int Sequence { get; set; }
		public string ProductCode { get; set; }
		public UluruUpgradeType UpgradeType { get; set; }
		public UluruLicenseType? LicenseType { get; set; }
	}
}
