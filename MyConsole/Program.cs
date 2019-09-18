using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static ILogger _logger;
        private static INotification _notification;
        private static IFailedCounter _failedCounter;
        private static IOtpService _otpService;
        private static IHash _hash;
        private static IProfile _profile;
        private static IAuthentication _authenticationService;

        static void Main(string[] args)
        {
            _logger = new FakeLogger();
            _notification = new FakeSlack();
            _failedCounter = new FakeFailedCounter();
            _otpService = new FakeOtp();
            _hash = new FakeHash();
            _profile = new FakeProfile();

            _authenticationService =
                new AuthenticationService(_profile, _hash, _otpService);

            _authenticationService = new NotificationDecorator(_authenticationService, _notification);

            _authenticationService = new FailedCounterDecorator(_authenticationService, _failedCounter);

            _authenticationService = new LogDecorator(_authenticationService, _failedCounter, _logger);

            var isValid = _authenticationService.Verify("joey", "abc", "123456");
            Console.WriteLine(isValid);
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