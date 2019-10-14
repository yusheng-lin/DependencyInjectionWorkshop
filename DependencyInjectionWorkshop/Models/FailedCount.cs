using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCount
    {
        void AddFailedCount(string account);
        void ResetFailedCount(string account);
        [AuditLog]
        bool GetAccountIsLocked(string account);
        int GetFailedCount(string account);
    }

    public class AuditLogAttribute : Attribute
    {
    }

    public class FailedCount : IFailedCount
    {
        public void AddFailedCount(string account)
        {
            var addFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", account).Result;

            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public void ResetFailedCount(string account)
        {
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", account).Result;

            resetResponse.EnsureSuccessStatusCode();
        }

        public bool GetAccountIsLocked(string account)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        public int GetFailedCount(string account)
        {
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }
    }
}