using Utils.logger;

namespace mutation_app;

public class Logger
{
    private static SubscribableLogger? _instance;

    public static ILogger GetLogger()
    {
        if (_instance == null)
        {
            _instance = new SubscribableLogger();
            _instance.SubscribeLogger(new SimpleLogger());
        }
        
        return _instance;
    }
}