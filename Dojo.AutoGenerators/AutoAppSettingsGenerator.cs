using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dojo.AutoGenerators
{
    [Generator]
    public class AppSettingsGenerator : ISourceGenerator
    {
        private static readonly DiagnosticDescriptor MissingSettingsFilesError = new(id: "CG1002",
            title: "Couldn't find specified settings files",
            messageFormat: "Couldn't find specified settings files: '{0}'. Make sure they are added as AdditionalFiles to the project.",
            category: "Dojo.SourceGenerators",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private const string SettingsAttributeName = "AutoAppSettings";
        private class PropertyDefinition
        {
            public string Name { get; set; }
            public string TypeName { get; set; }
            public JTokenType Type { get; set; }
        }
        private class ClassDefinition
        {
            public string Name { get; set; }
            public List<PropertyDefinition> Properties { get; set; } = new();
        }

        private Dictionary<string, ClassDefinition> _classes = new();

        public void Initialize(GeneratorInitializationContext context)
        {
             context.RegisterForSyntaxNotifications(() =>
             {
                 return new ClassWithAttributeSyntaxReceiver(SettingsAttributeName);
             });
        }

        private ClassDefinition ParseObject(string name, JObject @object)
        {
            var classDefinition = new ClassDefinition
            {
                Name = $"{name}Settings"
            };

            foreach (var child in @object.Children())
            {
                if (child is JProperty prop)
                {
                    if (prop.Value.Type == JTokenType.Object)
                    {
                        var objType = ParseObject(prop.Name, prop.Value as JObject);
                        classDefinition.Properties.Add(new PropertyDefinition()
                        {
                            Name = prop.Name,
                            Type = prop.Value.Type,
                            TypeName = objType.Name
                        });
                        _classes[objType.Name] = (objType); // TODO: check if class exists and merge props
                    }
                    else if (prop.Value.Type == JTokenType.Array)
                    {
                        var childType = ParseArray(prop.Name, prop.Value as JArray);
                        classDefinition.Properties.Add(new PropertyDefinition()
                        {
                            Name = prop.Name,
                            Type = prop.Value.Type,
                            TypeName = childType
                        });
                    }
                    else
                    {
                        classDefinition.Properties.Add(new PropertyDefinition()
                        {
                            Name = prop.Name,
                            Type = prop.Value.Type,
                            TypeName = JTokenTypeToNetType(prop.Value.Type)
                        });
                    }
                }
            }

            return classDefinition;
        }

        private static string JTokenTypeToNetType(JTokenType? tokenType) => tokenType switch
        {
            null => "object",
            JTokenType.Boolean => "bool",
            JTokenType.String => "string",
            JTokenType.Integer => "int",
            JTokenType.Float => "float",
            _ => ""
        };

        private string ParseArray(string name, JArray @array)
        {
            var props = new Dictionary<string, PropertyDefinition>();
            var firstChild = @array.Children().FirstOrDefault();
            string childType;
            if (firstChild?.Type == JTokenType.Object)
            {
                foreach (var child in @array.Children())
                {
                    var typeDeclaration = ParseObject("", child.Value<JObject>());
                    foreach (var prop in typeDeclaration.Properties)
                    {
                        props[prop.Name] = prop;
                    }
                }

                var classDeclaration = new ClassDefinition
                {
                    Name = MakeSingleNameSettings(name),
                    Properties = props.Values.ToList()
                };

                _classes[classDeclaration.Name] = classDeclaration; // TODO: check if class exists and merge props
                childType = classDeclaration.Name;
            }
            else
            {
                childType = JTokenTypeToNetType(firstChild?.Type);
            }

            return $"IEnumerable<{childType}>";
        }

        private static string MakeSingleNameSettings(string name)
        {
            // TODO: use library for this

            if (name.EndsWith("s"))
            {
                name = name.Substring(0, name.Length - 1);
            }

            return $"{name}Settings";
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var fileGroups = GetSettingFileGroups(context);

            foreach (var fileGroup in fileGroups)
            {
                GenerateSettingsGroup(context, fileGroup);
            }
        }

        private bool IsPathInFileGroup(string path, SettingsGroup fileGroup)
        {
            foreach (var file in fileGroup.FileList)
            {
                if (path.EndsWith(file, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private void GenerateSettingsGroup(GeneratorExecutionContext context, SettingsGroup fileGroup)
        {
            Dictionary<string, Tuple<string, JTokenType>> properties = new();
            var foundSettingFiles = false;

            foreach (var file in context.AdditionalFiles)
            {
                if (IsPathInFileGroup(file.Path, fileGroup))
                {
                    foundSettingFiles = true;
                    var appsettings = DeserializeToDictionary(file.GetText()?.ToString());

                    foreach (var child in appsettings.Children<JToken>())
                    {
                        if (child is JProperty prop)
                        {
                            switch (prop.Value.Type)
                            {
                                case JTokenType.Object:
                                    var objType = ParseObject(prop.Name, prop.Value as JObject);
                                    properties.Add(prop.Name, Tuple.Create(objType.Name, prop.Value.Type));
                                    _classes[objType.Name] = objType;
                                    break;
                                case JTokenType.Array:
                                    var propType = ParseArray(prop.Name, prop.Value as JArray);
                                    properties.Add(prop.Name, Tuple.Create(propType, prop.Value.Type));
                                    break;
                                default:
                                    properties.Add(prop.Name,
                                        Tuple.Create(JTokenTypeToNetType(prop.Value.Type), prop.Value.Type));
                                    break;
                            }
                        }
                    }
                }
            }

            if(!foundSettingFiles){
                context.ReportDiagnostic(Diagnostic.Create(MissingSettingsFilesError, null, string.Join(", ", fileGroup.FileList)));
                return;
            }

            string settingsNamespace = fileGroup.Namespace;
            var rootProperties = new StringBuilder();
            var rootPropertiesInitializers = new StringBuilder();

            foreach (var prop in properties)
            {
                rootProperties.AppendLine($"        public {prop.Value.Item1} {prop.Key} {{get;}}");
                if (prop.Value.Item2 == JTokenType.Object)
                {
                    rootPropertiesInitializers.AppendLine($@"
            this.{prop.Key} = new {prop.Value.Item1}();
            configuration.GetSection(""{prop.Key}"").Bind(this.{prop.Key});");
                }
                else if (prop.Value.Item2 == JTokenType.String)
                {
                    rootPropertiesInitializers.AppendLine($@"
            this.{prop.Key} = configuration[""{prop.Key}""];");
                }
                else if (prop.Value.Item2 == JTokenType.Array)
                {
                    var arrayItemTypeName = MakeSingleNameSettings(prop.Key);
                    if (!_classes.TryGetValue(arrayItemTypeName, out var arrayItemType))
                    {
                        continue;
                    }

                    StringBuilder propsBuilder = new();
                    foreach (var initProp in arrayItemType.Properties)
                    {
                        propsBuilder.AppendLine($@"                    {initProp.Name} = x.GetValue<{initProp.TypeName}>(""{initProp.Name}""),");
                    }
                    rootPropertiesInitializers.AppendLine($@"
            this.{prop.Key} = configuration.GetSection(""{prop.Key}"")
                .GetChildren()
                .Select(x => new {arrayItemTypeName}
                {{
{propsBuilder.ToString()}
                }}).ToList();");
                }
            }

            var rootBuild = new StringBuilder($@"
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace {settingsNamespace}
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class {fileGroup.ClassName}
    {{
{rootProperties}

        public AppSettings(IConfiguration configuration)
        {{
            if (configuration == null)
            {{
                throw new ArgumentNullException(nameof(configuration));
            }}

{rootPropertiesInitializers}
        }}
    }}
}}");
            context.AddSource($"{fileGroup.ClassName}.g.cs",
                SourceText.From(rootBuild.ToString(), Encoding.UTF8));


            foreach (var classDefinition in _classes.Values)
            {
                var propertiesBuilder = new StringBuilder();
                foreach (var prop in classDefinition.Properties)
                {
                    propertiesBuilder
                        .Append("        public ")
                        .Append(prop.TypeName)
                        .Append(' ')
                        .Append(prop.Name)
                        .AppendLine(" { get; set; }");
                }

                var sourceBuilder = new StringBuilder(@$"
using System;
using System.CodeDom.Compiler;

namespace {settingsNamespace}
{{
    [GeneratedCode(""Dojo.SourceGenerator"", ""{Assembly.GetExecutingAssembly().GetName().Version}"")]
    public partial class {classDefinition.Name}
    {{
{propertiesBuilder}
    }}
}}");
                // inject the created source into the users compilation
                context.AddSource($"{classDefinition.Name}.g.cs",
                    SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));

                // File.WriteAllText(
                //     $@"/Users/alexmalafeev/paymentsense/source-generators/Dojo.Generators.Tests/out/{classDefinition.Name}.g.cs",
                //     sourceBuilder.ToString()
                //     );
                // File.WriteAllText(
                //     $@"/Users/alexmalafeev/paymentsense/source-generators/Dojo.Generators.Tests/out/{fileGroup.ClassName}.g.cs",
                //     rootBuild.ToString());

            }
        }

        record SettingsGroup
        {
            public List<string> FileList { get; set; }
            public string ClassName { get; set; }
            public string Namespace { get; set; }
        }

        private static IEnumerable<SettingsGroup> GetSettingFileGroups(GeneratorExecutionContext context)
        {
            var syntaxReceiver = context.SyntaxReceiver as ClassWithAttributeSyntaxReceiver;

            foreach (var settingsClass in syntaxReceiver.CandidateClasses)
            {
                var semanticModel = context.Compilation.GetSemanticModel(settingsClass.GetReference().SyntaxTree);
                var symbolModel = semanticModel.GetDeclaredSymbol(settingsClass) as ITypeSymbol;
                var attributes = settingsClass.AttributeLists
                    .Select(a => a.Attributes.FirstOrDefault(b => b.Name.ToFullString() == SettingsAttributeName)).ToList();

                var fileNames = new List<string>();
                foreach (var attribute in attributes)
                {
                    var fileName = attribute.ArgumentList.Arguments[0].ToString().Replace("\"", "");
                    fileNames.Add(fileName);
                }

                yield return new SettingsGroup
                {
                    FileList = fileNames,
                    ClassName = symbolModel.Name,
                    Namespace = symbolModel.ContainingNamespace.ToString()
                };
            }
        }

        private static JObject DeserializeToDictionary(string appsettingsJson)
        {
            return JsonSerializer.CreateDefault().Deserialize<JObject>(new JsonTextReader(new StringReader(appsettingsJson)));
        }
    }
}
