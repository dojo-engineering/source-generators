using Dojo.AutoGenerators.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dojo.AutoGenerators.AppSettingsGenerator
{
    /// <summary>TODO: Replace with T4 (or similar) when gets too complicated.</summary>
    internal class TemplateEngine
    {
        public string Expand(string templateName, Dictionary<string, string> parameterValues)
        {
            string template = ReadTemplateContent(templateName);
            foreach (var kvp in parameterValues)
            {
                string macro = $"{{{kvp.Key}}}";
                int indention = CalculateIndention(template, macro);

                string value = kvp.Value;
                if (indention > 0)
                {
                    string[] lines = value.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                    if (lines.Length > 1)
                    {
                        lines = lines.Select((x, i) => $"{string.Empty.PadLeft(i > 0 ? indention : 0)}{x}".TrimEnd()).ToArray();
                        value = string.Join(Environment.NewLine, lines);
                    }
                }

                string newTemplate = template.Replace(macro, value);

                if (newTemplate == template)
                {
                    throw new InvalidOperationException($"Template content is the same after expanding the following parameter: {kvp.Key}");
                }
                template = newTemplate;
            }

            return template;
        }

        private string ReadTemplateContent(string templateName)
        {
            string resourceName = $"Dojo.AutoGenerators.AppSettingsGenerator.Templates.{templateName}";
            return AssemblyUtils.ReadEmbeddedResource(resourceName);
        }

        private int CalculateIndention(string template, string macro)
        {
            int indention = template.IndexOf(macro);

            if (indention <= 0) return indention;

            int start = indention;
            do
            {
                start--;

            }
            while (start > 0 && template[start] == ' ');

            return indention - start - 1;
        }
    }
}