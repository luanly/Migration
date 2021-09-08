using System;
using System.Collections.Generic;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public static class RateLimitConstants
	{
		public const int MaxProjectsPerUser = 1000;
		public const int MaxProjectRolesPerProject = 1000;
		public const int MaxChatMessageLength = 64000;
	}
}
