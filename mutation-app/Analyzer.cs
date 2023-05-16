using System.Text.Json;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using mutation_app.Monitoring;
using Utils.logger;

namespace mutation_app;

public class Analyzer: IAnalyzer
{
    private ILogger _logger = Logger.GetLogger();
    
    [MethodStats]
    public void Compare(Commit firstCommit, Commit secondCommit, string repoId)
    {
        _logger.LogDebug(JsonSerializer.Serialize(new { message = "analyzing", firstCommit = new { sha = firstCommit.Sha, filesAmount = firstCommit.Tree.Count }, secondCommit = new { sha = secondCommit.Sha, filesAmount = secondCommit.Tree.Count }, id = repoId }));

        Dictionary<string, TreeEntry> firstCommitFiles = new Dictionary<string, TreeEntry>(
            firstCommit.Tree.Select((file) => new KeyValuePair<string, TreeEntry>(file.Path, file))
            );

        foreach (var fileToCompare in secondCommit.Tree)
        {
            if (firstCommitFiles.ContainsKey(fileToCompare.Path))
            {
                _logger.LogDebug(JsonSerializer.Serialize(new { message = "file found", file = fileToCompare.Target.Peel<Blob>().GetContentText(), id = repoId, path = fileToCompare.Path }));
            }
            else
            {
                _logger.LogDebug(JsonSerializer.Serialize(new { message = "no corresponding file", id = repoId, commitSha = secondCommit.Sha, path = fileToCompare.Path }));
            }
        }
    }
}