using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Dojo.AutoGenerators.AppSettingsGenerator
{
    internal class ParsedSettings
    {
        internal Dictionary<string, Tuple<string, JTokenType>> Properties { get; set; }
        
        internal Dictionary<string, ClassDefinition> Classes { get; set; }
    }

    internal class SettingsJsonParser
    {
        private readonly string _content;
        private readonly HashSet<string> _excludedSections;
        private readonly bool _generateConventionalTypes;
        private Dictionary<string, ClassDefinition> _classes = new();

        internal SettingsJsonParser(string content, bool generateConventionalTypes, string[] excludedSections)
        {
            _content = content;
            _excludedSections = new HashSet<string>(excludedSections);
            _generateConventionalTypes = generateConventionalTypes;
        }

        public ParsedSettings Parse()
        {
            Dictionary<string, Tuple<string, JTokenType>> properties = new();
            var appsettings = DeserializeToDictionary(_content);

            foreach (var child in appsettings.Children<JToken>())
            {
                if (child is JProperty prop)
                {
                    switch (prop.Value.Type)
                    {
                        case JTokenType.Object:
                            if (_excludedSections.Contains(prop.Name)) continue;

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

            return new ParsedSettings { Properties = properties, Classes = _classes };
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
                            TypeName = JTokenTypeToNetType(prop.Name, prop.Value.Type)
                        });
                    }
                }
            }

            return classDefinition;
        }

        private string JTokenTypeToNetType(string name, JTokenType? tokenType)
        {
            if (_generateConventionalTypes)
            {
                string conventionalType = ConventionalTypeMapper.GetTypeFromName(name);
                if (!string.IsNullOrEmpty(conventionalType))
                {
                    return conventionalType;
                }
            }

            return JTokenTypeToNetType(tokenType);
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


        private static JObject DeserializeToDictionary(string appsettingsJson)
        {
            return JsonSerializer.CreateDefault().Deserialize<JObject>(new JsonTextReader(new StringReader(appsettingsJson)));
        }
    }
}
