using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Web;
using mutation_seeker_orchestrator.src.Monitoring;

namespace mutation_seeker_orchestrator.src.scrapper.addressGenerators
{
    internal class GithubResponse
    {
        public class GithubItem
        {
            [JsonPropertyName("clone_url")]
            public string? Url { get; set; }
        }

        [JsonPropertyName("items")]
        public GithubItem[]? Items { get; set; }
    }
    public class GithubGenerator: AddressGenerator
    {
        private readonly ILogger _logger = Logger.GetLogger();
        public GithubGenerator(): base("github")
        { }
        
        
        private string? CreateInitialQuery(SupportedLanguages language)
        {
            return $"q=language:{_languageMapper[language]}&sort=stars&order=desc&per_page=100";
        }

        private string GetInitialUrl(SupportedLanguages language)
        {
            return $"{_baseEndpoint}?{CreateInitialQuery(language)}";
        }

        private readonly Dictionary<SupportedLanguages, string> _languageMapper = new()
        {
            { SupportedLanguages.Cpp, HttpUtility.UrlEncode("C++") }
        };

        private readonly string _baseEndpoint = "https://api.github.com/search/repositories";
        public override async IAsyncEnumerable<string> GetAddresses(SupportedLanguages language)
        {
            await _awaitWorkMode.Task;
            string? currentUrl = GetInitialUrl(language);
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "mutation-orchestrator-app");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");


            while (currentUrl != null)
            {
                var response = await client.GetAsync(currentUrl);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<GithubResponse>();

                    if (result?.Items == null) yield break;

                    foreach (var repo in result.Items)
                    {
                        if(repo.Url != null && !WasUrlAnalyzed(repo.Url)) yield return repo.Url;
                    }

                    currentUrl = GetNextUrlFromHeader(response.Headers);
                }
                else
                {
                    _logger.LogCritical("Error while getting data from github", new object[] { response });
                    yield break;
                }
            }
        }

        private string? GetNextUrlFromHeader(HttpResponseHeaders headers)
        {
            var links = headers.GetValues("Link").ElementAtOrDefault(0);

            if (links == null) return null;

            var match = Regex.Match(links, "<(?<url>.+?)>; rel=\"next\"");

            if (match.Success)
            {
                return match.Groups["url"].Value;
            }

            return null;
        }
    }
}
