using System;
using System.Runtime.Caching;

namespace MyConsole
{
    public class MemoryCacheProvider : ICacheProvider
    {
        public bool Contains(string key)
        {
            return MemoryCache.Default[key] != null;
        }

        public object Get(string key)
        {
            return MemoryCache.Default[key];
        }

        public void Put(string key, object result, int duration)
        {
            if (duration <= 0)
                throw new ArgumentException("Duration cannot be less or equal to zero", nameof(duration));

            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddMilliseconds(duration)
            };

            MemoryCache.Default.Set(key, result, policy);
        }
    }
}