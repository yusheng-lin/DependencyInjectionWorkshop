namespace DependencyInjectionWorkshop.Models
{
    public class AddFailedCountDecorator : AuthenticationDecoratorBase
    {
        private readonly IFailedCounter _failedCounter;

        public AddFailedCountDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var inValid = base.Verify(account, password, otp);

            if (!inValid) this._failedCounter.AddFailedCount(account);

            return inValid;
        }
    }
}