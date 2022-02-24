using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    internal interface IHasRequestBody
    {
        ApiRequestBody RequestBody { get; }
        bool HasRequestBody { get; }
    }
}
