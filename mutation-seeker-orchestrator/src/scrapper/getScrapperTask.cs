using System.Text.Json;
using CommunicationTypes;
using mutation_seeker_orchestrator.src.environment;
using mutation_seeker_orchestrator.src.Monitoring;
using mutation_seeker_orchestrator.src.scrapper.addressGenerators;
using mutation_seeker_orchestrator.src.scrapper.status;
using RabbitMQ.Client;

namespace mutation_seeker_orchestrator.src.scrapper
{
    
    public static class Scrapper
    {
        private const int MaxMessages = 32;
        private static ILogger _logger = Logger.GetLogger();
        private static EnvironmentVariables _environment = EnvironmentVariables.GetEnvs();
        static async void RunScrapping()
        {
            
            var envVariables = EnvironmentVariables.GetEnvs();
            var factory = new ConnectionFactory
                { HostName = envVariables.QueueAddress, UserName = envVariables.QueueLogin, Password = envVariables.QueuePassword, Port = envVariables.QueuePort };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "seeker-tasks",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            channel.QueuePurge(channel.QueueDeclarePassive(channel.CurrentQueue));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            var blocker = new WaitForQueue(channel, MaxMessages);

            ScrapperStatus.UrlAnalyzeStartedEvent += _ => blocker.CheckAndUnlock();
            
            var addresses = AddressFactory.GetAddressGenerator(SupportedGitPages.Github); 
            var addressIterator = addresses.GetAddresses(SupportedLanguages.Cpp).GetAsyncEnumerator();

            while(await addressIterator.MoveNextAsync())
            {
                await blocker.Wait();
                var message = new AnalyzeTask() { Url = addressIterator.Current, MaxMetricDifference = _environment.DefaultMaxMetric};
                var body = JsonSerializer.SerializeToUtf8Bytes<SeekerTask>(message);
                channel.BasicPublish(exchange: string.Empty,
                    routingKey: "seeker-tasks",
                    basicProperties: properties,
                    body: body);
                blocker.CheckAndLock();
                _logger.LogInformation($"Analysing: {addressIterator.Current}");
            }
        }

        public static Thread GetScrapperTask() => new(RunScrapping);
    }
}
