namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string account, string password, string otp)
        {
            if (_failedCounter.IsAccountLocked(account))
            {
                throw new FailedTooManyTimesException();
            }

            var isValid = base.Verify(account, password, otp);

            if (isValid)
                this._failedCounter.ResetFailedCount(account);
            else
                this._failedCounter.AddFailedCount(account);

            return isValid;
        }
    }
}