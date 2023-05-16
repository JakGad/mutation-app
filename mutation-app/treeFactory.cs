using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LibGit2Sharp;

namespace mutation_app.grammars;

// ICharStream stream = CharStreams.fromStream(file.GetContentStream());
// ITokenSource lexer = new CPP14Lexer(stream);
// ITokenStream tokens = new CommonTokenStream(lexer);
// CPP14Parser parser = new CPP14Parser(tokens);
// parser.BuildParseTree = true;
// return parser.translationUnit();
public static class TreeFactory
{
    private static Dictionary<string, Func<ICharStream, IParseTree>> _treeGenerators = new ()
    {
        { "cpp", (stream) =>
        {
            CPP14Parser parser = new CPP14Parser(new CommonTokenStream(new CPP14Lexer(stream)));
            parser.BuildParseTree = true;
            return parser.translationUnit();
        } }
    };


    public static IParseTree? GetTree(Blob file, string extension, string id)
    {
        return _treeGenerators[extension]?.Invoke(CharStreams.fromStream(file.GetContentStream()));
    }
}