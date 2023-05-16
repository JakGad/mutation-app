// var builder = WebApplication.CreateBuilder(args);
// var app = builder.Build();
//
// app.MapGet("/", () => "Hello World!");
//
// app.Run();

using System.Text;
using System.Text.Json;
using CommunicationTypes;
using RabbitMQ.Client;

var factory = new ConnectionFactory { HostName = "localhost", UserName = "myuser", Password = "mypassword", Port = 5672};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "seeker-tasks",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

var message = new AnalyzeTask() { Url = "jg.com" };
var body = JsonSerializer.SerializeToUtf8Bytes<SeekerTask>(message);

var properties = channel.CreateBasicProperties();
properties.Persistent = true;

while (true)
{
    channel.BasicPublish(exchange: string.Empty,
        routingKey: "seeker-tasks",
        basicProperties: properties,
        body: body);
    Console.WriteLine($" [x] Sent {JsonSerializer.Serialize<SeekerTask>(message)}");

    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
}
