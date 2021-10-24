using Dojo.AutoGenerators.Utils;
using Dojo.Generators.Abstractions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.XPath;

namespace Dojo.AutoGenerators.AppSettingsGenerator
{
    internal class SettingsGroupParser
    {
        private const string DefaultSettingsJsonFile = "appsettings.json";

        private static readonly DiagnosticDescriptor OnlyStringLiteralsSupportedForExcludedSections = new(
          id: "CG1003",
          title: "Only string literals are supported",
          messageFormat: $"Only string literals are supported for '{nameof(AutoAppSettingsAttribute.ExcludedSections)}' argument. The value is ignored.",
          category: "Dojo.SourceGenerators",
          DiagnosticSeverity.Warning,
          isEnabledByDefault: true);

        internal static List<SettingsGroup> GetSettingFileGroups(GeneratorExecutionContext context, string settingsAttributeName)
        {
            var result = new List<SettingsGroup>();
            var syntaxReceiver = context.SyntaxReceiver as ClassWithAttributeSyntaxReceiver;

            foreach (var settingsClass in syntaxReceiver.CandidateClasses)
            {
                var semanticModel = context.Compilation.GetSemanticModel(settingsClass.GetReference().SyntaxTree);
                var symbolModel = semanticModel.GetDeclaredSymbol(settingsClass) as ITypeSymbol;
                var attributes = settingsClass.AttributeLists
                    .Select(a => a.Attributes.FirstOrDefault(b => b.Name.ToFullString() == settingsAttributeName)).ToList();

                var fileNames = new HashSet<string>();
                foreach (var attribute in attributes)
                {
                    var parseRes = ParseAttributeArguments(context, attribute?.ArgumentList);

                    result.Add(new SettingsGroup
                    {
                        FileName = parseRes.Filename,
                        ClassName = symbolModel.Name,
                        Namespace = symbolModel.ContainingNamespace.ToString(),
                        GenerateConventionalTypes = parseRes.GenerateConventionalTypes,
                        IsExplicitAttributeSpecified = true
                    });
                }
            }
            
            // Default, nothing specified
            if (!result.Any())
            {
                result.Add(new SettingsGroup
                {
                    FileName = DefaultSettingsJsonFile,
                    ClassName = "AppSettings",
                    Namespace = TryGetProjectDefaultNamespace(context),
                    GenerateConventionalTypes = true,
                    IsExplicitAttributeSpecified = false
                });
            }

            return result;
        }

        private static (string Filename, bool GenerateConventionalTypes, string[] ExcludedSections) ParseAttributeArguments(
            GeneratorExecutionContext context, 
            AttributeArgumentListSyntax argumentList)
        {
            string filename = null;
            bool generateConventionalTypes = false;
            string[] excludedSections = null;

            if (argumentList != null)
            {
                foreach (var arg in argumentList?.Arguments)
                {
                    string name = arg.NameEquals?.Name?.Identifier.Text;
                    string value = arg.Expression.ToString().Trim('\"');

                    // both syntaxes covered: AutoAppSettingsAttribute("appsettings.json")
                    // and AutoAppSettingsAttribute(FileName = "appsettings.json")
                    if (name == null || name == nameof(AutoAppSettingsAttribute.FileName))
                    {
                        filename = value;
                    }

                    if (name == nameof(AutoAppSettingsAttribute.UseConventionalTypes))
                    {
                        generateConventionalTypes = bool.Parse(value);
                    }

                    if (name == nameof(AutoAppSettingsAttribute.ExcludedSections))
                    {
                        excludedSections = ParseStringArrayExpression(context, arg.Expression);
                    }
                }
            }

            filename ??= DefaultSettingsJsonFile;
            return (filename, generateConventionalTypes, excludedSections);
        }

        private static string[] ParseStringArrayExpression(GeneratorExecutionContext context, ExpressionSyntax expression)
        {
            InitializerExpressionSyntax initializer = null;

            if (expression is ImplicitArrayCreationExpressionSyntax implicitArrayCreationSyntax)
            {
                initializer = implicitArrayCreationSyntax.Initializer;
            }
            else if (expression is ArrayCreationExpressionSyntax arrayCreationExpressionSyntax)
            {
                initializer = arrayCreationExpressionSyntax.Initializer;
            }

            string[] rawValues = initializer.Expressions.Select(x => x.ToString()).ToArray();
            
            if (rawValues.Any(x => !x.Trim().StartsWith("\"", StringComparison.OrdinalIgnoreCase)))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(OnlyStringLiteralsSupportedForExcludedSections, 
                    initializer.GetLocation()));

                return null;
            }

            return rawValues.Select(x => x.Trim('\"')).ToArray();
        }

        private static string TryGetProjectDefaultNamespace(GeneratorExecutionContext context)
        {
            if (context.TryGetProjectDir(out string projectDir))
            {
                string project = FileSystemUtil.FindFilesWithExtension(projectDir, ".csproj").SingleOrDefault();
                if (project != null)
                {
                    using var fileStream = File.Open(project, FileMode.Open);
                    XPathDocument xPath = new XPathDocument(fileStream);
                    XPathNavigator navigator = xPath.CreateNavigator();

                    var result = navigator.SelectSingleNode("Project/PropertyGroup/RootNamespace");
                    if (result != null)
                    {
                        return result.Value;
                    }

                    return Path.GetFileNameWithoutExtension(project);
                }
            }

            // Not found, return something
            return "GeneratedSettings";
        }
    }
}
