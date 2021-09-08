using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class UluruEligibilityCheckRequest
	{
		public string Email { get; set; }
		public List<UluruEligibilityCheckRequestItem> Products { get; set; }

		public static UluruEligibilityCheckRequest Create(string email, string productCode, UluruUpgradeType upgradeType, UluruLicenseType licenseType = UluruLicenseType.PERPETUAL)
		{
			return new UluruEligibilityCheckRequest
			{
				Email = email,
				Products = new List<UluruEligibilityCheckRequestItem>()
				{
					new UluruEligibilityCheckRequestItem
					{
						ProductCode = productCode,
						UpgradeType = upgradeType,
						LicenseType = licenseType,
						Sequence = 1
					}
				}
			};
		}
	}
}
