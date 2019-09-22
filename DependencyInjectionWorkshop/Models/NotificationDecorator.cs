namespace DependencyInjectionWorkshop.Models
{
    public abstract class AuthenticationDecoratorBase : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected AuthenticationDecoratorBase(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string account, string password, string otp)
        {
            return this._authentication.Verify(account, password, otp);
        }
    }

    public class NotificationDecorator : AuthenticationDecoratorBase
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(authentication)
        {
            _notification = notification;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var isValid = base.Verify(account, password, otp);

            if (!isValid) this._notification.Send(account);

            return isValid;
        }
    }
}