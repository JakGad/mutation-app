using OpenTelemetry;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;
using Utils.logger;

namespace mutation_app.src.Monitoring;


public interface IMetrics : LogAspect.IMetrics
{
}


public class Metrics : IMetrics
{
    public Histogram<long> SuccessfulMethodExecutionTime { get; private set; }
    public Counter<long> MethodError { get; private set; }
    public Counter<long> Extensions { get; private set; }


    private static Metrics? _instance;
    public static Metrics GetMetrics()
    {
        if (_instance == null)
        {
            var metricsProvider = Sdk.CreateMeterProviderBuilder().AddMeter("mutation_observer_meter").AddOtlpExporter().Build();
            var meter = new Meter("mutation_observer_meter", "1.0");

            _instance = new Metrics();

            _instance.SuccessfulMethodExecutionTime = meter.CreateHistogram<long>("successful_method_execution_time", "ms");
            _instance.MethodError = meter.CreateCounter<long>("method_error");
            _instance.Extensions = meter.CreateCounter<long>("extensions");
        }

        return _instance;
    }
}