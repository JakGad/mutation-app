using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using mutation_app.src.Monitoring;
using static System.Text.Json.JsonSerializer;

namespace mutation_app.src;

public class RepositoryFacade
{
    private static void ClearReadOnly(DirectoryInfo? parentDirectory)
    {
        if (parentDirectory != null)
        {
            parentDirectory.Attributes = FileAttributes.Normal;
            foreach (FileInfo fi in parentDirectory.GetFiles())
            {
                fi.Attributes = FileAttributes.Normal;
            }
            foreach (DirectoryInfo di in parentDirectory.GetDirectories())
            {
                ClearReadOnly(di);
            }
        }
    }

    private static readonly ILogger _logger = Logger.GetLogger();

    private static readonly string workdirPath =
        Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "workdir");

    private Repository _repository;
    private string url;
    
    private static void CreateDirectoryIfDoesntExist()
    {
        try
        {
            if (!Directory.Exists(workdirPath))
                Directory.CreateDirectory(workdirPath);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "cannot create workdir");
        }
    }
    
    private static void CleanWorkdir()
    {
        try
        {
            ClearReadOnly(new DirectoryInfo(workdirPath));
            Array.ForEach(Directory.GetFiles(workdirPath), (fileName) => File.Delete(Path.Combine(workdirPath, fileName)));
            Array.ForEach(Directory.GetDirectories(workdirPath), (dirName) => Directory.Delete(Path.Combine(workdirPath, dirName), true));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "cannot clear workdir");
            throw e;
        }
    }
    
    public RepositoryFacade(string url)
    {
        CreateDirectoryIfDoesntExist();
        CleanWorkdir();
        try
        {
            string repositoryLocation = Repository.Clone(url, workdirPath);
            _repository = new Repository(repositoryLocation);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "cannot clone repository");
            throw e;
        }
    }
    
    public RepoComparisonResult Analyze(IAnalyzer analyzer)
    {
        Dictionary<string, string> commitsCache = new Dictionary<string, string>();
        List<CommitComparisonResult> commitResults = new();

        foreach (Branch repositoryBranch in _repository.Branches)
        {
            _logger.LogDebug("analyzing branch", "RepositoryFacade.analyze", new { branchName = repositoryBranch.FriendlyName, id = url });
            Commit? childCommit = null;
            foreach (var parentCommit in repositoryBranch.Commits)
            {
                _logger.LogDebug(Serialize(new
                {
                    message = "analyzing commits",
                    parentCommit = parentCommit.MessageShort,
                    childCommit = childCommit?.MessageShort,
                    id = url
                }));
                if (childCommit != null && (!commitsCache.ContainsKey(childCommit.Sha) || commitsCache[childCommit.Sha] != parentCommit.Sha))
                {
                    var result = analyzer.Compare(parentCommit, childCommit, url);
                    commitResults.Add(new CommitComparisonResult(childCommit.Sha, parentCommit.Sha, result));
                }

                childCommit = parentCommit;
            }
        }

        return new RepoComparisonResult(url, commitResults);
    }
}