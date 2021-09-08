using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwissAcademic.Azure.Swagger
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum)]
	public class SwaggerTypeAttribute
		 : 
		Attribute
	{
		public SwaggerTypeAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}
