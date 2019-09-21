using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;
        private readonly FailCounter _failCounter;
        private readonly NLogAdapter _nLogAdapter;

        public AuthenticationService(
            IProfile profile, 
            Sha256Adapter sha256Adapter, 
            OtpService otpService, 
            SlackAdapter slackAdapter, 
            FailCounter failCounter, 
            NLogAdapter nLogAdapter)
        {
            _profile = profile;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _slackAdapter = slackAdapter;
            _failCounter = failCounter;
            _nLogAdapter = nLogAdapter;
        }

        //帳號 密碼 otp
        public bool Verify(string account, string password, string otp)
        {
            //get password
            //hash
            //get otp
            //compare hash and opt
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            if (_failCounter.IsAccountLocked(account))
            {
                throw new FailedTooManyTimesException();
            }

            var dbPassword = _profile.GetPassword(account);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOpt = _otpService.GetCurrentOpt(account);

            if (dbPassword != hashedPassword.ToString() || otp != currentOpt)
            {
                _failCounter.AddFailedCount(account);
                _nLogAdapter.LogMessage(account, _failCounter.GetFailedCount(account));
                _slackAdapter.NotifyUser(account);
                return false;
            }

            _failCounter.RestFailedCount(account);
            return true;
        }
    }
}