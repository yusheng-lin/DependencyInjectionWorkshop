using System;
using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class FailedCounterDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter) : base(
            authenticationService)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);
            return base.Verify(accountId, password, otp);
        }

        private void CheckAccountIsLocked(string accountId)
        {
            if (_failedCounter.IsAccountLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IFailedCounter _failedCounter;
        private readonly FailedCounterDecorator _failedCounterDecorator;
        private readonly IHash _hash;
        private readonly ILogger _logger;
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;

        public AuthenticationService(IFailedCounter failedCounter, IHash hash, ILogger logger, IOtpService otpService,
            IProfile profile)
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            _failedCounter = failedCounter;
            _hash = hash;
            _logger = logger;
            _otpService = otpService;
            _profile = profile;
        }

        public AuthenticationService()
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _logger = new NLogAdapter();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            //_failedCounterDecorator.CheckAccountIsLocked(accountId);

            var currentPassword = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (currentPassword == hashedPassword && otp == currentOtp)
            {
                _failedCounter.Reset(accountId);

                return true;
            }
            else
            {
                _failedCounter.Add(accountId);

                int failedCount = _failedCounter.Get(accountId);
                _logger.Info($"accountId:{accountId} failed times:{failedCount}");

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}