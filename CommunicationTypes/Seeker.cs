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
}