using Antlr4.Runtime.Tree;
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
    public List<ComparisonResult> FileResults;

    public CommitComparisonResult(string commitHash, string parentCommitHash, List<ComparisonResult> fileResults)
    {
        CommitHash = commitHash;
        ParentCommitHash = parentCommitHash;
        FileResults = fileResults;
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

public interface IAnalyzer
{
    public List<ComparisonResult> Compare(Commit commit1, Commit commit2, string repoId);
}