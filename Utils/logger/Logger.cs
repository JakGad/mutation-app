using System.Runtime.CompilerServices;

namespace Utils.logger;

public delegate void LogInfo(string message, string location, object data);

public interface ILogger
{
    public void Debug(string message, string location, object data);
    public void Info(string message, string location, object data);
    public void Warning(string message, string location, object data);
    public void Error(string message, string location, object data);
}

public class SubscribableLogger : ILogger
{
    public void SubscribeLogger(AbstractLogger loggerToSubscribe)
    {
        DebugEvent += loggerToSubscribe.DebugEvent;
        InfoEvent += loggerToSubscribe.InfoEvent;
        WarningEvent += loggerToSubscribe.WarningEvent;
        ErrorEvent += loggerToSubscribe.ErrorEvent;
    }

    public event LogInfo? DebugEvent;
    public event LogInfo? InfoEvent;
    public event LogInfo? WarningEvent;
    public event LogInfo? ErrorEvent;


    public void Debug(string message, string location, object data)
    {
        DebugEvent?.Invoke(message, location, data);
    }

    public void Info(string message, string location, object data)
    {
        InfoEvent?.Invoke(message, location, data);
    }

    public void Warning(string message, string location, object data)
    {
        WarningEvent?.Invoke(message, location, data);
    }

    public void Error(string message, string location, object data)
    {
        ErrorEvent?.Invoke(message, location, data);
    }
}