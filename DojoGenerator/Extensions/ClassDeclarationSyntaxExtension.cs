using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DojoGenerator
{
    public static class ClassDeclarationSyntaxExtension
    {
        public static bool IsPartial(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Modifiers.Any(m=>m.Text == "partial");
        }
    }
}