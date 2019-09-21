﻿namespace DependencyInjectionWorkshop.Models
{
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
}