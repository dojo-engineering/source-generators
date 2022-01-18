using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[assembly: InternalsVisibleTo("Dojo.AutoGenerators")]
[assembly: InternalsVisibleTo("Dojo.Generators.Tests")]
namespace Dojo.Generators.Core.CodeAnalysis
{
    internal class ClassWithAttributeSyntaxReceiver : ISyntaxReceiver
    {
        private string ExpectedAttributeName { get; }

        public ClassWithAttributeSyntaxReceiver(string expectedAttributeName)
        {
            ExpectedAttributeName = expectedAttributeName;
        }
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classSyntax)
            {
                var attribute = classSyntax.AttributeLists.Select(
                    a => a.Attributes.FirstOrDefault(b => b.Name.ToFullString() == ExpectedAttributeName)).FirstOrDefault(a => a != null);
                
                if (attribute is not null)
                {
                    CandidateClasses.Add(classSyntax);
                }
            }
        }
    }
}