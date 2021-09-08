using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	[DataContract]
	public class CitaviSpaceAttachementStatistic
	{
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public string Type { get; set; }
		[DataMember]
		public string CoreStatement { get; set; }

		[DataMember]
		public Guid Id { get; set; }

		[DataMember]
		public Guid ReferenceId { get; set; }

		[DataMember]
		public string ShortTitle { get; set; }

		[DataMember]
		public long Size { get; set; }
	}
}
