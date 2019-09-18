using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.DynamicProxy;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            RegisterContainer();

            //var authentication = _container.Resolve<IAuthentication>();

            //var isValid = authentication.Verify("joey", "abc", "wrong otp");
            //Console.WriteLine(isValid);

            //var orderService = new OrderService();
            var orderService = _container.Resolve<IOrderService>();

            Console.WriteLine(orderService.CreateGuid("Joey", 91));
            Console.WriteLine(orderService.CreateGuid("Joey", 91));
            Console.WriteLine(orderService.CreateGuid("Tom", 66));
            Console.WriteLine(orderService.CreateGuid("Joey", 91));
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FakeProfile>().As<IProfile>()
                   .EnableInterfaceInterceptors();
            //.InterceptedBy(typeof(LogMethodInfoInterceptor));

            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeOtp>().As<IOtpService>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeContext>().As<IContext>();

            builder.RegisterType<MemoryCacheProvider>().As<ICacheProvider>();
            builder.RegisterType<CacheResultInterceptor>();

            builder.RegisterType<OrderService>().As<IOrderService>()
                   .EnableInterfaceInterceptors()
                   .InterceptedBy(typeof(CacheResultInterceptor));

            builder.RegisterType<AuditLogInterceptor>();

            builder.RegisterType<FakeAlarm>().As<IAlarm>();
            builder.RegisterType<AlarmInterceptor>();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>()
                   .EnableInterfaceInterceptors()
                   .InterceptedBy(typeof(AuditLogInterceptor), typeof(AlarmInterceptor));

            builder.RegisterType<NotificationDecorator>();
            builder.RegisterType<FailedCounterDecorator>();
            builder.RegisterType<LogDecorator>();
            //builder.RegisterType<LogMethodInfoDecorator>();

            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogDecorator, IAuthentication>();
            //builder.RegisterDecorator<LogMethodInfoDecorator, IAuthentication>();

            _container = builder.Build();
        }
    }

    public interface IOrderService
    {
        [CacheResult(Duration = 1000)]
        string CreateGuid(string account, int token);
    }

    public class OrderService : IOrderService
    {
        public string CreateGuid(string account, int token)
        {
            Console.WriteLine($"sleep 1.5 seconds, account:{account}, token:{token}");
            Thread.Sleep(1500);
            return Guid.NewGuid().ToString("N");
        }
    }

    internal class FakeAlarm : IAlarm
    {
        public void Raise(string roleId, Exception exception)
        {
            Console.WriteLine($"call role:{roleId} with {exception}");
        }
    }

    internal class FakeContext : IContext
    {
        public string GetCurrentUser()
        {
            return "JoeyChen9191";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }

    internal class FakeHash : IHash
    {
        public string Compute(string input)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Compute)}({input})");
            return "my hashed password";
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void Add(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Add)}({accountId})");
        }

        public int Get(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Get)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return true;
            //return false;
        }

        public void Reset(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(Reset)}({accountId})");
        }
    }

    internal class FakeSlack : INotification
    {
        public void Notify(string accountId)
        {
            Console.WriteLine($"{nameof(FakeSlack)}.{nameof(Notify)}({accountId})");
        }
    }

    internal class FakeLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}