using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Extensions
{
    public static class OpenApiSchemaExtensions
    {
        public static bool IsReferenceType(this OpenApiSchema schema)
        {
            return schema.Reference != null;
        }
    }
}
