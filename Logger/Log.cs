using System;
using NLog;

public static class Log
{
    static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void Trace(string message_)
    {
        Logger.Trace(message_);
    }

    public static void Debug(string message_)
    {
        Logger.Debug(message_);
    }

    public static void Info(string message_)
    {
        Logger.Info(message_);
    }

    public static void Info(string message_, bool email_)
    {
        Logger.Info(message_);
    }

    public static void Warn(string message_)
    {
        Logger.Warn(message_);
    }

    public static void Error(string message_)
    {
        Logger.Error(message_);
    }

    public static void Error(string message_, bool email_)
    {
        Logger.Error(message_);
    }

    public static void Exception(string message_, Exception ex_)
    {
        Logger.Log(LogLevel.Fatal, ex_);
    }

    public static void Exception(string message_, Exception ex_, bool email_)
    {
        Logger.Log(LogLevel.Fatal, ex_);
    }

    public static void Fatal(string message_)
    {
        Logger.Fatal(message_);
    }
}
