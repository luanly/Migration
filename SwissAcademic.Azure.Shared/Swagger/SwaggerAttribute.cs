using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Azure.Swagger
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Enum)]
	public class SwaggerAttribute
		 : 
		Attribute
	{
		public SwaggerAttribute(string title)
		{
			Title = title;
		}

		public string Title { get; }
	}
}
