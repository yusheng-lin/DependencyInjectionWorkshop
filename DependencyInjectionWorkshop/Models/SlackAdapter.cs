using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface IMessenger
    {
        void PushMessage(string account);
    }

    public class SlackAdapter : IMessenger
    {
        public void PushMessage(string account)
        {
            var slackClient = new SlackClient("my api token");
            string message = $"{account} login failed";
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}