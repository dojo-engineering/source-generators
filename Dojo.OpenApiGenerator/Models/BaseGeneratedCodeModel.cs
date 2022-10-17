using Dojo.Generators.Core.Utils;

namespace Dojo.OpenApiGenerator.Models
{
    internal abstract class BaseGeneratedCodeModel
    {
        protected BaseGeneratedCodeModel(string projectNamespace)
        {
            ProjectNamespace = projectNamespace;
        }

        public string ProjectNamespace { get; set; }
        public string AssemblyVersion => AssemblyUtils.AssemblyVersion;
    }
}