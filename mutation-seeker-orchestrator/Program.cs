using CommunicationTypes;
using System.Text.Json;
using mutation_seeker_orchestrator.src;
using mutation_seeker_orchestrator.src.Monitoring;
using mutation_seeker_orchestrator.src.scrapper;
using mutation_seeker_orchestrator.src.scrapper.status;


var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var logger = Logger.GetLogger();

app.MapPut("/register-progress", async (HttpRequest req) =>
{
    try
    {
        var requestBodyString = new StreamReader(req.Body).ReadToEndAsync();
        var progressStatus = JsonSerializer.Deserialize<StatusReportDTO>(await requestBodyString);
        return progressStatus switch
        {
            RepoCompartisonResultDTO result => SaveResult(result),
            RepoComparisonStartedDTO info => ComparisonStarted(info),
            _ => UnknownEntity(),
        };
    }
    catch
    {
        return Results.UnprocessableEntity();
    }
});


IResult SaveResult(RepoCompartisonResultDTO result)
{
    ScrapperStatus.RegisterTaskCompletion(result.RepoUrl);
    DbFacade.SaveResult(result);
    return Results.Ok();
}

IResult ComparisonStarted(RepoComparisonStartedDTO info)
{
    ScrapperStatus.RegisterTaskStarted(info.RepoUrl);
    return Results.Ok();
}

IResult UnknownEntity()
{
    logger.LogError("Unprocessable entity");
    return Results.UnprocessableEntity();
}


var scrappingThread = Scrapper.GetScrapperTask();

scrappingThread.Start();
app.Run();
