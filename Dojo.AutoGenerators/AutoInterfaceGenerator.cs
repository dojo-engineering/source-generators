using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Dojo.Generators.Core.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            public bool IsGeneric { get; set; }
            public string GenericArguments { get; set; }
            public string GenericConstraints { get; set; }
            public string FullName => IsGeneric ? $"{Name}{GenericArguments}" : Name;
            public int Lifetime { get; set; } = 0; // 0=Scoped, 1=Singleton, 2=Transient
        }
    
        public static string GetGenericTypeArguments(INamedTypeSymbol classSymbol)
        {
            if (!classSymbol.IsGenericType) return null;

            var bdr = new StringBuilder();

            bdr.Append('<');
            var args = classSymbol.TypeArguments.Select(arg => arg.ToString()).ToList();
            bdr.Append(string.Join(", ", args));
            bdr.Append('>');

            return bdr.ToString();
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
            bdr.Append(GetGenericTypeConstraints(method, symbol => symbol.IsGenericMethod, symbol => symbol.TypeParameters));
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

        public static string GetGenericTypeConstraints<T>(T symbol, Func<T, bool> isGeneric, Func<T, IEnumerable<ITypeParameterSymbol>> getTypeParameters)
        {
            var typeParameters = getTypeParameters(symbol)?.ToList() ?? new List<ITypeParameterSymbol>();
            if (!isGeneric(symbol) || typeParameters.Count == 0)
            {
                return string.Empty;
            }

            StringBuilder bdr = new();
            foreach (var typeParam in typeParameters)
            {
                if (typeParam.ConstraintTypes.Length > 0 || typeParam.HasReferenceTypeConstraint ||
                    typeParam.HasValueTypeConstraint || typeParam.HasConstructorConstraint)
                {
                    bdr.AppendLine();
                    bdr.Append($"           where {typeParam.Name} : ");
                }
                else
                {
                    continue;
                }

                var constraints = new List<string>();
                if (typeParam.ConstraintTypes.Length > 0)
                {
                    constraints.Add($"{string.Join(", ", typeParam.ConstraintTypes.Select(x => x.ToDisplayString()))}");
                }

                if (typeParam.HasReferenceTypeConstraint)
                {
                    constraints.Add("class");
                }

                if (typeParam.HasValueTypeConstraint)
                {
                    constraints.Add("struct");
                }
                
                if (typeParam.HasConstructorConstraint)
                {
                    constraints.Add("new()");
                }

                bdr.Append(string.Join(", ", constraints));
            }

            return bdr.ToString();
        }

        public static string GetTypeFullName(ITypeSymbol type)
        {
            return type.ToString();
        }

        public static int GetLifetimeFromAttribute(INamedTypeSymbol classSymbol)
        {
            var attributeData = classSymbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "AutoInterfaceAttribute");

            if (attributeData == null) return 0;

            foreach (var namedArg in attributeData.NamedArguments)
            {
                if (namedArg.Key == "Lifetime" && namedArg.Value.Value != null)
                {
                    try { return Convert.ToInt32(namedArg.Value.Value); }
                    catch { return 0; }
                }
            }

            return 0; // Default: Scoped
        }

        public static int GetLifetimeFromSyntax(ClassDeclarationSyntax classNode)
        {
            var attribute = classNode.AttributeLists
                .SelectMany(al => al.Attributes)
                .FirstOrDefault(a => a.Name.ToFullString().Trim() == "AutoInterface");

            if (attribute?.ArgumentList == null) return 0;

            foreach (var arg in attribute.ArgumentList.Arguments)
            {
                if (arg.NameEquals?.Name?.Identifier.Text == "Lifetime")
                {
                    var valueText = arg.Expression.ToFullString().Trim();
                    if (valueText.EndsWith("Transient")) return 2;
                    if (valueText.EndsWith("Singleton")) return 1;
                }
            }

            return 0; // Default: Scoped
        }

        private static string GetDiMethodName(int lifetime) => lifetime switch
        {
            1 => "AddSingleton",
            2 => "AddTransient",
            _ => "AddScoped"
        };

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

                    var symbolModel = semanticModel.GetDeclaredSymbol(classNode) as INamedTypeSymbol;

                    classDefinition.Name = GetInterfaceName(symbolModel);
                    classDefinition.Namespace = GetNamespaceFullName(symbolModel.ContainingNamespace);
                    classDefinition.IsGeneric = symbolModel.IsGenericType;
                    classDefinition.GenericArguments = GetGenericTypeArguments(symbolModel);
                    classDefinition.GenericConstraints = GetGenericTypeConstraints(symbolModel, symbol => symbol.IsGenericType, symbol => symbol.TypeParameters);
                    classDefinition.Lifetime = GetLifetimeFromSyntax(classNode);

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
    public partial class {classDefinition.FullName}: I{classDefinition.FullName}{classDefinition.GenericConstraints}
    {{
    }}

    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public interface I{classDefinition.FullName}{classDefinition.GenericConstraints}
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

            // Generate the IServiceCollection extension with all non-generic registrations
            var diCandidates = classDefinitions.Where(c => !c.IsGeneric).ToList();
            if (diCandidates.Count > 0)
            {
                var assemblyName = context.Compilation.AssemblyName ?? "AutoGeneratedExtensions";
                var diBuilder = new StringBuilder();
                diBuilder.AppendLine($@"
using System;
using System.CodeDom.Compiler;
using Microsoft.Extensions.DependencyInjection;

namespace {assemblyName}
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public static class AutoInterfaceServiceCollectionExtensions
    {{
        public static IServiceCollection AddAutoInterfaces(this IServiceCollection services)
        {{
            if (services == null) throw new ArgumentNullException(nameof(services));");

                foreach (var classDef in diCandidates)
                {
                    var method = GetDiMethodName(classDef.Lifetime);
                    diBuilder.AppendLine($"            services.{method}<{classDef.Namespace}.I{classDef.Name}, {classDef.Namespace}.{classDef.Name}>();");
                }

                diBuilder.AppendLine(@"            return services;
        }
    }
}");
                context.AddSource("AutoInterfaceServiceCollectionExtensions.g.cs", SourceText.From(diBuilder.ToString(), Encoding.UTF8));
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
