using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class UluruOrderItem
	{
		public string CitaviProductId { get; set; }

		public string Quantity { get; set; }
		public UluruUpgradeType UpgradeType { get; set; }

		public UluruLicenseType LicenceType { get; set; }

		public string TransactionId { get; set; }
		public string SubscriptionLineRef { get; set; }

		public int ServiceLimit { get; set; }

		public DateTime? StartDateUtc { get; set; }
		public DateTime? EndDateUtc { get; set; }

		public int QuantityInt
		{
			get => int.Parse(Quantity, System.Globalization.NumberStyles.AllowDecimalPoint);
		}
	}
}
