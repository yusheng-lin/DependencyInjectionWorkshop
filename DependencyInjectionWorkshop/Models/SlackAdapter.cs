using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void Send(string account);
    }

    public class SlackAdapter : INotification
    {
        public SlackAdapter()
        {
        }

        public void Send(string account)
        {
            var message = $"{account} try to login failed";
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}