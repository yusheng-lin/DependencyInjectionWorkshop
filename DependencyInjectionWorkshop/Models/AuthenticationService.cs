using Dapper;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;

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
                passwordHash = connection.Query<string>("spGetUserPassword", new { Id = account },
                commandType: CommandType.StoredProcedure).SingleOrDefault();
             
            }

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            if (passwordHash != hash.ToString()) return false;

            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;

            var currentOpt = string.Empty;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            currentOpt = response.Content.ReadAsAsync<string>().Result;

            if (otp != currentOpt) return false;

            return true;
        }
    }
}