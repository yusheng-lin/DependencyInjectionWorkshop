﻿using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
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
}