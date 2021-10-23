using Newtonsoft.Json.Linq;

namespace Dojo.AutoGenerators.AppSettingsGenerator
{
    internal class PropertyDefinition
    {
        private string _name;

        public string Name 
        { 
            get => _name.Replace('.', '_');
            set => _name = value;
        }

        public string TypeName { get; set; }

        public JTokenType Type { get; set; }
    }
}
