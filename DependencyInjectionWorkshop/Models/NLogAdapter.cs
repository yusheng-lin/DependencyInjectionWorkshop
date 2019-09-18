namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void Info(string message);
    }

    public class NLogAdapter : ILogger
    {
        public NLogAdapter()
        {
        }

        public void Info(string message)
        {
            NLog.LogManager.GetCurrentClassLogger().Info(message);
        }
    }
}