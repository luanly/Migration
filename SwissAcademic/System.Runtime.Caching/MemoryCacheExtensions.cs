using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Runtime.Caching
{
    public static class MemoryCacheExtensions
    {
        #region Get

        public static T Get<T>(this MemoryCache cache, string key, Func<T> valueFactory)
            where T : class
        {
            return Get<T>(cache, key, valueFactory, new CacheItemPolicy());
        }

        public static T Get<T>(this MemoryCache cache, string key, Func<T> valueFactory, CacheItemPolicy policy)
            where T : class
        {
            var value = (T)cache.Get(key);

            if (value != null) return value;

            Wait:
            while (_constructorList.Contains(key))
            {
                Task.Delay(20).Wait();
            }

            var result = AddOrGetExisting(cache, key, valueFactory, policy);
            if (result == null) goto Wait;

            return result;
        }

        #endregion

        #region GetAsync

        public static Task<T> GetAsync<T>(this MemoryCache cache, string key, Func<Task<T>> asyncValueFactory)
            where T : class
        {
            return GetAsync(cache, key, asyncValueFactory, new CacheItemPolicy());
        }

        public static async Task<T> GetAsync<T>(this MemoryCache cache, string key, Func<Task<T>> asyncValueFactory, CacheItemPolicy policy)
            where T : class
        {
            var value = (T)cache.Get(key);

            if (value != null) return value;

            Wait:
            while (_constructorList.Contains(key))
            {
                await Task.Delay(20);
            }

            var result = await AddOrGetExistingAsync(cache, key, asyncValueFactory, policy);
            if (result == null) goto Wait;

            return result;
        }

        #endregion

        static List<string> _constructorList = new List<string>();
        static readonly object cacheLock = new object();

        #region AddOrGetExisting

        static T AddOrGetExisting<T>(MemoryCache cache, string key, Func<T> valueFactory, CacheItemPolicy policy)
            where T : class
        {
            lock (cacheLock)
            {
                if (_constructorList.Contains(key)) return (T)null;

                var value = (T)cache.Get(key);
                if (value != null) return value;

                _constructorList.Add(key);
            }

            try
            {
                var value = valueFactory();
                var cacheValue = (T)cache.AddOrGetExisting(key, value, policy);
                return cacheValue ?? value;
            }
            finally
            {
                _constructorList.Remove(key);
            }
        }

        #endregion

        #region AddOrGetExistingAsync

        static async Task<T> AddOrGetExistingAsync<T>(MemoryCache cache, string key, Func<Task<T>> asyncValueFactory, CacheItemPolicy policy)
            where T : class
        {
            lock (cacheLock)
            {
                if (_constructorList.Contains(key)) return (T)null;

                var value = (T)cache.Get(key);
                if (value != null) return value;

                _constructorList.Add(key);
            }

            try
            {
                var value = await asyncValueFactory();
                var cacheValue = (T)cache.AddOrGetExisting(key, value, policy);
                return cacheValue ?? value;
            }
            finally
            {
                _constructorList.Remove(key);
            }
        }

        #endregion
    }
}
