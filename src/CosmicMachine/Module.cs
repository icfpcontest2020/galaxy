using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CosmicMachine
{
    public class Module
    {
        public readonly bool HeadersOnly;
        public readonly List<(string, MethodDeclarationSyntax m)> Methods = new List<(string, MethodDeclarationSyntax)>();
        public readonly List<(string, VariableDeclaratorSyntax f)> Fields = new List<(string, VariableDeclaratorSyntax)>();
        public readonly SyntaxTree Tree;

        public Module(string sourceCode, string filename, bool headersOnly)
        {
            HeadersOnly = headersOnly;
            Tree = CSharpSyntaxTree.ParseText(sourceCode, path: filename);
            if (headersOnly) return;
            var root = (CompilationUnitSyntax)Tree.GetRoot();
            foreach (var classDeclarationSyntax in root.DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                Methods.AddRange(classDeclarationSyntax
                                 .DescendantNodes()
                                 .OfType<MethodDeclarationSyntax>().Where(method => !(method.ExpressionBody?.Expression is ThrowExpressionSyntax))
                                 .Select(m => (MakeName(classDeclarationSyntax, m.Identifier), m)));
                Fields.AddRange(classDeclarationSyntax.DescendantNodes().OfType<FieldDeclarationSyntax>()
                                                      .SelectMany(f => f.DescendantNodes().OfType<VariableDeclaratorSyntax>())
                                                      .Where(d => d.Initializer != null)
                                                      .Select(f => (MakeName(classDeclarationSyntax, f.Identifier), f)));

            }
        }

        public override string ToString()
        {
            return Tree.FilePath;
        }

        private string MakeName(ClassDeclarationSyntax classDecl, in SyntaxToken id)
        {
            return classDecl.Identifier.Text + "." + id.Text;
        }

        public IEnumerable<(string, int)> GetMethods()
        {
            return Fields.Select(f => (f.Item1, 0)).Concat(Methods.Select(m => (m.Item1, m.Item2.Arity)));
        }
    }
}