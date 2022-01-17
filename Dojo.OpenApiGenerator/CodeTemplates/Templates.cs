using Dojo.OpenApiGenerator.Utils;

namespace Dojo.OpenApiGenerator.CodeTemplates
{
    internal static class Templates
    {
        public static string Controller => "ControllerTemplate.mustache";
        public static string Model => "ModelTemplate.mustache";
        public static string ServiceInterface => "ServiceInterfaceTemplate.mustache";

        public static string ReadTemplate(string templateName)
        {
            var resourceName = $"Dojo.OpenApiGenerator.CodeTemplates.{templateName}";

            return AssemblyUtils.ReadEmbeddedResource(resourceName);
        }
    }
}
