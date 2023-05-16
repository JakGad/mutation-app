using System.Text.Json;
using CommunicationTypes;
using mutation_app.Monitoring;
using RabbitMQ.Client.Events;

namespace mutation_app;

public class MessageAnalyzer
{
    [MethodStats]
    public void ProcessMessage(object? model, BasicDeliverEventArgs ea)
    {
        var body = ea.Body.ToArray();
        var message = JsonSerializer.Deserialize<SeekerTask>(body);
        Console.WriteLine(
            message switch
            {
                AnalyzeTask analyze => "analyze",
                _ => "other"
            }
        );
    }
}