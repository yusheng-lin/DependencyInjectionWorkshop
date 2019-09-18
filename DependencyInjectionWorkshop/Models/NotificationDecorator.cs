namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationBaseDecorator : IAuthentication
    {
        protected IAuthentication _authentication;
        public bool Verify(string accountId, string password, string otp)
        {
            throw new System.NotImplementedException();
        }
    }

    public class NotificationDecorator : AuthenticationBaseDecorator
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var isValid = _authentication.Verify(accountId, password, otp);
            if (!isValid)
            {
                Notify(accountId);
            }

            return isValid;
        }

        private void Notify(string accountId)
        {
            _notification.Notify(accountId);
        }
    }
}