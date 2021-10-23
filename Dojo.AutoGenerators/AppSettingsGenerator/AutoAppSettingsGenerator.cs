using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dojo.AutoGenerators.Utils;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json.Linq;

namespace Dojo.AutoGenerators.AppSettingsGenerator
{
    [Generator]
    public partial class AutoAppSettingsGenerator : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor MissingSettingsFilesError = new(
            id: "CG1002",
            title: "Couldn't find specified settings files",
            messageFormat: "Couldn't find specified settings files: '{0}' neither in the project nor in the 'AdditionalFiles'" +
                "Make sure the settings file is added as 'AdditionalFiles' to the project. " +
                "See here: https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Using%20Additional%20Files.md.",
            category: "Dojo.SourceGenerators",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private const string SettingsAttributeName = "AutoAppSettings";
        private TemplateEngine _templateEngine = new TemplateEngine();

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() =>
            {
                return new ClassWithAttributeSyntaxReceiver(SettingsAttributeName);
            });
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Uncomment this for debugging
#if DEBUG
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
#endif

            List<SettingsGroup> fileGroups = SettingsGroupParser.GetSettingFileGroups(context, SettingsAttributeName);

            foreach (SettingsGroup fileGroup in fileGroups)
            {
                GenerateSettingsGroup(context, fileGroup);
            }
        }
       
        private bool IsPathInFileGroup(string path, SettingsGroup fileGroup)
        {
            if (path.EndsWith(fileGroup.FileName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private ParsedSettings ParseSettingsFile(GeneratorExecutionContext context, SettingsGroup fileGroup)
        {
            Dictionary<string, Tuple<string, JTokenType>> properties = new();

            string content = GetFileContentsForSettingsGroup(context, fileGroup);
            if (content == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(MissingSettingsFilesError, null, fileGroup.FileName));
                return null;
            }

            SettingsJsonParser parser = new SettingsJsonParser(
                content, 
                fileGroup.GenerateConventionalTypes, 
                fileGroup.ExcludedSections);

            return parser.Parse();
        }

        private string GetFileContentsForSettingsGroup(GeneratorExecutionContext context, SettingsGroup fileGroup)
        {
            // Check filesystem directly first
            if (context.TryGetProjectDir(out string projectDir))
            {
                string[] filePaths = FileSystemUtil.FindFiles(
                    projectDir,
                    fileGroup.FileName);

                if (filePaths.Any())
                {
                    return File.ReadAllText(filePaths.Single());
                }
            }

            // Check additional files
            foreach (var file in context.AdditionalFiles)
            {
                if (IsPathInFileGroup(file.Path, fileGroup))
                {
                    return file.GetText()?.ToString();
                }
            }

            return null;
        }

        private void GenerateSettingsGroup(GeneratorExecutionContext context, SettingsGroup fileGroup)
        {
            ParsedSettings parsedSettings = ParseSettingsFile(context, fileGroup);
            if (parsedSettings == null)
            {
                return;
            }

            var sources = new Dictionary<string, string>();
            string settingsNamespace = fileGroup.Namespace;
            var rootProperties = new StringBuilder();

            foreach (var prop in parsedSettings.Properties)
            {
                string rootProperty = ExpandMacro("Property.template", new Dictionary<string, string>
                {
                    ["propertyType"] = prop.Value.Item1,
                    ["propertyName"] = prop.Key
                });

                rootProperties.AppendLine(rootProperty);
                rootProperties.AppendLine();
            }

            var rootFile = _templateEngine.Expand("ClassDefinition.template", new Dictionary<string, string>
            {
                ["settingsNamespace"] = settingsNamespace,
                ["className"] = fileGroup.ClassName,
                ["assemblyVersion"] = AssemblyUtils.AssemblyVersion,
                ["properties"] = rootProperties.ToString().TrimEnd(),
            });

            sources.Add($"{fileGroup.ClassName}.g.cs", rootFile);

            foreach (var classDefinition in parsedSettings.Classes.Values)
            {
                var propertiesBuilder = new StringBuilder();
                foreach (var prop in classDefinition.Properties)
                {
                    string property = ExpandMacro("Property.template", new Dictionary<string, string>
                    {
                        ["propertyType"] = prop.TypeName,
                        ["propertyName"] = prop.Name,
                    });
                    propertiesBuilder.AppendLine(property);
                    propertiesBuilder.AppendLine();
                }

                var classDefinitionContent = _templateEngine.Expand("ClassDefinition.template", new Dictionary<string, string>
                {
                    ["settingsNamespace"] = settingsNamespace,
                    ["assemblyVersion"] = AssemblyUtils.AssemblyVersion,
                    ["className"] = classDefinition.Name,
                    ["properties"] = propertiesBuilder.ToString().TrimEnd()
                });

                // inject the created source into the users compilation
                sources.Add($"{classDefinition.Name}.g.cs", classDefinitionContent);
            }

            // AppSettingsExtender
            var nestedClassInjection = new StringBuilder();

            foreach (var prop in parsedSettings.Properties.Where(x => x.Value.Item2 == JTokenType.Object))
            {
                string classInjection = ExpandMacro("NestedClassInjection.template", new Dictionary<string, string>
                {
                    ["parent"] = "rootObject",
                    ["propertyName"] = prop.Key,
                });
                nestedClassInjection.AppendLine(classInjection);
            }

            var appSettingsExtender = _templateEngine.Expand("AppSettingsExtender.template", new Dictionary<string, string>
            {
                ["settingsNamespace"] = settingsNamespace,
                ["className"] = fileGroup.ClassName,
                ["assemblyVersion"] = AssemblyUtils.AssemblyVersion,
                ["nestedClassesInjection"] = nestedClassInjection.ToString().TrimEnd()
            });

            sources.Add($"{fileGroup.ClassName}Extender.g.cs", appSettingsExtender);

            context.AddSources(sources);
        }

        private string ExpandMacro(string templateName, Dictionary<string, string> parameters)
        {
            return _templateEngine.Expand(templateName, parameters);
        }
    }
}
