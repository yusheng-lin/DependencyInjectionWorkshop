using System;
using System.Net.Http;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string account, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IUserService _userService;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;

        public AuthenticationService(IUserService userService, IHash hash, IOtpService otpService)
        {
            _userService = userService;
            _hash = hash;
            _otpService = otpService;
        }

        public bool Verify(string account, string password, string otp)
        {

            ////依帳號取得DB內hash過的密碼
            var passwordFromDb = _userService.GetPassword(account);

            ////hash使用者輸入的密碼
            var hashedPassword = _hash.GetHashedPassword(password);

            ////取得當下使用者的otp
            var currentOtp = _otpService.GetCurrentOtp(account);

            ////檢查使用者密碼/otp是否正確
            return passwordFromDb == hashedPassword && otp == currentOtp;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}