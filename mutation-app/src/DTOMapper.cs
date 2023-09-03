using CommunicationTypes;

namespace mutation_app.src;

public interface IDTOMapper
{
    public RepoCompartisonResultDTO MapToDTO(RepoComparisonResult result);
    public RepoComparisonStartedDTO MapToStartEvent(string url);
}

public class DTOMapper : IDTOMapper
{
    public RepoCompartisonResultDTO MapToDTO(RepoComparisonResult objectToMapFrom)
    {
        return new RepoCompartisonResultDTO()
        {
            RepoUrl = objectToMapFrom.Url,
            CommitResults = objectToMapFrom.Results.Select(MapCommitResult).ToList(),
        };
    }

    public RepoComparisonStartedDTO MapToStartEvent(string url)
    {
        return new RepoComparisonStartedDTO()
        {
            RepoUrl = url
        };
    }

    private CommitComparisonResultDTO MapCommitResult(CommitComparisonResult objectToMapFrom)
    {
        return new CommitComparisonResultDTO()
        {
            CommitHash = objectToMapFrom.CommitHash,
            CommitParentHash = objectToMapFrom.ParentCommitHash,
            FileResults = objectToMapFrom.FileResults.Select(res => res.MapToDto()).ToList(),
            ScoreOverall = objectToMapFrom.MetricDifferenceBeforeLimiting
        };
    }
}