using System.Diagnostics.Metrics;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;
using CommunicationTypes;
using Microsoft.Extensions.Logging;
using mutation_app;
using mutation_app.Monitoring;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

const string serviceName = "mutation-seeker";
const string serviceVersion = "1.0.0";

ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);

var factory = new ConnectionFactory { HostName = "localhost", UserName = "myuser", Password = "mypassword", Port = 5672};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "seeker-tasks",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);

MessageAnalyzer analyzer = new MessageAnalyzer();

consumer.Received += analyzer.ProcessMessage;
channel.BasicConsume(queue: "seeker-tasks", autoAck: true, consumer: consumer);

// RepositoryFacade repo = new RepositoryFacade("");
// repo.analyze(new Analyzer());

waitHandle.Wait();