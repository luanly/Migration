using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class CrmAttributeValidationException
		:
		Exception
	{
		public CrmAttributeValidationException(string attributeName, string value, string message)
			:
			base(message)
		{
			AttributeName = attributeName;
			Value = value;

			Data["AttributeName"] = attributeName;
			Data["Value"] = value;
		}

		public string AttributeName { get; }
		public string Value { get; }
	}
}
