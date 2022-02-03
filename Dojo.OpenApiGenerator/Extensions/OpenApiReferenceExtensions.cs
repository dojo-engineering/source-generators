using System.Linq;
using Microsoft.OpenApi.Models;

namespace Dojo.OpenApiGenerator.Extensions
{
    internal static class OpenApiReferenceExtensions
    {
        public static string GetModelName(this OpenApiReference openApiReference)
        {
            return openApiReference.ReferenceV3.Split('/').Last();
        }

        public static string GetApiModelReference(this OpenApiReference openApiReference, string localApiFileName)
        {
            return openApiReference.IsExternal ?
                openApiReference.GetModelName().GetApiModelKey(openApiReference.ExternalResource.Replace("./", string.Empty)) :
                openApiReference.GetModelName().GetApiModelKey(localApiFileName);
        }
    }
}
