using PostSharp.Aspects;
using PostSharp.Serialization;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using PostSharp.Aspects.Configuration;
using PostSharp.Aspects.Serialization;

namespace Utils.logger;




[OnMethodBoundaryAspectConfiguration(SerializerType=typeof(MsilAspectSerializer))]
public class LogAspect: OnMethodBoundaryAspect
{
    public interface IMetrics
    {
        public Histogram<long> SuccessfulMethodExecutionTime { get; }
        public Counter<long> MethodError { get; }
    }
    
    private ILogger _logger;
    private IMetrics _metrics;
    private bool _isInDebugMode;
    private DateTime _entryTimeStamp;
    private string _id;

    public LogAspect(ILogger logger, IMetrics metrics, bool isInDebugMode = false)
    {
        this._logger = logger;
        this._metrics = metrics;
        this._isInDebugMode = true;
        _id = System.Guid.NewGuid().ToString();
    }
    
    public override void OnEntry(MethodExecutionArgs args)
    {
        _entryTimeStamp = DateTime.Now;
        
        if (_isInDebugMode)
        {
            
            _logger.LogInformation(JsonSerializer.Serialize(new
            {
                id = _id,
                method = args.Method.Name,
                // args = args.Arguments.Count,
                eventName = "entry"
            }));
        }
    }

    public override void OnSuccess(MethodExecutionArgs args)
    {
        _metrics.SuccessfulMethodExecutionTime.Record((DateTime.Now - _entryTimeStamp).Milliseconds,
            new KeyValuePair<string, object?>("method", args.Method.Name));
        if (_isInDebugMode)
        {
            _logger.LogInformation(JsonSerializer.Serialize(new
            {
                id = _id,
                method = args.Method.Name,
                returnedValue = args.ReturnValue,
                eventName = "success_exit",
                time = $"{(DateTime.Now - _entryTimeStamp).Milliseconds}ms",
            }));
        }
    }

    public override void OnException(MethodExecutionArgs args)
    {
        _metrics.MethodError.Add(1,  new KeyValuePair<string, object?>("method", args.Method.Name));
        _logger.LogError(args.Exception, JsonSerializer.Serialize(new {
            id = _id,
            method = args.Method.Name,
            eventName = "exception_exit",
            time = $"{(DateTime.Now - _entryTimeStamp).Milliseconds}ms",
        }));
    }
}