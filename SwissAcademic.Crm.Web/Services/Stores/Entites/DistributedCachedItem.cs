using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;

namespace SwissAcademic.Crm.Web
{
    public class DistributedCachedItem
        :
        TableEntity
    {
        public readonly static List<string> ColumnNames = new List<string> {
            nameof(Data),
            nameof(SlidingExperiation),
            nameof(AbsolutExperiation),
            nameof(LastAccessTime),
        };

        public DistributedCachedItem()
        {

        }
        public DistributedCachedItem(DistributedCacheEntryOptions options, byte[] data)
        {
            var currentTime = DateTimeOffset.UtcNow;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                AbsolutExperiation = currentTime.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }
            else if (options.AbsoluteExpiration.HasValue)
            {
                AbsolutExperiation = options.AbsoluteExpiration;
            }
            SlidingExperiation = options.SlidingExpiration;
            LastAccessTime = currentTime;
            Data = data;
        }

        public byte[] Data { get; set; }
        public TimeSpan? SlidingExperiation { get; set; }
        public DateTimeOffset? AbsolutExperiation { get; set; }
        public DateTimeOffset? LastAccessTime { get; set; }

        public bool IsExpired
        {
            get
            {
                var currentTime = DateTimeOffset.UtcNow;
                if (AbsolutExperiation != null && AbsolutExperiation.Value <= currentTime)
                {
                    return true;
                }
                if (SlidingExperiation.HasValue && LastAccessTime.HasValue && LastAccessTime.Value.Add(SlidingExperiation.Value) < currentTime)
                {
                    return true;
                }

                return false;
            }
        }

    }
}
