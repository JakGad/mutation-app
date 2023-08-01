using Antlr4.Runtime.Tree;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using mutation_app.src.Monitoring;
using System.Text.Json;

namespace mutation_app.src;

public class NaiveEditorialDistanceAnalyzer : IAnalyzer
{
    private ILogger _logger = Logger.GetLogger();
    
    public List<ComparisonResult> Compare(Commit firstCommit, Commit secondCommit, string repoId)
    {
        _logger.LogDebug(JsonSerializer.Serialize(new { message = "analyzing", firstCommit = new { sha = firstCommit.Sha, filesAmount = firstCommit.Tree.Count }, secondCommit = new { sha = secondCommit.Sha, filesAmount = secondCommit.Tree.Count }, id = repoId }));

        Dictionary<string, TreeEntry> firstCommitFiles = new Dictionary<string, TreeEntry>(
            firstCommit.Tree.Where(file => file.TargetType == TreeEntryTargetType.Blob).Select((file) => new KeyValuePair<string, TreeEntry>(file.Path, file))
            );

        List<ComparisonResult> result = new();

        foreach (var newFile in secondCommit.Tree.Where(file => file.TargetType == TreeEntryTargetType.Blob))
        {
            if (firstCommitFiles.TryGetValue(newFile.Path, out TreeEntry? oldFile))
            {
                //_logger.LogDebug(JsonSerializer.Serialize(new { message = "file found", file1 = oldFile.Target.Peel<Blob>().GetContentText(), id = repoId, file2= newFile.Target.Peel<Blob>().GetContentText() }));

                var fileComparison = CompareFiles(oldFile, newFile, repoId);
                if (fileComparison is { Score: > 0 })
                    result.Add(fileComparison.Value);
            }
            else
            {
                _logger.LogDebug(JsonSerializer.Serialize(new { message = "no corresponding file", id = repoId, commitSha = secondCommit.Sha, path = newFile.Path }));
            }
        }

        return result;
    }
    
    private ComparisonResult? CompareFiles(TreeEntry firstFile, TreeEntry secondFile, string repoId)
    {
        IParseTree? treeOfFirstFile = TreeFactory.GetTree(firstFile.Target.Peel<Blob>(), firstFile.GetFileExtension(), repoId);
        IParseTree? treeOfSecondFile = TreeFactory.GetTree(secondFile.Target.Peel<Blob>(), secondFile.GetFileExtension(), repoId);


        var comparisonResult = TreeEditDistance(treeOfFirstFile, treeOfSecondFile);
        if (comparisonResult.Score > 0)
        {
            return new ComparisonResult(comparisonResult, firstFile.Path);
        }

        return null;
    }

    private ComparisonResult ProcessNonNull(IParseTree firstNode, IParseTree secondNode)
    {
        var childrenAmount = Math.Max(firstNode.ChildCount, secondNode.ChildCount);
        var childrenComparisonResults = Enumerable.Range(0, childrenAmount)
            .Select((i) => TreeEditDistance(firstNode.GetChild(i), secondNode.GetChild(i))).ToList();

        var varyingResults = childrenComparisonResults.Where(result => result.Score > 0).ToList();
        var currentNodeCost = firstNode.GetType().Equals(secondNode.GetType()) ? 0 : 1;

        _logger.LogDebug($"ProcessNonNull: {currentNodeCost}, {firstNode.GetType()}, {secondNode.GetType()}");

        return varyingResults.Count switch
        {
            > 1 => new ComparisonResult(varyingResults.Sum((result) => result.Score) + currentNodeCost,
                firstNode, secondNode),
            1 => currentNodeCost == 0
                ? varyingResults[0]
                : new ComparisonResult(varyingResults[0].Score + currentNodeCost, firstNode, secondNode),
            _ => currentNodeCost == 0
                ? new ComparisonResult(0, null, null)
                : new ComparisonResult(currentNodeCost, firstNode, secondNode)
        };
    }
    
    private ComparisonResult TreeEditDistance(IParseTree? firstNode, IParseTree? secondNode)
    {
        var result = (firstNode, secondNode) switch
        {
            (null, null) => new ComparisonResult(0, null, null),
            (null, not null) => new ComparisonResult(secondNode.CountAllSubChildren(), null, secondNode),
            (not null, null) => new ComparisonResult(firstNode.CountAllSubChildren(), firstNode, null),
            (not null, not null) => ProcessNonNull(firstNode, secondNode),
        };
        _logger.LogDebug(JsonSerializer.Serialize(new { message = "TED", node1 = firstNode?.GetText(), node2 = secondNode?.GetText(), result = result.ToString() }));

        return result;
    }
}