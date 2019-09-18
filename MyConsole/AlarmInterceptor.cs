using System;
using Castle.DynamicProxy;
using DependencyInjectionWorkshop;

namespace MyConsole
{
    internal class AlarmInterceptor : IInterceptor
    {
        private readonly IAlarm _alarm;
        //private readonly string _supportId = "911";

        public AlarmInterceptor(IAlarm alarm)
        {
            _alarm = alarm;
        }

        public void Intercept(IInvocation invocation)
        {
            if (Attribute.GetCustomAttribute(invocation.Method, typeof(AlarmAttribute)) is AlarmAttribute alarmAttribute
            )
            {
                string roleId = alarmAttribute.RoleId;
                try
                {
                    invocation.Proceed();
                }
                catch (Exception e)
                {
                    _alarm.Raise(roleId, e);
                    throw;
                }
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}