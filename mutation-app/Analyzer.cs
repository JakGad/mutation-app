using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LibGit2Sharp;
using Utils.logger;

namespace mutation_app;

public class Analyzer: IAnalyzer
{
    private ILogger _logger = Logger.GetLogger();
    public void Compare(Commit firstCommit, Commit secondCommit, string repoId)
    {
        _logger.Debug("analyzing", "Analyzer.Compare", new { firstCommit = new { sha = firstCommit.Sha, filesAmount = firstCommit.Tree.Count }, secondCommit = new { sha = secondCommit.Sha, filesAmount = secondCommit.Tree.Count }, id = repoId });

        Dictionary<string, TreeEntry> firstCommitFiles = new Dictionary<string, TreeEntry>(
                firstCommit.Tree.Select((file) => new KeyValuePair<string, TreeEntry>(file.Path, file))
            );

        foreach (var fileToCompare in secondCommit.Tree)
        {
            if (firstCommitFiles.ContainsKey(fileToCompare.Path))
            {
                _logger.Debug("file", "Analyzer.Compare", new { file = fileToCompare.Target.Peel<Blob>().GetContentText(), id=repoId, path = fileToCompare.Path });
                
                
                
            }
            else
            {
                _logger.Debug("corresponding file not found", "Analyzer.Compare", new { id = repoId, commitSha = secondCommit.Sha, path = fileToCompare.Path });
            }
        }
    }
}