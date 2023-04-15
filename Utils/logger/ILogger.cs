using System.Text.Json;
using System.Text.Json.Nodes;

namespace Utils.logger;

public abstract class AbstractLogger
{
    protected string getLogObject(string message, string location, string level, object data)
    {
        JsonNode json = JsonSerializer.SerializeToNode(data);
        json["message"] = message;
        json["location"] = location;
        json["level"] = level;
        json["timestamp"] = DateTime.UtcNow.ToString();
        json["app"] = AppDomain.CurrentDomain.FriendlyName;
        return json.ToJsonString();
    }

    public abstract void DebugEvent(string message, string location, object data);
    public abstract void InfoEvent(string message, string location, object data);
    public abstract void WarningEvent(string message, string location, object data);
    public abstract void ErrorEvent(string message, string location, object data);
}