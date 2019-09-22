namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isValid = this._authentication.Verify(account, password, otp);

            if (!isValid) this.Send(account);

            return isValid;
        }

        private void Send(string account)
        {
            _notification.Send(account);
        }
    }
}