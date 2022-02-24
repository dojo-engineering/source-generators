using System.IO;
using System.Linq;
using System.Xml.XPath;
using Dojo.Generators.Core.Utils;
using Microsoft.CodeAnalysis;

namespace Dojo.OpenApiGenerator.Extensions
{
    internal static class GeneratorExecutionContextExtender
    {
        public static string GetProjectDir(this GeneratorExecutionContext context)
        {
            context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir", out var projectDir);

            return projectDir;
        }

        public static string GetProjectDefaultNamespace(this GeneratorExecutionContext context)
        {
            var projectDir = context.GetProjectDir();
            var project = FileSystemUtils.FindFilesWithExtension(projectDir, ".csproj").SingleOrDefault();

            if (project == null)
            {
                return "GeneratedNamespace";
            }

            using var fileStream = File.Open(project, FileMode.Open);

            var xPath = new XPathDocument(fileStream);
            var navigator = xPath.CreateNavigator();
            var explicitNamespace = navigator.SelectSingleNode("Project/PropertyGroup/RootNamespace");
            var result = explicitNamespace != null ? explicitNamespace.Value : Path.GetFileNameWithoutExtension(project);

            return $"{result}";
        }
    }
}
