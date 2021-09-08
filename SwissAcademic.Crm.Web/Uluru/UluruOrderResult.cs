using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	[DataContract]
	public class UluruOrderResult
	{
		[DataMember]
		public string CitaviId { get; set; }
		[IgnoreDataMember]
		public UluruOrderStatus Status { get; set; }
		[IgnoreDataMember]
		internal string ExceptionMessage { get; set; }
	}

	public enum UluruOrderStatus
	{
		CrmException,
		Ok,
		UnknownException,
		ContactKeyEmailMissmatch
	}
}
