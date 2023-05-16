using Microsoft.Extensions.Logging;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;
using PostSharp.Serialization;
using Utils.logger;

namespace mutation_app.Monitoring;

[OnMethodBoundaryAspectConfiguration(SerializerType=typeof(MsilAspectSerializer))]
public class MethodStats: LogAspect
{
    public MethodStats(bool isInDebugMode = false) : base(Logger.GetLogger(), Metrics.GetMetrics(), isInDebugMode)
    {
    }
}