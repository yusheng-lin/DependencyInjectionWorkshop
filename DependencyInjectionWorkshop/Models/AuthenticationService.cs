using System;
using System.Net.Http;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly UserDal _userDal;
        private readonly IHash _hash;
        private readonly OtpService _otpService;
        private readonly NLogAdapter _nLogAdapter;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailedCount _failedCount;

        public AuthenticationService(UserDal userDal, IHash hash, OtpService otpService, NLogAdapter nLogAdapter, SlackAdapter slackAdapter, FailedCount failedCount)
        {
            _userDal = userDal;
            _hash = hash;
            _otpService = otpService;
            _nLogAdapter = nLogAdapter;
            _slackAdapter = slackAdapter;
            _failedCount = failedCount;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = _failedCount.GetAccountIsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
            var passwordFromDb = _userDal.GetPasswordFromDb(account);

            var hashedPassword = _hash.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(account, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                _failedCount.ResetFailedCount(account, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });
                return true;
            }
            else
            {
                _failedCount.AddFailedCount(account, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });

                var failedCount = _failedCount.GetFailedCount(account, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });
                _nLogAdapter.LogInfo($"accountId:{account} failed times:{failedCount}");

                _slackAdapter.PushMessage(account);
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}