using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dojo.AutoGenerators
{
    internal class ClassWithAttributeSyntaxReceiver : ISyntaxReceiver
    {
        private string ExpectedAttributeName { get; }

        public ClassWithAttributeSyntaxReceiver(string expectedAttributeName)
        {
            ExpectedAttributeName = expectedAttributeName;
        }
        public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

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