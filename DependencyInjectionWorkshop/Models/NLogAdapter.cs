namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void LogInfo(string message);
    }

    public class NLogAdapter : ILogger
    {
        public void LogInfo(string message)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}