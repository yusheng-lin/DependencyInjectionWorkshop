using System;
using System.Net.Http;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;
        private readonly IMessenger _messenger;
        private readonly IFailedCount _failedCount;

        public AuthenticationService(IUserService userService, IHash hash, IOtpService otpService, ILogger logger, IMessenger messenger, IFailedCount failedCount)
        {
            _userService = userService;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
            _messenger = messenger;
            _failedCount = failedCount;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCount.GetAccountIsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
            var passwordFromDb = _userService.GetPassword(account);

            var hashedPassword = _hash.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(account);

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCount.ResetFailedCount(account);
                return true;
            }
            else
            {
                _failedCount.AddFailedCount(account);

                var failedCount = _failedCount.GetFailedCount(account);
                _logger.LogInfo($"accountId:{account} failed times:{failedCount}");

                _messenger.PushMessage(account);
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}