namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
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