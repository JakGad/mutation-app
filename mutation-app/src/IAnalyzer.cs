using Antlr4.Runtime.Tree;
using CommunicationTypes;
using LibGit2Sharp;

namespace mutation_app.src;


public struct RepoComparisonResult
{
    public string Url;
    public List<CommitComparisonResult> Results;

    public RepoComparisonResult(string url, List<CommitComparisonResult> results)
    {
        Url = url;
        Results = results;
    }
}

public struct CommitComparisonResult
{
    public string CommitHash;
    public string ParentCommitHash;
    public List<FinalComparisonResult> FileResults;
    public float MetricDifferenceBeforeLimiting;

    public CommitComparisonResult(string commitHash, string parentCommitHash, List<FinalComparisonResult> fileResults)
    {
        CommitHash = commitHash;
        ParentCommitHash = parentCommitHash;
        FileResults = fileResults;
        MetricDifferenceBeforeLimiting = FileResults.Aggregate((long)0, (result, comparisonResult) => result + comparisonResult.Score);
    }
    
    public CommitComparisonResult(CommitComparisonResult toCopy)
    {
        CommitHash = toCopy.CommitHash;
        ParentCommitHash = toCopy.ParentCommitHash;
        FileResults = toCopy.FileResults;
        MetricDifferenceBeforeLimiting = toCopy.MetricDifferenceBeforeLimiting;
    }
}

public struct ComparisonResult
{
    public int Score;
    public IParseTree? OriginalSubtree;
    public IParseTree? NewSubtree;
    public string? FilePath;

    public ComparisonResult(int score, IParseTree? originalSubtree, IParseTree? newSubtree, string? filePath = null)
    {
        Score = score;
        OriginalSubtree = originalSubtree;
        NewSubtree = newSubtree;
        FilePath = filePath;
    }

    public ComparisonResult(ComparisonResult toCopy, string? filePath) : this(toCopy.Score, toCopy.OriginalSubtree, toCopy.NewSubtree, filePath ?? toCopy.FilePath)
    { }

    public override string ToString()
    {
        return
            $"Score: {Score}\nFilePath: {FilePath}\noriginalTree: {OriginalSubtree?.GetText()}\nnewTree: {NewSubtree?.GetText()}";
    }
}

public class FinalComparisonResult
{
    public int Score;
    public string FilePath;
    public FinalComparisonResult(int score, string filePath)
    {
        Score = score;
        FilePath = filePath;
    }

    public virtual FileComparisonResultDTO MapToDto()
    {
        return new FileComparisonResultDTO()
        {
            Score = this.Score,
            Path = this.FilePath,
        };
    }
    
}

public class FinalComparisonResultWithValues: FinalComparisonResult
{
    public readonly string OriginalSubtree;
    public readonly string NewSubtree;
    public FinalComparisonResultWithValues(int score, string filePath, string originalSubtree, string newSubtree): base(score, filePath)
    {
        OriginalSubtree = originalSubtree;
        NewSubtree = newSubtree;
    }

    public override FileComparisonResultDTO MapToDto()
    {
        var partialMap = base.MapToDto();
        partialMap.OriginalTreeFragment = OriginalSubtree;
        partialMap.ParentTreeFragment = NewSubtree;
        return partialMap;
    }
}

public interface IAnalyzer
{
    public List<FinalComparisonResult>? Compare(Repository repo, Commit commit1, Commit commit2, string repoId);
}