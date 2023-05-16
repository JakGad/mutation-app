using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Utils.logger;

namespace mutation_app.Monitoring;


public interface IMetrics: LogAspect.IMetrics
{
}


public class Metrics: IMetrics
{
    public Histogram<long> SuccessfulMethodExecutionTime { get; set; }
    public Counter<long> MethodError { get; set; }
    
    
    private static Metrics? _instance;
    public static IMetrics GetMetrics()
    {
        if (_instance == null)
        {
            var metricsProvider = Sdk.CreateMeterProviderBuilder().AddMeter("mutation_observer_meter").AddOtlpExporter().Build();
            var meter = new Meter("mutation_observer_meter", "1.0");
            
            _instance = new Metrics();
            
            _instance.SuccessfulMethodExecutionTime = meter.CreateHistogram<long>("successful_method_execution_time", "ms");
            _instance.MethodError = meter.CreateCounter<long>("method_error");
        }

        return _instance;
    }
}