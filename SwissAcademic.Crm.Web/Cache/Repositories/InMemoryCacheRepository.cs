using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace SwissAcademic.Crm.Web
{
    [ExcludeFromCodeCoverage]
    public class InMemoryCacheRepository
        :
        IDisposable
    {
		#region Felder

		bool IsDisposed;
        MemoryCache Cache = new MemoryCache(new MemoryCacheOptions());
        CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

		#endregion

		#region Konsturktor

        public InMemoryCacheRepository()
        {

        }

        #endregion

        #region Methoden

        #region AddOrUpdate

        public Task AddOrUpdateAsync<T>(T item, string key)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions();
            cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
            cacheEntryOptions.AddExpirationToken(new CancellationChangeToken(CancellationTokenSource.Token));
            Cache.Set(key, item, cacheEntryOptions);
            return Task.CompletedTask;
        }

        #endregion

        #region Clear

        public void Clear()
        {
            CancellationTokenSource.Cancel();
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                Cache.Dispose();
                CancellationTokenSource.Dispose();
            }

            IsDisposed = true;
        }

        #endregion

        #region Get

        public Task<T> GetAsync<T>(string key)
        {
            if (Cache.TryGetValue(key, out IHasCacheETag result))
            {
                return Task.FromResult((T)result);
            }
            return Task.FromResult(default(T));
        }

        #endregion

        #region GetETagAsync

        public Task<string> GetETagAsync(string key)
        {
            if (Cache.TryGetValue(key, out IHasCacheETag result))
            {
                return Task.FromResult(result.CacheETag);
            }
            return Task.FromResult(string.Empty);
        }

        #endregion

        #region Remove
        public Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Task.CompletedTask;
            }
            Cache.Remove(key);
            return Task.CompletedTask;
        }

        #endregion

        #endregion
    }
}