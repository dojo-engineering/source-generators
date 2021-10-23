using System.Collections.Generic;

namespace Dojo.AutoGenerators.AppSettingsGenerator
{
    internal class ClassDefinition
    {
        public string Name { get; set; }

        public List<PropertyDefinition> Properties { get; set; } = new();
    }
}
