using Dapper;
using SlackAPI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDao
    {
        public string GetPassword(string account)
        {
            using (var connection = new SqlConnection("my connection string"))
            {
                return connection.Query<string>("spGetUserPassword", new {Id = account},
                        commandType: CommandType.StoredProcedure)
                    .SingleOrDefault();
            }
        }
    }

    public class Sha256Adapter
    {
        public Sha256Adapter()
        {
        }

        public string GetHashedPassword(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
    }

    public class OtpService
    {
        public OtpService()
        {
        }

        public string GetCurrentOpt(string account, HttpClient httpClient)
        {
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;

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

    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;

        public AuthenticationService(ProfileDao profileDao)
        {
            _profileDao = profileDao;
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
        }

        //帳號 密碼 otp
        public bool Verify(string account, string password, string otp)
        {
            //get password
            //hash
            //get otp
            //compare hash and opt
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            if (IsAccountLocked(account, httpClient))
            {
                throw new FailedTooManyTimesException();
            }

            var dbPassword = _profileDao.GetPassword(account);

            var hashedPassword = _sha256Adapter.GetHashedPassword(password);

            var currentOpt = _otpService.GetCurrentOpt(account, httpClient);

            if (dbPassword != hashedPassword.ToString() || otp != currentOpt)
            {
                AddFailedCount(account, httpClient);
                LogMessage(account, GetFailedCount(account, httpClient));
                _slackAdapter.NotifyUser(account);
                return false;
            }

            RestFailedCount(account, httpClient);
            return true;
        }

        private static void RestFailedCount(string account, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static void LogMessage(string account, int failedCount)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount}");
        }

        private static int GetFailedCount(string account, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static void AddFailedCount(string account, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static bool IsAccountLocked(string account, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();

            return isLockedResponse.Content.ReadAsAsync<bool>().Result;
        }
    }
}