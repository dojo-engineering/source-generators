using System.IO;
using Newtonsoft.Json;

namespace Dojo.OpenApiGenerator.Configuration
{
    internal class AutoApiGeneratorSettings
    {
        public string VersionParameterName { get; set; }
        
        public bool IncludeVersionParameterInActionSignature { get; set; }
        
        public string OpenApiSupportedVersionsExtension { get; set; }
        
        public string DateTimeVersionFormat { get; set; }
        
        public string ApiAuthorizationPoliciesExtension { get; set; }

        public bool GenerateApiExplorer { get; set; } = true;

        public static AutoApiGeneratorSettings GetAutoApiGeneratorSettings(string projectDir)
        {
            var settingFilePath = $"{projectDir}/{Constants.AutoApiGeneratorSettingsFile}";
            var settingFile = new FileInfo(settingFilePath);

            if (!settingFile.Exists)
            {
                return new AutoApiGeneratorSettings();
            }

            var settings = File.ReadAllText(settingFilePath);
           
            return JsonConvert.DeserializeObject<AutoApiGeneratorSettings>(settings);
        }
    }
}