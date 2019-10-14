namespace DependencyInjectionWorkshop.Models
{
    public class LogMethodDecorator :IAuthentication
    {
        private readonly ILogger _logger;
        private readonly IAuthentication _authentication;
        private readonly ISessionData _sessionData;

        public LogMethodDecorator(ILogger logger, IAuthentication authentication, ISessionData sessionData)
        {
            _logger = logger;
            _authentication = authentication;
            _sessionData = sessionData;
        }

        public bool Verify(string account, string password, string otp)
        {
            this._logger.LogInfo($"{_sessionData.UserInfo.Account} invoke verify with parameters:{account}|{password}|{otp}");
            var isValid = this._authentication.Verify(account, password, otp);
            this._logger.LogInfo($"isValid:{isValid}");
            return isValid;
        }
    }

    public interface ISessionData
    {
        UserInfo UserInfo { get; set; }
    }

    public class UserInfo
    {
        public string Account;
    }
}