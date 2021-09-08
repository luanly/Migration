using SwissAcademic.Crm.Web;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	[DataContract]
	public class CitaviSpaceProjectStatistic
	{
		private CitaviSpaceProjectStatistic()
		{

		}
		public CitaviSpaceProjectStatistic(ProjectEntry projectEntry)
		{
			Name = projectEntry.Name;
			Key = projectEntry.Key;
		}

		[DataMember]
		public List<CitaviSpaceAttachementStatistic> Attachments { get; set; } = new List<CitaviSpaceAttachementStatistic>();

		[DataMember]
		public string LastUpdate { get; set; }

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Key { get; set; }

		[DataMember]
		public long Size { get; set; }
	}
}
