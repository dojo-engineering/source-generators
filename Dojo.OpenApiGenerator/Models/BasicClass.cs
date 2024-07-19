namespace Dojo.OpenApiGenerator.Models
{
    internal class BasicClass : BaseGeneratedCodeModel
    {
        public BasicClass(string projectNamespace, bool generateApiExplorer = false) : base(projectNamespace)
        {
            GenerateApiExplorer = generateApiExplorer;
        }

        public bool GenerateApiExplorer { get; private set; }
    }
}
