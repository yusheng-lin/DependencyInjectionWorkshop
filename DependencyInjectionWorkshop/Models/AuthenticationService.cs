using System;
using System.Net.Http;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IUserService _userService;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;
        private readonly IMessenger _messenger;
        private readonly IFailedCount _failedCount;

        public AuthenticationService(IUserService userService, IHash hash, IOtpService otpService, ILogger logger, IMessenger messenger, IFailedCount failedCount)
        {
            _userService = userService;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
            _messenger = messenger;
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
                ////重設帳號登入錯誤紀錄
                _failedCount.ResetFailedCount(account);
                return true;
            }
            else
            {
                ////累計登入錯誤次數
                _failedCount.AddFailedCount(account);

                var failedCount = _failedCount.GetFailedCount(account);

                ////寫Log
                _logger.LogInfo($"accountId:{account} failed times:{failedCount}");

                ////(email/slack)通知使用者登入失敗
                _messenger.PushMessage(account);
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}