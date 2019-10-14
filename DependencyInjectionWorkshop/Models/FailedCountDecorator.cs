namespace DependencyInjectionWorkshop.Models
{
    public class FailedCountDecorator : IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly IFailedCount _failedCounter;

        public FailedCountDecorator(IAuthentication authenticationService, IFailedCount failedCounter)
        {
            _authenticationService = authenticationService;
            _failedCounter = failedCounter;
        }

        private void ReSetFailedCount(string account)
        {
            _failedCounter.ResetFailedCount(account);
        }

        public bool Verify(string account, string password, string otp)
        {
            ////檢查帳號是否登入失敗太多次，被鎖
            var isLocked = _failedCounter.GetAccountIsLocked(account);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var isValid = this._authenticationService.Verify(account, password, otp);

            if (isValid)
            {
                this.ReSetFailedCount(account);
            }
            else
            {
                ////累計登入錯誤次數
                _failedCounter.AddFailedCount(account);
            }

            return isValid;
        }
    }
}