using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class UluruOrder
	{
		public string CitaviId { get; set; }

		public string SoldToFirstName { get; set; }
		public string SoldToLastName { get; set; }
		public string SoldToEmail { get; set; }
		public string SoldToOrganisation { get; set; }
		public string SoldToCountryCode { get; set; }

		public string SubscriptionRef { get; set; }

		public DateTime? ContractStartUtc { get; set; }
		public DateTime? ContractEndDateUtc { get; set; }

		public List<UluruOrderItem> Products { get; set; } = new List<UluruOrderItem>();

		public string GetContactField(ContactPropertyId propertyId) 
		{
			switch (propertyId)
			{
				case ContactPropertyId.FirstName:
					return SoldToFirstName;

				case ContactPropertyId.LastName:
					return SoldToLastName;

				case ContactPropertyId.EMailAddress1:
					return SoldToEmail;

				case ContactPropertyId.Firm:
					return SoldToOrganisation;

				default:
					throw new NotImplementedException($"Unkown ContactPropertyId {propertyId}");
			}
		}

		public string GetContactCountryCode(UluruContactType contactType)
		{
			switch (contactType) 
			{
				case UluruContactType.SoldTo:
					return SoldToCountryCode;

				default:
					throw new NotImplementedException($"Unkown ZuoraContactType '{contactType}'");
			}
		}

		public string Raw { get; set; }
	}
}
