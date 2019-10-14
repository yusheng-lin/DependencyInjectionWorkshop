namespace DependencyInjectionWorkshop.Models
{
    public class FailedCountDecorator : IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly IFailedCount _failedCounter;

        public FailedCountDecorator(IAuthentication authenticationService, IFailedCount failedCounter)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
        }

        private void ReSetFailedCount(string account)
        {
            _failedCounter.ResetFailedCount(account);
        }

        public bool Verify(string account, string password, string otp)
        {
            var isValid = this._authenticationService.Verify(account, password, otp);

            if (isValid)
            {
                this.ReSetFailedCount(account);
            }

            return isValid;
        }
    }
}