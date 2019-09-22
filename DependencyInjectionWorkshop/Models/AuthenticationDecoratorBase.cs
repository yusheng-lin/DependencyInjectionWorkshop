namespace DependencyInjectionWorkshop.Models
{
    public abstract class AuthenticationDecoratorBase : IAuthentication
    {
        private readonly IAuthentication _authentication;

        protected AuthenticationDecoratorBase(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string account, string password, string otp)
        {
            return this._authentication.Verify(account, password, otp);
        }
    }
}