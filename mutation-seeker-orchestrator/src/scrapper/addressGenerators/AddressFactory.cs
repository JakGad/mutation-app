using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using mutation_seeker_orchestrator.src.scrapper.status;

namespace mutation_seeker_orchestrator.src.scrapper.addressGenerators
{
    public enum SupportedGitPages
    {
        Github
    }
    public static class AddressFactory
    {
        public static AddressGenerator GetAddressGenerator(SupportedGitPages page)
        {
            return page switch
            {
                SupportedGitPages.Github => new GithubGenerator()
            };
        }
    }
}
