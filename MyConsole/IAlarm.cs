using System;

namespace MyConsole
{
    internal interface IAlarm
    {
        void Raise(string roleId, Exception exception);
    }
}