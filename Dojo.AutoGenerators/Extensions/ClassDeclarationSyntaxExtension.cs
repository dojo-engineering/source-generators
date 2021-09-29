using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dojo.AutoGenerators
{
    public static class ClassDeclarationSyntaxExtension
    {
        public static bool IsPartial(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Modifiers.Any(m=>m.Text == "partial");
        }
    }
}