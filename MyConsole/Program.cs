using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    
    class Program
    {
        private static readonly IContainer Container = SetUp();
        static void Main(string[] args)
        {
            var optService = Container.Resolve<IOtpService>();
            optService.GetCurrentOtp("Test");
            optService.GetCurrentOtp("Test1");
            optService.GetCurrentOtp("Test");
            System.Threading.Thread.Sleep(10000);
            optService.GetCurrentOtp("Test");
            //IUserService fakeUserService = new FakeUserService();
            //IHash fakeHash = new FakeHash();
            //IOtpService fakeOtpService = new FakeOtpService();
            //IAuthentication authenticationService = new AuthenticationService(fakeUserService, fakeHash, fakeOtpService);

            //IFailedCount fakeFailedCounter = new FakeFailCounter();
            //ILogger fakeLogger = new FakeLogger();
            //IMessenger fakeMessenger = new FakeMessenger();

            //var fakeSessionData = Container.Resolve<ISessionData>();
            //fakeSessionData.UserInfo = new UserInfo() {Account = "superuser"};

            //var authenticationService = Container.Resolve<IAuthentication>();

            ////authenticationService = new FailedCountDecorator(authenticationService, fakeFailedCounter);
            ////authenticationService = new LogFailedCountDecorator(authenticationService, fakeFailedCounter , fakeLogger);
            ////authenticationService = new NotifyDecorator(authenticationService, fakeMessenger);
            ////authenticationService = new LogMethodDecorator( fakeLogger, authenticationService, fakeSessionData);

            //authenticationService.Verify("alan", "pass.123", "123456");
            Console.ReadKey();
        }

        static IContainer SetUp()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FakeUserService>().As<IUserService>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeOtpService>().As<IOtpService>().EnableInterfaceInterceptors()
                .InterceptedBy(typeof(CacheInterceptor));
            builder.RegisterType<LogInterceptor>();
            builder.RegisterType<FakeFailCounter>().As<IFailedCount>()
                .EnableInterfaceInterceptors().InterceptedBy(typeof(LogInterceptor));
            builder.RegisterType<MemoryCacheProvider>().As<ICacheProvider>();
            builder.RegisterType<CacheInterceptor>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeMessenger>().As<IMessenger>();
            builder.RegisterType<LogFailedCountDecorator>();
            builder.RegisterType<FailedCountDecorator>();
            builder.RegisterType<NotifyDecorator>();
            builder.RegisterType<LogMethodDecorator>();
            builder.RegisterType<AuthenticationService>().As<IAuthentication>();
            builder.RegisterType<FakeSessionData>().As<ISessionData>().SingleInstance();

            builder.RegisterDecorator<LogFailedCountDecorator, IAuthentication>();
            builder.RegisterDecorator<FailedCountDecorator, IAuthentication>();
            builder.RegisterDecorator<NotifyDecorator, IAuthentication>();
            builder.RegisterDecorator<LogMethodDecorator, IAuthentication>();

            return builder.Build();
        }
    }

    public class CacheInterceptor:IInterceptor
    {
        private readonly ICacheProvider _cache;

        public CacheInterceptor(ICacheProvider cache)
        {
            _cache = cache;
        }

        public void Intercept(IInvocation invocation)
        {
            var key = $"{invocation.TargetType.Name}-{invocation.Method.Name}-{string.Join(",", invocation.Arguments)}";
            if (_cache.Contain(key))
            {
                invocation.ReturnValue = _cache.Get(key);
                return;
            }
            else
            {
                invocation.Proceed();
                _cache.Put(key,invocation.ReturnValue,10);
            }
        }
    }

    public interface ICacheProvider
    {
        bool Contain(string key);
        object Get(string key);
        void Put(string key, object result, int duration);

    }

    public class MemoryCacheProvider : ICacheProvider
    {
        public bool Contain(string key)
        {
            return MemoryCache.Default[key] != null;
        }

        public object Get(string key)
        {
            return MemoryCache.Default[key];
        }

        public void Put(string key, object result, int duration)
        {
            MemoryCache.Default.Set(key, result, new CacheItemPolicy()
            {
                AbsoluteExpiration = DateTime.Now.AddSeconds(duration)
            });
        }
    }

    public  class LogInterceptor:IInterceptor
    {
        private readonly ILogger _logger;

        public LogInterceptor(ILogger logger)
        {
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            if (Attribute.GetCustomAttribute(invocation.Method, typeof(AuditLogAttribute)) is AuditLogAttribute auditlog
            )
            {
                _logger.LogInfo($"{invocation.Method.Name} - Invocation Begin");
                invocation.Proceed();
                _logger.LogInfo($"{invocation.Method.Name} -Invocation End");
            }
            else
                invocation.Proceed();
        }
    }

    internal class FakeSessionData : ISessionData
    {
        public UserInfo UserInfo { get; set; }
    }

    internal class FakeFailCounter : IFailedCount
    {
        public void AddFailedCount(string account)
        {
            Console.WriteLine($"{nameof(FakeFailCounter)}.{nameof(AddFailedCount)}({account})");
        }

        public void ResetFailedCount(string account)
        {
            Console.WriteLine($"{nameof(FakeFailCounter)}.{nameof(ResetFailedCount)}({account})");
        }

        public bool GetAccountIsLocked(string account)
        {
            Console.WriteLine($"{nameof(FakeFailCounter)}.{nameof(GetAccountIsLocked)}({account})");
            return false;
        }

        public int GetFailedCount(string account)
        {
            Console.WriteLine($"{nameof(FakeFailCounter)}.{nameof(GetFailedCount)}({account})");
            return 123;
        }
    }

    internal class FakeMessenger : IMessenger
    {
        public void PushMessage(string account)
        {
            Console.WriteLine($"{nameof(FakeMessenger)}.{nameof(PushMessage)}({account})");
        }
    }

    internal class FakeLogger : ILogger
    {
        public void LogInfo(string message)
        {
            Console.WriteLine($"{nameof(FakeLogger)}.{nameof(LogInfo)}({message})");
        }
    }


    internal class FakeOtpService : IOtpService
    {
        public string GetCurrentOtp(string account)
        {
            Console.WriteLine($"{nameof(FakeOtpService)}.{nameof(GetCurrentOtp)}({account})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string GetHashedPassword(string password)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(GetHashedPassword)}({password})");
            return "my hashed password";
        }
    }

    internal class FakeUserService : IUserService
    {
        public string GetPassword(string account)
        {
            Console.WriteLine($"{nameof(FakeUserService)}.{nameof(GetPassword)}({account})");
            return "my hashed password";
        }
    }
}
