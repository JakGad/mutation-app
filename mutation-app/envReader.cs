using System.Net;
using System.Text.Json;
using Antlr4.Runtime.Misc;
using Microsoft.Extensions.Logging;
using mutation_app.Monitoring;
using Utils.logger;

namespace mutation_app;

internal struct QueueConnectData
{
    public string hostName;
    public uint port;
    public string userName;
    public string password;

    public static QueueConnectData? Anonymize(QueueConnectData? data)
    {
        if (data.HasValue)
            return new QueueConnectData()
            {
                hostName = data.Value.hostName,
                password = new string(data.Value.password.Select(c => '*').ToArray()),
                port = data.Value.port,
                userName = data.Value.userName,
            };

        return null;
    }
}

internal static class EnvReader
{
    private static ILogger _logger = Logger.GetLogger();
    private static bool IsValidPortNumber(string? portToCheck, out uint? number)
    {
        if (uint.TryParse(portToCheck, out var portNumber) && portNumber >= IPEndPoint.MinPort &&
            portNumber <= IPEndPoint.MaxPort)
        {
            number = portNumber;
            return true;
        }

        number = null;
        return false;
    }

    private static void LogConnectionData(QueueConnectData? data)
    {
        _logger.LogInformation(JsonSerializer.Serialize( new { message = "Received queue connection data", credentials = QueueConnectData.Anonymize(data) }));
    } 
    
    public static QueueConnectData? GetQueueCredentials()
    {
        string? hostName = Environment.GetEnvironmentVariable("RABBIT_HOST");
        string? port = Environment.GetEnvironmentVariable("RABBIT_PORT");
        string? username = Environment.GetEnvironmentVariable("RABBIT_USERNAME");
        string? password = Environment.GetEnvironmentVariable("RABBIT_PASSWORD");

        QueueConnectData? dataToReturn = null;
        if (!string.IsNullOrWhiteSpace(hostName)
            && IsValidPortNumber(port, out var portNumber) && portNumber.HasValue
            && !string.IsNullOrWhiteSpace(username)
            && !string.IsNullOrWhiteSpace(password))
            dataToReturn =  new QueueConnectData()
            {
                hostName = hostName,
                password = password,
                port = portNumber.Value,
                userName = username
            };

        LogConnectionData(dataToReturn);
        return dataToReturn;
    }
}