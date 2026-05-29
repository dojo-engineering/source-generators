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

        private bool IsExpectedAttributeName(string attributeName)
        {
            if (attributeName == ExpectedAttributeName)
            {
                return true;
            }

            if (attributeName == $"{ExpectedAttributeName}Attribute")
            {
                return true;
            }

            if (ExpectedAttributeName.EndsWith("Attribute")
                && attributeName == ExpectedAttributeName.Substring(0, ExpectedAttributeName.Length - "Attribute".Length))
            {
                return true;
            }

            return false;
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classSyntax)
            {
                var attribute = classSyntax.AttributeLists
                    .Select(a => a.Attributes.FirstOrDefault(b =>
                    {
                        var name = b.Name.ToString().Split('.').Last().Trim();
                        if (name.StartsWith("global::"))
                        {
                            name = name.Substring("global::".Length);
                        }

                        return IsExpectedAttributeName(name);
                    }))
                    .FirstOrDefault(a => a != null);

                if (attribute is not null)
                {
                    CandidateClasses.Add(classSyntax);
                }
            }
        }
    }
}