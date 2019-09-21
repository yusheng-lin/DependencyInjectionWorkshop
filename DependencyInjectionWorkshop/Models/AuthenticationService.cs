using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly INotification _notification;
        private readonly IFailCounter _failCounter;
        private readonly ILogger _logger;

        public AuthenticationService(
            IProfile profile, 
            IHash hash, 
            IOtpService otpService, 
            INotification notification, 
            IFailCounter failCounter,
            ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _notification = notification;
            _failCounter = failCounter;
            _logger = logger;
        }

        //帳號 密碼 otp
        public bool Verify(string account, string password, string otp)
        {
            //get password
            //hash
            //get otp
            //compare hash and opt

            if (_failCounter.IsAccountLocked(account))
            {
                throw new FailedTooManyTimesException();
            }

            var dbPassword = _profile.GetPassword(account);

            var hashedPassword = _hash.Compute(password);

            var currentOpt = _otpService.GetCurrentOpt(account);

            if (dbPassword != hashedPassword || otp != currentOpt)
            {
                _failCounter.AddFailedCount(account);
                _logger.LogInfo(account, _failCounter.GetFailedCount(account));
                _notification.Send(account);
                return false;
            }

            _failCounter.RestFailedCount(account);
            return true;
        }
    }
}