using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using Utils.logger;

namespace mutation_seeker_orchestrator.src.Monitoring;

[OnMethodBoundaryAspectConfiguration(SerializerType = typeof(MsilAspectSerializer))]
public class MethodStats : LogAspect
{
    public MethodStats(bool isInDebugMode = false) : base(Logger.GetLogger(), Metrics.GetMetrics(), isInDebugMode)
    {
    }
}