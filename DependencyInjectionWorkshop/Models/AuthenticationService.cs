using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        //帳號 密碼 otp
        public bool Verify(string account, string password, string otp)
        {
            //get password
            //hash
            //get otp
            //compare hash and opt

            var passwordHash = string.Empty;

            using (var connection = new SqlConnection("my connection string"))
            {
                passwordHash = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var httpClient = new HttpClient() {BaseAddress = new Uri("http://joey.com/")};
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;

            var currentOpt = string.Empty;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            currentOpt = response.Content.ReadAsAsync<string>().Result;

            if (passwordHash != hash.ToString() || otp != currentOpt)
            {
                var message = $"{account} try to login failed";
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
                return false;
            }

            return true;
        }
    }
}