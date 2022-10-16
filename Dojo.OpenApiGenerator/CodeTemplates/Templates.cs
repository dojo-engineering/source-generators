using Dojo.Generators.Core.Utils;

namespace Dojo.OpenApiGenerator.CodeTemplates
{
    internal static class Templates
    {
        public static string Model => "ModelTemplate.mustache";
        public static string ServiceInterface => "ServiceInterfaceTemplate.mustache";
        public static string AbstractController => "AbstractControllerTemplate.mustache";
        public static string Enum => "EnumTemplate.mustache";
        public static string ApiConstantsTemplate => "ApiConstantsTemplate.mustache";
        public static string InheritedApiVersionAttribute => "InheritedApiVersionAttribute.mustache";
        public static string ApiConfiguratorTemplate => "ApiConfiguratorTemplate.mustache";

        public static string ReadTemplate(string templateName)
        {
            var resourceName = $"Dojo.OpenApiGenerator.CodeTemplates.{templateName}";

            return AssemblyUtils.ReadEmbeddedResource(resourceName);
        }
    }
}
