using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace mutation_app.Monitoring;

public class Logger
{
    private static ILogger? _instance;

    public static ILogger GetLogger()
    {
        if (_instance == null)
        {
            _instance = LoggerFactory.Create(builder => builder.AddOpenTelemetry((opt) =>
            {
                opt.IncludeFormattedMessage = true;
                opt.IncludeScopes = true;
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                opt.AddOtlpExporter();
            }).AddConsole()).CreateLogger<Program>();
        }
        
        return _instance;
    }
}