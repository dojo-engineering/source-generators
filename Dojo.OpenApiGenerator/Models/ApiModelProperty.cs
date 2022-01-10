using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Models
{
    internal class ApiModelProperty : ApiModelBase
    {
        public static ApiModelProperty Create(string name, OpenApiSchema openApiSchema)
        {
            return new ApiModelProperty
            {
                Name = name,
                Type = GetBuiltInType(openApiSchema.Type, openApiSchema.Format)
            };
        }

        protected override string GetTypeFullName()
        {
            return Type.FullName;
        }
    }
}
