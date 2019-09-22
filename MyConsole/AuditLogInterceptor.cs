using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;

namespace MyConsole
{
    public class AuditLogInterceptor : IInterceptor
    {
        private readonly ILogger _logger;

        public AuditLogInterceptor(ILogger logger)
        {
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            var parameters = string.Join("|", invocation.Arguments.Select(arg => arg ?? ""));

            this._logger.LogInfo($"parameters:{parameters}");

            invocation.Proceed();

            var result = invocation.ReturnValue;

            this._logger.LogInfo($"result:{result}");
        }
    }
}
