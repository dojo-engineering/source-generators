using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace Dojo.AutoGenerators
{
    internal static class GeneratorExecutionContextExtender
    {
        public static bool TryGetProjectDir(this GeneratorExecutionContext context, out string projectDir)
        {
            return context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir", out projectDir);
        }

        public static void AddSources(this GeneratorExecutionContext context, Dictionary<string, string> sourceFiles)
        {
            foreach (var kvp in sourceFiles)
            {
                context.AddSource(kvp.Key, SourceText.From(kvp.Value, Encoding.UTF8));
            }
        }
    }
}
