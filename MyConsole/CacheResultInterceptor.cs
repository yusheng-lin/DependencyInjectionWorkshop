using System;
using System.Linq;
using Castle.DynamicProxy;

namespace MyConsole
{
    public class CacheResultInterceptor : IInterceptor
    {
        private readonly ICacheProvider _cache;

        public CacheResultInterceptor(ICacheProvider cache)
        {
            _cache = cache;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!(Attribute.GetCustomAttribute(invocation.Method, typeof(CacheResultAttribute)) is CacheResultAttribute
                cacheResultAttribute))
            {
                invocation.Proceed();
            }
            else
            {
                string key = GetInvocationSignature(invocation);

                if (_cache.Contains(key))
                {
                    invocation.ReturnValue = _cache.Get(key);
                    return;
                }

                invocation.Proceed();
                var result = invocation.ReturnValue;

                var duration = cacheResultAttribute.Duration;
                if (result != null)
                {
                    _cache.Put(key, result, duration);
                }
            }
        }

        private string GetInvocationSignature(IInvocation invocation)
        {
            return
                $"{invocation.TargetType.FullName}-{invocation.Method.Name}-{String.Join("-", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())}";
        }
    }

    public class CacheResultAttribute : Attribute
    {
        public int Duration { get; set; }
    }
}