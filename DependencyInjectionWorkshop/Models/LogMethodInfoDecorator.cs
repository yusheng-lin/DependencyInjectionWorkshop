using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionWorkshop.Models
{
    public class LogMethodInfoDecorator : AuthenticationDecoratorBase
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authentication,ILogger logger) : base(authentication)
        {
            this._logger = logger;
        }

        public override bool Verify(string account, string password, string otp)
        {
            this._logger.LogInfo($"{nameof(account)}:{account},{nameof(password)};{password},{nameof(otp)}:{otp}");

            var isValid = base.Verify(account, password, otp);

            this._logger.LogInfo($"isValid:{isValid}");

            return isValid;
        }
    }
}
