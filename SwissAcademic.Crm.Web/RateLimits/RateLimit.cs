using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public static class RateLimits
	{
		public static CrmEntityRateLimit<CrmUser> CreateProject => new CrmEntityRateLimit<CrmUser>(RateLimitType.MaxProjectsPerUser);
		public static CrmEntityRateLimit<ProjectEntry> CreateProjectRole => new CrmEntityRateLimit<ProjectEntry>(RateLimitType.MaxProjectRolesPerProject);
	}

	public class CrmEntityRateLimit<T>
		where T: class
	{
		internal CrmEntityRateLimit(RateLimitType rateLimitType)
		{
			RateLimitType = rateLimitType;
		}

		public RateLimitType RateLimitType { get; }

		public async Task<bool> ExceededAsync(T entity, bool throwIfExceeded = true, int? overrideRateLimit = null)
		{
			var exceeded = false;
			switch (RateLimitType)
			{
				case RateLimitType.MaxProjectsPerUser:
					{
						exceeded = (entity as CrmUser).ProjectRoles.Count() >= (overrideRateLimit ?? RateLimitConstants.MaxProjectsPerUser);
					}
					break;
				case RateLimitType.MaxProjectRolesPerProject:
					{
						var projectRoles = await(entity as ProjectEntry).ProjectRoles.Get();
						exceeded = projectRoles.Count() >= (overrideRateLimit ?? RateLimitConstants.MaxProjectRolesPerProject);
					}
					break;
			}
			if (exceeded && throwIfExceeded)
			{
				throw new RateLimitException($"RateLimit {RateLimitType} exceeded");
			}

			return exceeded;
		}
	}
}
