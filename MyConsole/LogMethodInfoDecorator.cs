using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    internal class LogMethodInfoDecorator : AuthenticationBaseDecorator
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }

        public override bool Verify(string accountId, string password, string otp)
        {
            var message = $"{nameof(AuthenticationService)}.{nameof(Verify)}:{accountId} | {password} |{otp}";
            _logger.Info(message);

            var isValid = base.Verify(accountId, password, otp);

            _logger.Info($"{accountId} isValid: {isValid.ToString()}");

            return isValid;
        }
    }
}