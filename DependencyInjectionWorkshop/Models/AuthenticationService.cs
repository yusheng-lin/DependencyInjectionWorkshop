using SlackAPI;
using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public class OtpService
    {
        public OtpService()
        {
        }

        public string GetCurrentOpt(string account)
        {
            var response = new HttpClient().PostAsJsonAsync("api/otps", account).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            return response.Content.ReadAsAsync<string>().Result;
        }
    }

    public class SlackAdapter
    {
        public SlackAdapter()
        {
        }

        public void NotifyUser(string account)
        {
            var message = $"{account} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }

    public class FailCounter
    {
        public FailCounter()
        {
        }

        public void RestFailedCount(string account)
        {
            var resetResponse = new HttpClient().PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public int GetFailedCount(string account)
        {
            var failedCountResponse =
                new HttpClient().PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void AddFailedCount(string account)
        {
            var addFailedCountResponse = new HttpClient().PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public bool IsAccountLocked(string account)
        {
            var isLockedResponse = new HttpClient().PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();

            return isLockedResponse.Content.ReadAsAsync<bool>().Result;
        }
    }

    public class NLogAdapter
    {
        public NLogAdapter()
        {
        }

        public void LogMessage(string account, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount}");
        }
    }

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