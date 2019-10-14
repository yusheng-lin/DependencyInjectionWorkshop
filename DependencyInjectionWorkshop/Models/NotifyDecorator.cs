namespace DependencyInjectionWorkshop.Models
{
    public class NotifyDecorator: IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly IMessenger _messenger;

        public NotifyDecorator(IAuthentication authenticationService, IMessenger messenger)
        {
            _authenticationService = authenticationService;
            _messenger = messenger;
        }

        private void PushMessage(string account)
        {
            _messenger.PushMessage(account);
        }

        public bool Verify(string account, string password, string otp)
        {
            var isValid = this._authenticationService.Verify(account, password, otp);

            if (!isValid)
            {
                this.PushMessage(account);
            }

            return isValid;
        }
    }
}