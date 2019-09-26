using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class SlackAdapter
    {
        public void PushMessage(string account)
        {
            var slackClient = new SlackClient("my api token");
            string message = $"{account} login failed";
            slackClient.PostMessage(response1 => { }, "my channel", message, "my bot name");
        }
    }
}