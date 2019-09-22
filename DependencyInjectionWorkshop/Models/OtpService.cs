using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IOtpService
    {
        string GetCurrentOtp(string account);
    }

    public class OtpService : IOtpService
    {
        public OtpService()
        {
        }

        public string GetCurrentOtp(string account)
        {
            var response = new HttpClient().PostAsJsonAsync("api/otps", account).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"web api error, accountId:{account}");
            }

            return response.Content.ReadAsAsync<string>().Result;
        }
    }
}