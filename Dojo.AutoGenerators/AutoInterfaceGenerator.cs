﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Dojo.Generators.Core.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Dojo.AutoGenerators
{
    [Generator]
    public class AutoInterfaceGenerator : ISourceGenerator
    {
        public AutoInterfaceGenerator()
        {
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif
        }

        private class ClassDefinition
        {
            public string Name { get; set; }
            public string Namespace { get; set; }
            public List<string> Methods { get; set; } = new();
        }

        public static string GetInterfaceName(ITypeSymbol typeSymbol)
        {
            return typeSymbol.Name; // TODO: check what to do with generic
        }

        public static string GetNamespaceFullName(INamespaceSymbol namespaceSymbol)
        {
            return namespaceSymbol.ToString();
        }

        public static string GetMethodDefinition(IMethodSymbol method)
        {
            var bdr = new StringBuilder();

            bdr.Append(method.ReturnType).Append(' ');

            bdr.Append(method.Name);
            if (method.IsGenericMethod)
            {
                List<string> args = new();
                bdr.Append('<');
                foreach (var arg in method.TypeArguments)
                {
                    args.Add(arg.ToString());
                }

                bdr.Append(string.Join(", ", args));
                bdr.Append('>');
            }

            bdr.Append(GetParametersDefinition(method));
            bdr.Append(GetGenericTypeConstraints(method));
            bdr.Append(';');
            return bdr.ToString();
        }

        public static string GetPropertyDefinition(IPropertySymbol propertySymbol)
        {
            var bdr = new StringBuilder();

            bdr.Append(propertySymbol.Type).Append(' ');

            bdr.Append(propertySymbol.Name);

            bdr.Append(" { ");
            if (propertySymbol.GetMethod != null)
            {
                bdr.Append("get;");
            }
            if (propertySymbol.SetMethod != null)
            {
                bdr.Append(" set;");
            }
            bdr.Append(" }");

            return bdr.ToString();
        }

        public static string GetParametersDefinition(IMethodSymbol method)
        {
            List<string> parameters = new();
            StringBuilder bdr = new();
            foreach (var param in method.Parameters)
            {
                bdr.Clear();
                bdr.Append(param.Type);
                bdr.Append(' ');
                bdr.Append(param.Name);
                if (param.HasExplicitDefaultValue)
                {
                    bdr.Append(" = ");
                    if (param.ExplicitDefaultValue is null)
                    {
                        bdr.Append("default(").Append(param.Type).Append(')');
                    }
                    else if (param.ExplicitDefaultValue is string)
                    {
                        bdr.Append('\"').Append(param.ExplicitDefaultValue).Append('\"');
                    }
                    else if (param.ExplicitDefaultValue is bool)
                    {
                        bdr.Append(param.ExplicitDefaultValue.ToString().ToLower());
                    }
                    else
                    {
                        bdr.Append(param.ExplicitDefaultValue);
                    }
                }

                parameters.Add(bdr.ToString());
            }

            return "(" + string.Join(", ", parameters) + ")";
        }

        public static string GetGenericTypeConstraints(IMethodSymbol method)
        {
            if (!method.IsGenericMethod
                || method.TypeParameters.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder bdr = new();
            foreach (var typeParam in method.TypeParameters)
            {
                if (typeParam.ConstraintTypes.Length > 0)
                {
                    bdr.AppendLine();
                    bdr.Append($"           where {typeParam.Name} : {string.Join(", ", typeParam.ConstraintTypes.Select(x => x.ToDisplayString()))}");
                }
                else if (typeParam.HasReferenceTypeConstraint)
                {
                    bdr.AppendLine();
                    bdr.Append($"           where {typeParam.Name} : class");
                }
                else if (typeParam.HasValueTypeConstraint)
                {
                    bdr.AppendLine();
                    bdr.Append($"           where {typeParam.Name} : struct");
                }
            }

            return bdr.ToString();
        }

        public static string GetTypeFullName(ITypeSymbol type)
        {
            return type.ToString();
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxReceiver = context.SyntaxReceiver as ClassWithAttributeSyntaxReceiver;

            var classDefinitions = new List<ClassDefinition>();

            foreach (var classNode in syntaxReceiver.CandidateClasses)
            {
                var semanticModel = context.Compilation.GetSemanticModel(classNode.GetReference().SyntaxTree);

                var isPartial = classNode.IsPartial();
                if (!isPartial)
                {
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "CG1001",
                            "AutoInterface.Failed",
                            "Mark {0} class as partial",
                            "Dojo.CodeGeneration",
                            DiagnosticSeverity.Warning,
                            true),
                        classNode.GetLocation()));
                }
                else
                {
                    var classDefinition = new ClassDefinition();

                    var symbolModel = semanticModel.GetDeclaredSymbol(classNode) as ITypeSymbol;

                    classDefinition.Name = GetInterfaceName(symbolModel);
                    classDefinition.Namespace = GetNamespaceFullName(symbolModel.ContainingNamespace);

                    foreach (var member in symbolModel.GetMembers())
                    {
                        if (!member.IsAbstract && !member.IsStatic && !member.IsVirtual && !member.IsOverride
                        && member.DeclaredAccessibility == Accessibility.Public
                        && !member.IsImplicitlyDeclared
                        )
                        {
                            if (member is IPropertySymbol property)
                            {
                                var propertyDefinition = GetPropertyDefinition(property);
                                classDefinition.Methods.Add(propertyDefinition);
                            }
                            else if (member is IMethodSymbol method)
                            {
                                if (method.MethodKind == MethodKind.Constructor
                                    || method.MethodKind == MethodKind.PropertyGet
                                    || method.MethodKind == MethodKind.PropertySet
                                    || method.MethodKind == MethodKind.EventAdd
                                    || method.MethodKind == MethodKind.EventRaise
                                    || method.MethodKind == MethodKind.EventRemove
                                    || method.MethodKind == MethodKind.Destructor)
                                {
                                    continue;
                                }
                                var methodDefinition = GetMethodDefinition(method);
                                Console.WriteLine(methodDefinition);
                                classDefinition.Methods.Add(methodDefinition);
                            }
                        }
                    }

                    classDefinitions.Add(classDefinition);
                }
            }

            foreach (var classDefinition in classDefinitions)
            {
                // begin creating the source we'll inject into the users compilation
                var sourceBuilder = new StringBuilder(@$"
using System;
using System.CodeDom.Compiler;

namespace {classDefinition.Namespace}
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class {classDefinition.Name}: I{classDefinition.Name}
    {{
    }}

    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public interface I{classDefinition.Name}
    {{
");
                // add the filepath of each tree to the class we're building
                foreach (var method in classDefinition.Methods)
                {
                    sourceBuilder.Append("        ").Append(method).AppendLine("\r");
                }

                // finish creating the source to inject
                sourceBuilder.Append(@"    }
}");

                // inject the created source into the users compilation
                context.AddSource($"I{classDefinition.Name}.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
            //#endif

            //Debug.WriteLine("Initialize code generator");

            context.RegisterForSyntaxNotifications(() => new ClassWithAttributeSyntaxReceiver("AutoInterface"));
        }
    }
}
