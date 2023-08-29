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
    private static readonly Metrics _metrics = Metrics.GetMetrics();

    private static readonly string workdirPath =
        Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "workdir");

    private Repository _repository;
    private string _url;
    
    private static void CreateDirectoryIfDoesntExist()
    {
        try
        {
            if (!Directory.Exists(workdirPath))
                Directory.CreateDirectory(workdirPath);
        }
        catch (Exception error)
        {
            _logger.LogCritical(error, "cannot create workdir {@error}", error);
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
        catch (Exception error)
        {
            _logger.LogCritical(error, "cannot clear workdir {@error}", error);
            throw error;
        }
    }
    
    public RepositoryFacade(string url)
    {
        var entryTimeStamp = DateTime.Now;
        this._url = url;
        CreateDirectoryIfDoesntExist();
        CleanWorkdir();
        try
        {
            string repositoryLocation = Repository.Clone(url, workdirPath);
            _repository = new Repository(repositoryLocation);
        }
        catch (Exception error)
        {
            _logger.LogCritical(error, "cannot clone repository {@url} {@error}",url, error);
            throw error;
        }
        _metrics.SuccessfulMethodExecutionTime.Record((DateTime.Now - entryTimeStamp).Milliseconds,
            new KeyValuePair<string, object?>("method", "RepoCtor"));
    }
    
    public RepoComparisonResult Analyze(IAnalyzer analyzer)
    {
        _logger.LogInformation("Analyze of {@url} {action}", _url, "started");
        var entryTimeStamp = DateTime.Now;
        Dictionary<string, string> commitsCache = new Dictionary<string, string>();
        List<CommitComparisonResult> commitResults = new();

        foreach (Branch repositoryBranch in _repository.Branches)
        {
            Commit? childCommit = null;
            foreach (var parentCommit in repositoryBranch.Commits)
            {
                if (childCommit != null && (!commitsCache.ContainsKey(childCommit.Sha) || commitsCache[childCommit.Sha] != parentCommit.Sha))
                {
                    var result = analyzer.Compare(parentCommit, childCommit, _url);
                    commitResults.Add(new CommitComparisonResult(childCommit.Sha, parentCommit.Sha, result));
                }

                childCommit = parentCommit;
            }
        }
        _logger.LogInformation("Analyze of {@url} {action}", _url, "finished");
        _metrics.SuccessfulMethodExecutionTime.Record((DateTime.Now - entryTimeStamp).Milliseconds,
            new KeyValuePair<string, object?>("method", "RepoAnalyze"));

        return new RepoComparisonResult(_url, commitResults);
    }
}