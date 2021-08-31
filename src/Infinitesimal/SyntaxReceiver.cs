using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Infinitesimal
{
    class SyntaxReceiver : ISyntaxContextReceiver
    {
        private readonly Dictionary<string, List<UsingDirectiveSyntax>> _usingDirectives = new();
        private readonly List<LocalFunctionStatementSyntax> _functions = new ();
        private readonly List<string> _filePaths = new();

        public SyntaxList<UsingDirectiveSyntax> UsingDirectives => SyntaxFactory.List(_usingDirectives
            .Where(kp => _filePaths.Contains(kp.Key))
            .SelectMany(kp => kp.Value)
            .ToList());

        public MemberDeclarationSyntax[] Methods => _functions
            .Select(f => SyntaxFactory
                .MethodDeclaration(f.ReturnType, f.Identifier)
                .WithAttributeLists(f.AttributeLists)
                .WithModifiers(f.Modifiers.Add(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
                .WithParameterList(f.ParameterList)
                .WithBody(f.Body)
                .WithExpressionBody(f.ExpressionBody)
                .WithSemicolonToken(f.SemicolonToken))
            .ToArray<MemberDeclarationSyntax>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node.IsKind(SyntaxKind.UsingDirective))
            {
                var filePath = context.Node.SyntaxTree.FilePath;
                var usingDirective = (UsingDirectiveSyntax)context.Node;

                if (_usingDirectives.TryGetValue(filePath, out var usingDirectives))
                {
                    usingDirectives.Add(usingDirective);
                }
                else
                {
                    _usingDirectives[filePath] = new () { usingDirective };
                }

                return;
            }

            if (context.Node.IsKind(SyntaxKind.LocalFunctionStatement) &&
                context.Node.Parent.IsKind(SyntaxKind.GlobalStatement))
            {
                var filePath = context.Node.SyntaxTree.FilePath;
                var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);

                if (symbol is IMethodSymbol methodSymbol && methodSymbol.IsStatic)
                {
                    var function = (LocalFunctionStatementSyntax)context.Node;

                    _functions.Add(function);
                    _filePaths.Add(filePath);
                }
            }
        }
    }
}
