using System;
using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class LogDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogDecorator(IAuthentication authenticationService, IFailedCounter failedCounter, ILogger logger) :
            base(authenticationService)
        {
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var isValid = base.Verify(accountId, password, otp);
            if (!isValid)
            {
                LogMessage(accountId);
            }

            return isValid;
        }

        private void LogMessage(string accountId)
        {
            int failedCount = _failedCounter.Get(accountId);
            _logger.Info($"accountId:{accountId} failed times:{failedCount}");
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly IHash _hash;
        private readonly LogDecorator _logDecorator;
        private readonly ILogger _logger;
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;

        public AuthenticationService(IFailedCounter failedCounter, IHash hash, ILogger logger, IOtpService otpService,
            IProfile profile)
        {
            //_logDecorator = new LogDecorator(this);
            _failedCounter = failedCounter;
            _hash = hash;
            _logger = logger;
            _otpService = otpService;
            _profile = profile;
        }

        public AuthenticationService()
        {
            //_logDecorator = new LogDecorator(this);
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _logger = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var currentPassword = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (currentPassword == hashedPassword && otp == currentOtp)
            {
                return true;
            }
            else
            {
                //_logDecorator.LogMessage(accountId);

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}