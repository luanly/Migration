using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	[DataContract]
	public class CitaviSpaceUserStatistic
	{
		[IgnoreDataMember]
		public long AvailableBytes { get; set; }

		/// <summary>
		/// CitaviSpace from CitaviSpace Licenses
		/// </summary>
		[DataMember]
		public long CitaviSpaceLicenseBytes { get; set; }

		/// <summary>
		/// CitaviSpace from Licenses other than CitaviSpace (e.g. CitaviWeb)
		/// </summary>
		[DataMember]
		public long CitaviLicenseBytes { get; set; }

		[DataMember]
		public bool HasCitaviWebLicense { get; set; }

		[DataMember]
		public long FreeBytes { get; set; }

		[DataMember]
		public long UsedBytes { get; set; }

		[DataMember]
		public bool IsExceeded { get; set; }

		[DataMember]
		public List<CitaviSpaceProjectStatistic> Projects { get; } = new List<CitaviSpaceProjectStatistic>();
	}
}
