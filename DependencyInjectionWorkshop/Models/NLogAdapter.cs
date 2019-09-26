﻿namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
    {
        public void LogInfo(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}