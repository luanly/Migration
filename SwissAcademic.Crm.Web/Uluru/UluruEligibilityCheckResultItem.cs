using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class UluruEligibilityCheckResultItem
	{
		public int AllowedUpgrades { get; set; }
		public string ProductCode { get; set; }
		public UluruUpgradeType UpgradeType { get; set; }
		public UluruLicenseType? LicenseType { get; set; }

		public int Sequence { get; set; }
	}
}
