using System.Net.Configuration;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IHash _hash;
        private IUserService _userService;
        private ILogger _logger;
        private IFailedCount _failedCount;
        private IOtpService _otpService;
        private IMessenger _messenger;
        protected internal IAuthentication AuthenticationService;
        protected internal string DefaultAccount = "min";
        protected internal string HashedPassword = "my hashed password";
        protected internal string InputPassword = "pass.123";
        protected internal string CurrentOtp = "123456";
        protected internal int DefaultFailedCount = 88;

        [SetUp]
        public void SetUp()
        {
            _hash =Substitute.For<IHash>();
            _userService = Substitute.For<IUserService>();
            _logger = Substitute.For<ILogger>();
            _failedCount = Substitute.For<IFailedCount>();
            _otpService = Substitute.For<IOtpService>();
            _messenger = Substitute.For<IMessenger>();
            AuthenticationService = new AuthenticationService(_userService, _hash, _otpService);
            AuthenticationService= new LogFailedCountDecorator(AuthenticationService,_failedCount,_logger);
            AuthenticationService = new NotifyDecorator(AuthenticationService,_messenger);
            AuthenticationService = new FailedCountDecorator(AuthenticationService,_failedCount );
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(DefaultAccount, HashedPassword);
            GiveHashed(InputPassword, HashedPassword);
            GivenOtp(DefaultAccount, CurrentOtp);

            var isValid = WhenVerify(DefaultAccount, InputPassword, CurrentOtp);
            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid()
        {
            GivenPassword(DefaultAccount, HashedPassword);
            GiveHashed(InputPassword, HashedPassword);
            GivenOtp(DefaultAccount, CurrentOtp);

            var isValid = WhenVerify(DefaultAccount, InputPassword, "wrong otp");
            ShouldBeInValid(isValid);
        }


        [Test]
        public void throw_exception_when_account_locked()
        {
            GivenAccountIsLocked(DefaultAccount, true);
            ShouldThrowException();
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount();
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount();
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultAccount, DefaultFailedCount);
            WhenInvalid();
            ShouldContains(DefaultAccount, DefaultFailedCount);
        }

        [Test]
        public void notify_when_invalid()
        {
            WhenInvalid();
            ShouldPushMessage(DefaultAccount);
        }

        private void ShouldPushMessage(string defaultAccount)
        {
            _messenger.Received(1).PushMessage(defaultAccount);
        }

        private void ShouldContains(string defaultAccount, int defaultFailedCount)
        {
            _logger.Received(1)
                .LogInfo(Arg.Is<string>(msg => msg.Contains(defaultAccount) && msg.Contains(defaultFailedCount.ToString())));
        }

        private void GivenFailedCount(string defaultAccount, int defaultFailedCount)
        {
            _failedCount.GetFailedCount(defaultAccount).Returns(defaultFailedCount);
        }

        private void WhenValid()
        {
            GivenPassword(DefaultAccount, HashedPassword);
            GiveHashed(InputPassword, HashedPassword);
            GivenOtp(DefaultAccount, CurrentOtp);

            var isValid = WhenVerify(DefaultAccount, InputPassword, CurrentOtp);
            ShouldBeValid(isValid);
        }

        private void WhenInvalid()
        {
            GivenPassword(DefaultAccount, HashedPassword);
            GiveHashed(InputPassword, HashedPassword);
            GivenOtp(DefaultAccount, CurrentOtp);

            var isValid = WhenVerify(DefaultAccount, InputPassword, "wrong otp");
            ShouldBeInValid(isValid);
        }

        private void ShouldThrowException()
        {
            TestDelegate action = () => WhenVerify(DefaultAccount, InputPassword, CurrentOtp);
            Assert.Throws<FailedTooManyTimesException>(action);
        }

        private void GivenAccountIsLocked(string defaultAccount, bool isLocked)
        {
            _failedCount.GetAccountIsLocked(defaultAccount).Returns(isLocked);
        }


        private void ShouldAddFailedCount()
        {
            _failedCount.Received(1).AddFailedCount(DefaultAccount);
        }

        private void ShouldResetFailedCount()
        {
            _failedCount.Received(1).ResetFailedCount(DefaultAccount);
        }

        private static void ShouldBeInValid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }
        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private bool WhenVerify(string defaultAccount, string inputPassword, string currentOtp)
        {
            var isValid = AuthenticationService.Verify(defaultAccount, inputPassword, currentOtp);
            return isValid;
        }

        private void GivenOtp(string defaultAccount, string currentOtp)
        {
            _otpService.GetCurrentOtp(defaultAccount).Returns(currentOtp);
        }

        private void GiveHashed(string inputPassword, string hashedPassword)
        {
            _hash.GetHashedPassword(inputPassword).Returns(hashedPassword);
        }

        private void GivenPassword(string defaultAccount, string hashedPassword)
        {
            _userService.GetPassword(defaultAccount).Returns(hashedPassword);
        }
    }
}