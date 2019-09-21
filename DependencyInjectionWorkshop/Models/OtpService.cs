using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IOtpService
    {
        string GetCurrentOpt(string account);
    }

    public class OtpService : IOtpService
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
}