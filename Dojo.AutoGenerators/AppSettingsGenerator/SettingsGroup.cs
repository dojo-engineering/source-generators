namespace Dojo.AutoGenerators.AppSettingsGenerator
{
    internal record SettingsGroup
    {
        public string FileName { get; set; }

        public string ClassName { get; set; }

        public string Namespace { get; set; }

        public bool GenerateConventionalTypes { get; set; } = false;

        public string[] ExcludedSections { get; set; } = new[] { "Logging" };

        public bool IsExplicitAttributeSpecified { get; set; } = false;
    }
}
