using CommunicationTypes;
using MongoDB.Bson.Serialization.Attributes;

namespace mutation_seeker_orchestrator.src.dataStructures
{


    [Serializable]
    public class RepoResult
    {
        [BsonId]
        public string RepoUrl { get; set; }
        public List<CommitResult>? CommitResults { get; set; }

        public RepoResult(RepoCompartisonResultDTO dto)
        {
            RepoUrl = dto.RepoUrl;
            CommitResults = dto.CommitResults.ConvertAll((el) => new CommitResult(el));
        }
    }

    [Serializable]
    public class CommitResult
    {
        public string CommitHash { get; set; }
        public string CommitParentHash { get; set; }
        public float ScoreOverall { get; set; }

        public List<FileResult> FileResults { get; set; }

        public CommitResult(CommitComparisonResultDTO dto)
        {
            CommitHash = dto.CommitHash;
            CommitParentHash = dto.CommitParentHash;
            ScoreOverall = dto.ScoreOverall;
            FileResults = dto.FileResults.ConvertAll((el) => new FileResult(el));
        }
    }

    [Serializable]
    public class FileResult
    {
        public string Path { get; set; }
        public int Score { get; set; }
        public string ParentTreeFragment { get; set; }
        public string OriginalTreeFragment { get; set; }

        public FileResult(FileComparisonResultDTO dto)
        {
            Path = dto.Path;
            Score = dto.Score;
            ParentTreeFragment = dto.ParentTreeFragment;
            OriginalTreeFragment = dto.OriginalTreeFragment;
        }
    }
}
