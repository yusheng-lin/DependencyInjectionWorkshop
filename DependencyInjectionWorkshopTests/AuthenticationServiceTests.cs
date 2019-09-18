using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultAccountId = "joey";
        private AuthenticationService _authenticationService;
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
                new AuthenticationService(_failedCounter, _hash, _logger, _notification, _otpService, _profile);
        }

        [Test]
        public void is_valid()
        {
            _profile.GetPassword(DefaultAccountId).Returns("my hashed password");
            _hash.Compute("abc").Returns("my hashed password");
            _otpService.GetCurrentOtp(DefaultAccountId).Returns("123456");

            var isValid = _authenticationService.Verify(DefaultAccountId, "abc", "123456");
            Assert.IsTrue(isValid);
        }
    }
}