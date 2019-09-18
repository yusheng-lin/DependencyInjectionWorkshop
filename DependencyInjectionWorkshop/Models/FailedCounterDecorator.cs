namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authenticationService, IFailedCounter failedCounter) : base(
            authenticationService)
        {
            _failedCounter = failedCounter;
        }

        public void AddFailedCount(string accountId)
        {
            _failedCounter.Add(accountId);
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            CheckAccountIsLocked(accountId);

            var isValid = base.Verify(accountId, password, otp);
            if (isValid)
            {
                Reset(accountId);
            }
            else
            {
                AddFailedCount(accountId);
            }

            return isValid;
        }

        private void CheckAccountIsLocked(string accountId)
        {
            if (_failedCounter.IsAccountLocked(accountId))
            {
                throw new FailedTooManyTimesException();
            }
        }

        private void Reset(string accountId)
        {
            _failedCounter.Reset(accountId);
        }
    }
}