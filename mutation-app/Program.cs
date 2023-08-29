using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.Logging;
using mutation_app.src;
using mutation_app.src.Monitoring;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var containerId = Dns.GetHostName();
var appDetails = new
{
    version = Assembly.GetExecutingAssembly().GetName().Version,
    containerId,
    ip = Dns.GetHostEntry(containerId).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)
};

ManualResetEventSlim waitHandle = new ManualResetEventSlim(false);

var EnvReader = SeekerEnvs.GetEnvs();
var factory = new ConnectionFactory { HostName = EnvReader.QueueAddress, UserName = EnvReader.QueueLogin, Password = EnvReader.QueuePassword, Port = EnvReader.QueuePort, SocketReadTimeout = Timeout.InfiniteTimeSpan, SocketWriteTimeout = Timeout.InfiniteTimeSpan };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "seeker-tasks",
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null);
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

var consumer = new EventingBasicConsumer(channel);

MessageAnalyzer analyzer = new MessageAnalyzer(channel);

consumer.Received += analyzer.ProcessMessage;
channel.BasicConsume(queue: "seeker-tasks", autoAck: false, consumer: consumer);

Logger.GetLogger().LogInformation("App {appDetails} started", appDetails);

waitHandle.Wait();