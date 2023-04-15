namespace Utils.logger;

public class SimpleLogger: AbstractLogger
{
    public override void DebugEvent(string message, string location, object data)
    {
        Console.WriteLine(getLogObject(message, location, "debug", data));
    }

    public override void InfoEvent(string message, string location, object data)
    {
        Console.WriteLine(getLogObject(message, location, "info", data));
    }

    public override void WarningEvent(string message, string location, object data)
    {
        Console.WriteLine(getLogObject(message, location, "warn", data));
    }

    public override void ErrorEvent(string message, string location, object data)
    {
        Console.WriteLine(getLogObject(message, location, "error", data));
    }
}