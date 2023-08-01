using mutation_app.src;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

const string serviceName = "mutation-seeker";
const string serviceVersion = "1.0.0";

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

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);

MessageAnalyzer analyzer = new MessageAnalyzer(channel);

consumer.Received += analyzer.ProcessMessage;
channel.BasicConsume(queue: "seeker-tasks", autoAck: false, consumer: consumer);

waitHandle.Wait();