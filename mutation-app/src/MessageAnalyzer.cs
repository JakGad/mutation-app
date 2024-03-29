using CommunicationTypes;
using Microsoft.Extensions.Logging;
using mutation_app.src.Monitoring;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace mutation_app.src;

public class MessageAnalyzer
{
    private readonly ILogger _logger = Logger.GetLogger();
    private readonly SeekerEnvs _envVariables = SeekerEnvs.GetEnvs();
    private readonly IModel _channel;
    private readonly HttpClient _httpClient = new();
    IDTOMapper _dtoMapper = new DTOMapper();

    private string _responseAddress;

    public MessageAnalyzer(IModel channel)
    {
        _responseAddress = new UriBuilder
        {
            Scheme = "http",
            Host = _envVariables.SupervisorAddress,
            Path = "/register-progress",
            Port = _envVariables.SupervisorPort
        }.Uri.ToString();
        _channel = channel;
    }
    
    public void ProcessMessage(object? model, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<SeekerTask>(body);
            
            _logger.LogInformation("Received {@message}", message);
            switch (message)
            {
                case AnalyzeTask task:
                    Analyze(task);
                    break;
                case EndTask _:
                    Kill();
                    break;
                default:
                    Unknown(body);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing {@message}, {@error}", ea.Body.ToString(), ex);
        }
        finally
        {
            _channel.BasicAck(ea.DeliveryTag, false);
        }
    }

    private async void Analyze(AnalyzeTask task)
    {
        try
        {
            ReportStart(task.Url);
            RepositoryFacade repo = new RepositoryFacade(task.Url);
            var result = repo.Analyze(new NaiveEditorialDistanceAnalyzer(task.MaxMetricDifference));
            // var cleanedResult = LimitValuesInResult(result, task.MaxMetricDifference);
            var json = JsonSerializer.Serialize(_dtoMapper.MapToDTO(result));
            var dataToSend = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(_responseAddress, dataToSend);
            GC.Collect();
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Unable to send response for {@url}. Status code: {@code}", task.Url, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error analyzing {@url} {@error}", task.Url, ex);
        }
    }

    private RepoComparisonResult LimitValuesInResult(RepoComparisonResult dataToClean,
        int maxFileMetricDifference)
    {
        var cleanedResults = dataToClean.Results.Select(commitResult =>
        {
            var newCommitComparison = new CommitComparisonResult(commitResult);
            newCommitComparison.FileResults = commitResult.FileResults
                .Where(fileResult => fileResult.Score <= maxFileMetricDifference).ToList();

            return newCommitComparison;
        }).ToList();

        return new RepoComparisonResult(dataToClean.Url, cleanedResults);
    }

    private async Task<bool> ReportStart(string url)
    {
        var json = JsonSerializer.Serialize(_dtoMapper.MapToStartEvent(url));
        var dataToSend = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PutAsync(_responseAddress, dataToSend);

        return response.IsSuccessStatusCode;
    }
    
    private void Kill()
    {
        Environment.Exit(0);
    }

    private void Unknown(byte[] task)
    {
        _logger.LogError("Task unknown {@task}", task.ToString());
    }
}