using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;

namespace Infinitesimal
{
    [Generator]
    public class InfinitesimalGenerator : ISourceGenerator
    {
        private static readonly string[] _attributeNames = new [] { "HttpGet", "HttpPost", "HttpPatch", "HttpPut", "HttpDelete" };

        private const string _defaultRoute = "\"/\"";

        private const string _templateStart = @"
namespace Infinitesimal
{
    public static class InfinitesimalWebApp
    {
        private static readonly IEnumerable<string> _HttpGet = new[] { ""GET"" };
        private static readonly IEnumerable<string> _HttpPost = new[] { ""POST"" };
        private static readonly IEnumerable<string> _HttpPatch = new[] { ""PATCH"" };
        private static readonly IEnumerable<string> _HttpPut = new[] { ""PUT"" };
        private static readonly IEnumerable<string> _HttpDelete = new[] { ""DELETE"" };

        public static WebApplication MapFunctions(this WebApplication app)
        {
            // Methods mapping
";

        private const string _templateEnd = @"
            return app;
        }

        public static void Run(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.MapFunctions();
            app.Run();
        }
    }
}";

        private const string _globalUsing = @"
global using Microsoft.AspNetCore.Mvc;
global using Infinitesimal;";

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxContextReceiver = (SyntaxReceiver)context.SyntaxContextReceiver!;
            var usingDirectives = syntaxContextReceiver.UsingDirectives;
            var methods = syntaxContextReceiver.Methods;

            var codeBuilder = new StringBuilder(_templateStart);

            foreach (var method in methods)
            {
                var attributes = method.AttributeLists
                    .SelectMany(al => al.Attributes)
                    .Where(a => _attributeNames.Contains($"{a.Name}"))
                    .ToArray();

                foreach (var attribute in attributes)
                {
                    var methodName = ((MethodDeclarationSyntax)method).Identifier.ValueText;
                    var httpMethodName = $"_{attribute.Name}";
                    var route = attribute.ArgumentList?.Arguments.Select(a => a.Expression).FirstOrDefault()?.ToString() ?? _defaultRoute;

                    codeBuilder.AppendLine($"app.MapMethods({route}, {httpMethodName}, {methodName});");
                }
            }

            codeBuilder.Append(_templateEnd);

            var compilationUnit = SyntaxFactory
                .ParseCompilationUnit(codeBuilder.ToString())
                .WithUsings(usingDirectives)
                .NormalizeWhitespace();

            var applicationClass = compilationUnit
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .First();

            var combinedClass = applicationClass.AddMembers(methods);

            compilationUnit = compilationUnit
                .ReplaceNode(applicationClass, combinedClass)
                .NormalizeWhitespace();

            var syntaxTree = SyntaxFactory.SyntaxTree(compilationUnit);
            var code = syntaxTree.ToString();

            context.AddSource("_infinitesimalWebApp", code);

            //if (Debugger.IsAttached)
            //{
            //    Debugger.Break();
            //}
            //else
            //{
            //    Debugger.Launch();
            //}
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(pi => pi.AddSource("_infinitesimalGlobalUsing", _globalUsing));

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }
    }
}
