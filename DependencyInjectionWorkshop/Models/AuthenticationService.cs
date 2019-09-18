﻿using System;
using NLog;

namespace DependencyInjectionWorkshop.Models
{
    public interface IAuthentication
    {
        bool Verify(string accountId, string password, string otp);
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IProfile _profile;

        public AuthenticationService(IHash hash, IOtpService otpService, IProfile profile)
        {
            _hash = hash;
            _otpService = otpService;
            _profile = profile;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
        }

        public bool Verify(string accountId, string password, string otp)
        {
            var currentPassword = _profile.GetPassword(accountId);

            var hashedPassword = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(accountId);

            if (currentPassword == hashedPassword && otp == currentOtp)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}