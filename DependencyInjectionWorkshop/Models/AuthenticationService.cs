using System;
using System.Net.Http;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly UserDal _userDal = new UserDal();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();

        public bool Verify(string account, string password, string otp)
        {
            var isLocked = GetAccountIsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }
            var passwordFromDb = _userDal.GetPasswordFromDb(account);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOtp = _otpService.GetCurrentOtp(account, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });

            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                ResetFailedCount(account, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });
                return true;
            }
            else
            {
                AddFailedCount(account, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });

                var failedCount = GetFailedCount(account, new HttpClient() { BaseAddress = new Uri("http://joey.com/") });
                _nLogAdapter.LogInfo($"accountId:{account} failed times:{failedCount}");

                _slackAdapter.PushMessage(account);
                return false;
            }
        }

        private static int GetFailedCount(string account, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static bool GetAccountIsLocked(string account)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        private static void AddFailedCount(string account, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;

            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCount(string account, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;

            resetResponse.EnsureSuccessStatusCode();
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}