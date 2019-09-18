using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            RegisterContainer();

            var authentication = _container.Resolve<IAuthentication>();

            var isValid = authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine(isValid);
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeOtp>().As<IOtpService>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<FakeLogger>().As<ILogger>();

            builder.RegisterType<AuthenticationService>().As<IAuthentication>();

            builder.RegisterType<NotificationDecorator>();
            builder.RegisterType<FailedCounterDecorator>();
            builder.RegisterType<LogDecorator>();
            builder.RegisterType<LogMethodInfoDecorator>();

            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogDecorator, IAuthentication>();
            builder.RegisterDecorator<LogMethodInfoDecorator, IAuthentication>();

            _container = builder.Build();
        }
    }

    internal class LogMethodInfoDecorator : AuthenticationBaseDecorator
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var message = $"{nameof(AuthenticationService)}.{nameof(Verify)}:{accountId} | {password} |{otp}";
            _logger.Info(message);

            var isValid = base.Verify(accountId, password, otp);

            _logger.Info($"{accountId} isValid: {isValid.ToString()}");

            return isValid;
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
            return false;
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