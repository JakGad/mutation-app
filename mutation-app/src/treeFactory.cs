using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using LibGit2Sharp;
using Microsoft.Extensions.Logging;
using mutation_app.src.Monitoring;

namespace mutation_app.src;

public static class TreeFactory
{
    private static readonly List<(string[] extensions, Func<ICharStream, IParseTree> generator)>
        ExtensionsDefinitions = new()
        {
            // (
            //     new[] { "cpp", "c++", "cc", "cp", "cxx" },
            //     stream =>
            //     {
            //         var parser = new CPP14Parser(new CommonTokenStream(new CPP14Lexer(stream)))
            //         {
            //             BuildParseTree = true
            //         };
            //         return parser.translationUnit();
            //     }
            // ),
            (
                new[] { "c" },
                stream =>
                {
                    var lexer = new CLexer(stream);
                    lexer.RemoveErrorListeners();
                    var parser = new CParser(new CommonTokenStream(lexer))
                    {
                        BuildParseTree = true
                    };
                    parser.RemoveErrorListeners();
                    return parser.translationUnit();
                }
            ),
            // (
            //     new[] { "cs" },
            //     stream =>
            //     {
            //         var parser = new CSharpParser(new CommonTokenStream(new CSharpLexer(stream)))
            //         {
            //             BuildParseTree = true
            //         };
            //         return parser.compilation_unit();
            //     }
            // ),
            (
                new[] { "java" },
                stream =>
                {
                    var lexer = new Java20Lexer(stream);
                    lexer.RemoveErrorListeners();
                    var parser = new Java20Parser(new CommonTokenStream(lexer))
                    {
                        BuildParseTree = true
                    };
                    parser.RemoveErrorListeners();
                    return parser.compilationUnit();
                }
            ),
            // (
            //     new[] { "js" },
            //     stream =>
            //     {
            //         var parser = new JavaScriptParser(new CommonTokenStream(new JavaScriptLexer(stream)))
            //         {
            //             BuildParseTree = true
            //         };
            //         return parser.program();
            //     }
            // ),
            // (
            //     new[] { "py" },
            //     stream =>
            //     {
            //         var parser = new PythonParser(new CommonTokenStream(new PythonLexer(stream)))
            //         {
            //             BuildParseTree = true
            //         };
            //         return parser.file_input();
            //     }
            // ),
            // (
            //     new[] { "rb" },
            //     stream =>
            //     {
            //         var parser = new CorundumParser(new CommonTokenStream(new CorundumLexer(stream)))
            //         {
            //             BuildParseTree = true
            //         };
            //         return parser.prog();
            //     }
            // ),
            // (
            //     new[] { "rs" },
            //     stream =>
            //     {
            //         var parser = new RustParser(new CommonTokenStream(new RustLexer(stream)))
            //         {
            //             BuildParseTree = true
            //         };
            //         return parser.crate();
            //     }
            // ),
            // (
            //     new[] { "scala", "kojo", "sc" },
            //     stream =>
            //     {
            //         var parser = new ScalaParser(new CommonTokenStream(new ScalaLexer(stream)))
            //         {
            //             BuildParseTree = true
            //         };
            //         return parser.compilationUnit();
            //     }
            // ),
            // (
            //     new[] { "ts" },
            //     stream =>
            //     {
            //         var parser = new TypeScriptParser(new CommonTokenStream(new TypeScriptLexer(stream)))
            //         {
            //             BuildParseTree = true
            //         };
            //         return parser.program();
            //     }
            // ),
        };

    private static readonly Dictionary<string, Func<ICharStream, IParseTree>> TreeGenerators =
        ExtensionsDefinitions.SelectMany(
            def => def.extensions, (def, extension) => new { Extension = extension, def.generator }
        ).ToDictionary(x => x.Extension, x => x.generator);

    public static IParseTree? GetTree(Blob file, string extension, string id)
    {
        if (TreeGenerators.TryGetValue(extension, out var generator))
            return generator?.Invoke(CharStreams.fromStream(file.GetContentStream()));

        Metrics.GetMetrics().Extensions.Add(1, new KeyValuePair<string, object?>("extension", extension));
        return null;
    }
}