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

        public AuthenticationService(IOtpService otpService,
            IProfile profile, IHash hash)
        {
            _otpService = otpService;
            _profile = profile;
            _hash = hash;
        }

        //帳號 密碼 otp
        public bool Verify(string account, string password, string otp)
        {
            var dbPassword = _profile.GetPassword(account);

            var hashedPassword = _hash.Compute(password);

            var currentOpt = _otpService.GetCurrentOtp(account);

            return dbPassword == hashedPassword && otp == currentOpt;
        }
    }
}