using System.Text.Json.Serialization;

namespace CommunicationTypes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "TaskType")]
[JsonDerivedType(typeof(EndTask), typeDiscriminator: "end")]
[JsonDerivedType(typeof(AnalyzeTask), typeDiscriminator: "analyze")]
public abstract class SeekerTask
{

}

public class EndTask : SeekerTask
{

}

public class AnalyzeTask : SeekerTask
{
    public string Url { get; set; }
    public int MaxMetricDifference { get; set; }
}

[Serializable]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "Status")]
[JsonDerivedType(typeof(RepoCompartisonResultDTO), typeDiscriminator: "complete")]
[JsonDerivedType(typeof(RepoComparisonStartedDTO), typeDiscriminator: "start")]
public class StatusReportDTO
{
    public string? RepoUrl { get; set; }
}

[Serializable]
public class RepoComparisonStartedDTO : StatusReportDTO
{
    public string Status { get; set; } = "start";
}

[Serializable]
public class RepoCompartisonResultDTO: StatusReportDTO
{
    public string Status { get; set; } = "complete";
    public List<CommitComparisonResultDTO>? CommitResults { get; set; }
}

[Serializable]
public class CommitComparisonResultDTO
{
    public string CommitHash { get; set; }
    public string CommitParentHash { get; set; }
    public float ScoreOverall { get; set; }

    public List<FileComparisonResultDTO> FileResults { get; set; }
}

[Serializable]
public class FileComparisonResultDTO
{
    public string Path { get; set; }
    public int Score { get; set; }
    public string? ParentTreeFragment { get; set; }
    public string? OriginalTreeFragment { get; set; }
}