using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    internal interface IHasRouteParameters
    {
        IList<ApiRouteParameter> RouteParameters { get; }
        bool HasRouteParameters { get; }
    }
}
