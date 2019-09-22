namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public LogFailedCountDecorator(IAuthentication authentication, IFailedCounter failedCounter, ILogger logger) : base(authentication)
        {
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var isValid = base.Verify(account, password, otp);

            if (!isValid) _logger.LogInfo($"accountId:{account} failed times:{_failedCounter.GetFailedCount(account)}");

            return isValid;
        }
    }
}