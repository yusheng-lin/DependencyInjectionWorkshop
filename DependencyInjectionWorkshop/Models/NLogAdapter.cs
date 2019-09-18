namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void LogFailedCount(string message);
    }

    public class NLogAdapter : ILogger
    {
        public NLogAdapter()
        {
        }

        public void LogFailedCount(string message)
        {
            NLog.LogManager.GetCurrentClassLogger().Info(message);
        }
    }
}