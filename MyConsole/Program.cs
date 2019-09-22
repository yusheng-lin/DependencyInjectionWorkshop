﻿using Autofac;
using Autofac.Extras.DynamicProxy;
using DependencyInjectionWorkshop.Models;
using System;

namespace MyConsole
{
    class Program
    {
        private static IContainer _container;
        private static IAuthentication _authentication;

        static void Main(string[] args)
        {
            RegisterContainer();

            Console.WriteLine("who are you?");
            var name = Console.ReadLine();
            var context = _container.Resolve<IContext>();
            context.SetCurrentUser(new Account() { Name = name });

            _authentication = _container.Resolve<IAuthentication>();

            var isValid = _authentication.Verify("joey", "abc", "wrong otp");
        }

        private static void RegisterContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<FakeProfile>().As<IProfile>();
            builder.RegisterType<FakeHash>().As<IHash>();
            builder.RegisterType<FakeOtp>().As<IOtpService>();
            builder.RegisterType<FakeLogger>().As<ILogger>();
            builder.RegisterType<FakeSlack>().As<INotification>();
            builder.RegisterType<FakeFailedCounter>().As<IFailedCounter>();
            builder.RegisterType<AuthenticationService>()
                .As<IAuthentication>()
                .EnableInterfaceInterceptors()
                .InterceptedBy(typeof(AuditLogInterceptor));

            builder.RegisterType<AuditLogInterceptor>();
            builder.RegisterType<MyContext>().As<IContext>().SingleInstance();
            builder.RegisterDecorator<NotificationDecorator, IAuthentication>();
            builder.RegisterDecorator<FailedCounterDecorator, IAuthentication>();
            builder.RegisterDecorator<LogFailedCountDecorator, IAuthentication>();

            var container = builder.Build();
            _container = container;
        }
    }
}

internal class FakeLogger : ILogger
{
    public void LogInfo(string message)
    {
        Console.WriteLine(message);
    }
}

internal class FakeSlack : INotification
{
    public void PushMessage(string message)
    {
        Console.WriteLine(message);
    }

    public void Send(string accountId)
    {
        PushMessage($"{nameof(Send)}, accountId:{accountId}");
    }
}

internal class FakeFailedCounter : IFailedCounter
{
    public void ResetFailedCount(string accountId)
    {
        Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(ResetFailedCount)}({accountId})");
    }

    public void AddFailedCount(string accountId)
    {
        Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(AddFailedCount)}({accountId})");
    }

    public bool GetAccountIsLocked(string accountId)
    {
        return IsAccountLocked(accountId);
    }

    public int GetFailedCount(string accountId)
    {
        Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
        return 91;
    }

    public bool IsAccountLocked(string accountId)
    {
        Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
        return false;
    }
}

internal class FakeOtp : IOtpService
{
    public string GetCurrentOtp(string accountId)
    {
        Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
        return "123456";
    }
}

internal class FakeHash : IHash
{
    public string Compute(string plainText)
    {
        Console.WriteLine($"{nameof(FakeHash)}.{nameof(Compute)}({plainText})");
        return "my hashed password";
    }
}

internal class FakeProfile : IProfile
{
    public string GetPassword(string accountId)
    {
        Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
        return "my hashed password";
    }
}


public interface IContext
{
    Account GetCurrentUser();
    void SetCurrentUser(Account account);
}

public class MyContext : IContext
{
    private Account _account;

    public Account GetCurrentUser()
    {
        return _account;
    }

    public void SetCurrentUser(Account account)
    {
        _account = account;
    }
}

