using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace mutation_app.src.Monitoring;

public class Logger
{
    private static ILogger? _instance;
    private static SeekerEnvs _envVariables = SeekerEnvs.GetEnvs();

    public static ILogger GetLogger()
    {
        if (_instance == null)
        {
            _instance = LoggerFactory.Create(builder => builder.AddOpenTelemetry((opt) =>
            {
                opt.IncludeFormattedMessage = true;
                opt.IncludeScopes = true;
                AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                opt.AddOtlpExporter(options => { options.Endpoint = new Uri(_envVariables.OptlCollector); });
            }).AddConsole().SetMinimumLevel(LogLevel.Debug)).CreateLogger<Program>();
        }

        return _instance;
    }
}