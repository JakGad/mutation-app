using Antlr4.Runtime.Tree;
using LibGit2Sharp;

namespace mutation_app.src
{
    public static class GitExtensions
    {
        public static string GetFileExtension(this TreeEntry file)
        {
            var fileName = file.Name;
            return fileName.Substring(fileName.LastIndexOf('.') + 1);
        }

        public static int CountAllSubChildren(this IParseTree node)
        {
            return node.GetChildrenIterator().Select((tree) => tree.CountAllSubChildren()).Sum() + 1;
        }

        public static IEnumerable<IParseTree> GetChildrenIterator(this IParseTree node)
        {
            for (var i = 0; i < node.ChildCount; ++i)
            {
                yield return node.GetChild(i);
            }
        }
    }
}
