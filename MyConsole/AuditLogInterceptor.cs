using System.Linq;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    internal class AuditLogInterceptor : IInterceptor
    {
        private readonly ILogger _logger;
        private readonly IContext _context;

        public AuditLogInterceptor(ILogger logger, IContext context)
        {
            _logger = logger;
            _context = context;
        }

        public void Intercept(IInvocation invocation)
        {
            string currentUser = _context.GetCurrentUser();
            var message = $"{currentUser} invoke method - {invocation.TargetType.FullName}.{invocation.Method.Name}:" +
                $"{string.Join("|", invocation.Arguments.Select(x => (x ?? "").ToString()))}";

            _logger.Info(message);

            invocation.Proceed();

            _logger.Info($"result value is {invocation.ReturnValue}");
        }
    }
}