using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "joey";
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultOtp = "123456";
        private const string DefaultInputPassword = "abc";
        private const int DefaultFailedCount = 666;
        private IAuthentication _authenticationService;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtpService _otpService;
        private IProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _logger = Substitute.For<ILogger>();
            _notification = Substitute.For<INotification>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _otpService = Substitute.For<IOtpService>();
            _hash = Substitute.For<IHash>();
            _profile = Substitute.For<IProfile>();

            _authenticationService =
                new AuthenticationService(_profile, _hash, _otpService);

            _authenticationService = new NotificationDecorator(_authenticationService, _notification);

            _authenticationService = new FailedCounterDecorator(_authenticationService, _failedCounter);

            _authenticationService = new LogDecorator(_authenticationService, _failedCounter, _logger);
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, "wrong otp");
            ShouldBeInvalid(isValid);
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotifyUser(DefaultAccountId);
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount(DefaultAccountId);
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultFailedCount);

            WhenInvalid();

            LogShouldContains(DefaultAccountId, DefaultFailedCount);
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            FailedCountShouldReset(DefaultAccountId);
        }

        [Test]
        public void when_account_is_locked()
        {
            GivenAccountIsLocked(true);
            ShouldThrow<FailedTooManyTimesException>(
                () => WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp));
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private void ShouldThrow<TException>(TestDelegate action) where TException : Exception
        {
            Assert.Throws<TException>(action);
        }

        private void GivenAccountIsLocked(bool isLocked)
        {
            _failedCounter.IsAccountLocked(DefaultAccountId).Returns(isLocked);
        }

        private void FailedCountShouldReset(string accountId)
        {
            _failedCounter.Received(1).Reset(accountId);
        }

        private bool WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            return isValid;
        }

        private void LogShouldContains(string accountId, int failedCount)
        {
            _logger.Received(1).Info(
                Arg.Is<string>(m => m.Contains(failedCount.ToString()) && m.Contains(accountId)));
        }

        private void GivenFailedCount(int failedCount)
        {
            _failedCounter.Get(DefaultAccountId).Returns(failedCount);
        }

        private void ShouldAddFailedCount(string accountId)
        {
            _failedCounter.Received(1).Add(accountId);
        }

        private void ShouldNotifyUser(string accountId)
        {
            _notification.Received(1).Notify(Arg.Is<string>(m => m.Contains(accountId)));
        }

        private bool WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, "wrong otp");
            return isValid;
        }

        private bool WhenVerify(string accountId, string inputPassword, string otp)
        {
            return _authenticationService.Verify(accountId, inputPassword, otp);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).Returns(otp);
        }

        private void GivenHash(string inputPassword, string hashedPassword)
        {
            _hash.Compute(inputPassword).Returns(hashedPassword);
        }

        private void GivenPassword(string accountId, string hashedPassword)
        {
            _profile.GetPassword(accountId).Returns(hashedPassword);
        }
    }
}