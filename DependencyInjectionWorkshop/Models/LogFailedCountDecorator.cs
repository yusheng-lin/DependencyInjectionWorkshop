namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator: IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly IFailedCount _failedCounter;
        private readonly ILogger _logger;

        public LogFailedCountDecorator(IAuthentication authenticationService, IFailedCount failedCounter, ILogger logger)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
            _logger = logger;
        }

        private void LogFailedCount(string account)
        {
            var failedCount = _failedCounter.GetFailedCount(account);

            ////寫Log
            _logger.LogInfo($"accountId:{account} failed times:{failedCount}");
        }

        public bool Verify(string account, string password, string otp)
        {
            var isValid = this._authenticationService.Verify(account, password, otp);

            if (!isValid)
            {
                this.LogFailedCount(account);
            }

            return isValid;
        }
    }
}