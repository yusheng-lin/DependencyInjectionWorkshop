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
        private readonly IFailedCount _failedCount;

        public AuthenticationService(IUserService userService, IHash hash, IOtpService otpService, IFailedCount failedCount)
        {
            _userService = userService;
            _hash = hash;
            _otpService = otpService;
            _failedCount = failedCount;
        }

        public bool Verify(string account, string password, string otp)
        {
            ////檢查帳號是否登入失敗太多次，被鎖
            var isLocked = _failedCount.GetAccountIsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            ////依帳號取得DB內hash過的密碼
            var passwordFromDb = _userService.GetPassword(account);

            ////hash使用者輸入的密碼
            var hashedPassword = _hash.GetHashedPassword(password);

            ////取得當下使用者的otp
            var currentOtp = _otpService.GetCurrentOtp(account);

            ////檢查使用者密碼/otp是否正確
            if (passwordFromDb == hashedPassword && otp == currentOtp)
            {
                return true;
            }
            else
            {
                ////累計登入錯誤次數
                _failedCount.AddFailedCount(account);
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}