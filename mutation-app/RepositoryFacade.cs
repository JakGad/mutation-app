using System.Runtime.InteropServices;
using System.Text.Json;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using mutation_app.Monitoring;
using Utils.logger;

namespace mutation_app;

public class RepositoryFacade
{
    private static readonly ILogger _logger = Logger.GetLogger();
    
    private static readonly string workdirPath =
        Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "workdir");

    private Repository _repository;
    private string url;
    
    [MethodStats]
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
    
    [MethodStats]
    private static void CleanWorkdir()
    {
        try
        {
            Array.ForEach(Directory.GetFiles(workdirPath), (fileName) => File.Delete(Path.Combine(workdirPath, fileName)));
            Array.ForEach(Directory.GetDirectories(workdirPath), (dirName) => Directory.Delete(Path.Combine(workdirPath, dirName), true));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "cannot clear workdir");
        }
    }
    
    public RepositoryFacade(string url)
    {
        CreateDirectoryIfDoesntExist();
        CleanWorkdir();
        try
        {
            // string repositoryLocation = Repository.Clone(url, workdirPath);
            _repository = new Repository("/home/jakub/code/testRepo");
            //_logger.Info("repository cloned", "RepositoryFacade.ctor", new { id = url });
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "cannot clone repository");
            throw e;
        }
    }
    
    [MethodStats]
    public void analyze(IAnalyzer analyzer)
    {
        Dictionary<string, string> commitsCache = new Dictionary<string, string>();

        foreach (Branch repositoryBranch in _repository.Branches)
        {
            _logger.LogDebug("analyzing branch", "RepositoryFacade.analyze", new { branchName = repositoryBranch.FriendlyName, id = url });
            Commit childCommit = null;
            foreach (Commit parentCommit in repositoryBranch.Commits)
            {
                _logger.LogDebug(JsonSerializer.Serialize(new { message = "analyzing commits", parentCommit = parentCommit.MessageShort, childCommit = childCommit?.MessageShort, id = url }));
                if (childCommit != null && (!commitsCache.ContainsKey(childCommit.Sha) || commitsCache[childCommit.Sha] != parentCommit.Sha))
                {
                    analyzer.Compare(parentCommit, childCommit, url);
                }

                childCommit = parentCommit;
            }
        }
    }
}