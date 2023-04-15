using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LibGit2Sharp;

namespace mutation_app.grammars;


public class TreeFactory
{
    public IParseTree getTree(Blob file, string esxtension, string id)
    {
        // ICharStream stream = CharStreams.fromStream(file.GetContentStream());
        // ITokenSource lexer = new CPP14Lexer(stream);
        // ITokenStream tokens = new CommonTokenStream(lexer);
        // CPP14Parser parser = new CPP14Parser(tokens);
        // parser.BuildParseTree = true;
        // return parser.translationUnit();
    }
}