using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dojo.Generators.Core.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Dojo.AutoGenerators
{
    [Generator]
    public class AutoExceptionGenerator : ISourceGenerator
    {
        private class ClassDefinition
        {
            public string Name { get; set; }
            public string Namespace { get; set; }
            public string BaseType { get; set; }
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = context.SyntaxReceiver as ClassWithAttributeSyntaxReceiver;

            var classDefinitions = new List<ClassDefinition>();

            foreach (var classNode in syntaxReceiver.CandidateClasses)
            {
                var semanticModel = context.Compilation.GetSemanticModel(classNode.GetReference().SyntaxTree);
                
                var isPartial = classNode.IsPartial();
                if (isPartial)
                {
                    var classDefinition = new ClassDefinition();
                    var symbolModel = semanticModel.GetDeclaredSymbol(classNode) as ITypeSymbol;

                    classDefinition.Name = symbolModel.Name;
                    classDefinition.Namespace = symbolModel.ContainingNamespace.ToString();
                    classDefinition.BaseType = symbolModel.BaseType.ToString();

                    classDefinitions.Add(classDefinition);
                }
            }
            foreach (var classDefinition in classDefinitions)
            {
                var baseTypeDefinition = string.Equals(classDefinition.BaseType, nameof(Object), StringComparison.OrdinalIgnoreCase) ? " : Exception" : "";
                // begin creating the source we'll inject into the users compilation
                var sourceBuilder = new StringBuilder(@$"
using System;
using System.CodeDom.Compiler;
using System.Runtime.Serialization;

namespace {classDefinition.Namespace}
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class {classDefinition.Name}{baseTypeDefinition}
    {{
        public {classDefinition.Name}()
        {{
        }}

        public {classDefinition.Name}(string message) : base(message)
        {{
        }}

        public {classDefinition.Name}(string message, Exception innerException) : base(message, innerException)
        {{
        }}

        protected {classDefinition.Name}(SerializationInfo info, StreamingContext context) : base(info, context)
        {{
        }}
    }}
}}
");
                // inject the created source into the users compilation
                context.AddSource($"{classDefinition.Name}.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new ClassWithAttributeSyntaxReceiver("AutoException"));
        }
    }
}
