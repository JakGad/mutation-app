using LibGit2Sharp;

namespace mutation_app;

public interface IAnalyzer
{
    public void Compare(Commit commit1, Commit commit2, string repoId);
}