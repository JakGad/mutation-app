using System.Collections.Immutable;
using mutation_seeker_orchestrator.src.scrapper.status;

namespace mutation_seeker_orchestrator.src.scrapper.addressGenerators
{
    public enum SupportedLanguages
    {
        Csharp, C, Java, Cpp,  Js, Python, Ruby, Rust, Scala, Ts
    }
    public abstract class AddressGenerator
    {
        private Phases _currentPhase = Phases.Initialization;
        protected Phases CurrentPhase
        {
            get => _currentPhase;
            set
            {
                _currentPhase = value;
                NotifyStatusChanged?.Invoke(value);
                if(value == Phases.Work)
                    _awaitWorkMode.SetResult(true);
            }
        }

        public abstract IAsyncEnumerable<string> GetAddresses(SupportedLanguages language);
        public event Action<Phases> NotifyStatusChanged;

        protected void UrlAnalyzeFinished(string url)
        {
            if (!_urlsBeingAnalyzed.Contains(url)) return;
            _finishedUrls.Add(url);
            _urlsBeingAnalyzed.Remove(url);
        }
        public string GeneratorName { get; }

        protected HashSet<string> _finishedUrls = new();
        protected HashSet<string> _urlsBeingAnalyzed = new();

        protected bool WasUrlAnalyzed(string url)
        {
            return _finishedUrls.Contains(url) || _urlsBeingAnalyzed.Contains(url);
        }

        public async void LoadAnalyzedUrls()
        {
            _finishedUrls = await DbFacade.GetReposUrl();
            CurrentPhase = Phases.Work;
        }

        protected AddressGenerator(string generatorName)
        {
            ScrapperStatus.RegisterAddressGenerator(this);
            this.GeneratorName = generatorName;

            CurrentPhase = Phases.Indexing;
            LoadAnalyzedUrls();
            ScrapperStatus.UrlAnalyzeFinishedEvent += UrlAnalyzeFinished;
        }

        protected TaskCompletionSource<bool> _awaitWorkMode = new();
    }
}
