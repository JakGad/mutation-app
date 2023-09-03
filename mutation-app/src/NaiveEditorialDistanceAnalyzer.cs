using Antlr4.Runtime.Tree;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using mutation_app.src.Monitoring;
using System.Text.Json;

namespace mutation_app.src;

public class NaiveEditorialDistanceAnalyzer : IAnalyzer
{
    private ILogger _logger = Logger.GetLogger();
    private readonly int _limit;

    public NaiveEditorialDistanceAnalyzer(int limit)
    {
        _limit = limit;
    }
    
    public List<FinalComparisonResult>? Compare(Repository repo, Commit firstCommit, Commit secondCommit, string repoId)
    {

        var changes = repo.Diff.Compare<TreeChanges>(firstCommit.Tree, secondCommit.Tree);
        List<FinalComparisonResult> result = new();

        foreach (var commitChange in changes.Modified)
        {
            var oldEntry = firstCommit.Tree[commitChange.OldPath];
            var newEntry = secondCommit.Tree[commitChange.Path];
            if (oldEntry?.TargetType == TreeEntryTargetType.Blob && newEntry?.TargetType == TreeEntryTargetType.Blob)
            {
                var fileComparison = CompareFiles(oldEntry, newEntry, repoId);
                if (fileComparison is { Score: > 0 })
                {
                    result.Add(fileComparison);
                }
            }
        }

        return result.Count > 0 ? result : null;
    }
    
    private FinalComparisonResult? CompareFiles(TreeEntry firstFile, TreeEntry secondFile, string repoId)
    {
        IParseTree? treeOfFirstFile = TreeFactory.GetTree(firstFile.Target.Peel<Blob>(), firstFile.GetFileExtension(), repoId);
        IParseTree? treeOfSecondFile = TreeFactory.GetTree(secondFile.Target.Peel<Blob>(), secondFile.GetFileExtension(), repoId);

        if (treeOfFirstFile == null || treeOfSecondFile == null) return null;
        var comparisonResult = TreeEditDistance(treeOfFirstFile, treeOfSecondFile);
        if (comparisonResult.Score <= 0 || comparisonResult.OriginalSubtree is null ||
            comparisonResult.NewSubtree is null) return null;
        return comparisonResult.Score < _limit
            ? new FinalComparisonResultWithValues(comparisonResult.Score, firstFile.Path, comparisonResult.OriginalSubtree.GetText(), comparisonResult.NewSubtree.GetText())
            : new FinalComparisonResult(comparisonResult.Score, firstFile.Path);

    }

    private ComparisonResult ProcessNonNull(IParseTree firstNode, IParseTree secondNode)
    {
        var childrenAmount = Math.Max(firstNode.ChildCount, secondNode.ChildCount);
        var childrenComparisonResults = Enumerable.Range(0, childrenAmount)
            .Select((i) => TreeEditDistance(firstNode.GetChild(i), secondNode.GetChild(i))).ToList();

        var varyingResults = childrenComparisonResults.Where(result => result.Score > 0).ToList();
        var t1 = firstNode.GetType();
        var t2 = secondNode.GetType();
        int currentNodeCost = 0;
        if (firstNode.GetType() != secondNode.GetType())
            currentNodeCost = 1;
        else if (firstNode is TerminalNodeImpl term1)
        {
            if (term1.GetText() != secondNode.GetText())
                currentNodeCost = 1;
        }

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
        return result;
    }
}