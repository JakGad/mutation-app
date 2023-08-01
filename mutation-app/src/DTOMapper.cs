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
            FileResults = objectToMapFrom.FileResults.Select(MapFileResult).ToList(),
            ScoreOverall = objectToMapFrom.FileResults.Sum((x) => x.Score),
        };
    }

    private FileComparisonResultDTO MapFileResult(ComparisonResult objectToMapFrom)
    {
        return new FileComparisonResultDTO()
        {
            Path = objectToMapFrom.FilePath,
            Score = objectToMapFrom.Score,
            OriginalTreeFragment = objectToMapFrom.NewSubtree.GetText(),
            ParentTreeFragment = objectToMapFrom.OriginalSubtree.GetText(),
        };
    }
}