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
            _logger = new NLogAdapter();
            _notification = new SlackAdapter();
            _failedCounter = new FailedCounter();
            _otpService = new OtpService();
            _hash = new Sha256Adapter();
            _profile = new ProfileDao();

            _authenticationService =
                new AuthenticationService(_profile, _hash, _otpService);

            _authenticationService = new NotificationDecorator(_authenticationService, _notification);

            _authenticationService = new FailedCounterDecorator(_authenticationService, _failedCounter);

            _authenticationService = new LogDecorator(_authenticationService, _failedCounter, _logger);
        }
    }
}