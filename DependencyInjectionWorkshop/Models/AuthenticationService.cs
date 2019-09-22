namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string account, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public AuthenticationService(IFailedCounter failedCounter, ILogger logger, IOtpService otpService,
            IProfile profile, IHash hash)
        {
            _failedCounter = failedCounter;
            _logger = logger;
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
        }

        //帳號 密碼 otp
        public bool Verify(string account, string password, string otp)
        {
            if (_failedCounter.IsAccountLocked(account))
            {
                throw new FailedTooManyTimesException();
            }

            var dbPassword = _profile.GetPassword(account);

            var hashedPassword = _hash.Compute(password);

            var currentOpt = _otpService.GetCurrentOpt(account);

            if (dbPassword != hashedPassword || otp != currentOpt)
            {
                _logger.LogInfo($"accountId:{account} failed times:{_failedCounter.GetFailedCount(account)}");
                return false;
            }

            return true;
        }
    }
}