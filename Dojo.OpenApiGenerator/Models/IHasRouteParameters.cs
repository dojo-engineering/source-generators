using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    internal interface IHasRouteParameters
    {
        IEnumerable<ApiRouteParameter> RouteParameters { get; set; }
        bool HasRouteParameters { get; }
    }
}
