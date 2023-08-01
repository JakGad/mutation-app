using System.Collections.ObjectModel;
using mutation_seeker_orchestrator.src.scrapper.addressGenerators;

namespace mutation_seeker_orchestrator.src.scrapper.status
{
    public enum Phases
    {
        Initialization,
        Indexing,
        Work,
        ShuttingDown,
        Off
    }

    public static class ScrapperStatus
    {
        public static Dictionary<string, Phases> _generatorsPhases = new();
        public static ReadOnlyDictionary<string, Phases> GeneratorsStatus => _generatorsPhases.AsReadOnly();
        

        public static void RegisterAddressGenerator(AddressGenerator generator)
        {
            generator.NotifyStatusChanged += (phase => _generatorsPhases[generator.GeneratorName] = phase);
        }

        public static event Action<string> UrlAnalyzeFinishedEvent;

        public static void RegisterTaskCompletion(string? url)
        {
            if(url != null) UrlAnalyzeFinishedEvent.Invoke(url);
        }
        
        public static event Action<string> UrlAnalyzeStartedEvent;

        public static void RegisterTaskStarted(string? url)
        {
            if(url != null) UrlAnalyzeStartedEvent.Invoke(url);
        }
    }
}
