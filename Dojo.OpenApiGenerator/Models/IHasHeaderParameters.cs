using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    internal interface IHasHeaderParameters
    {
        IList<ApiHeaderParameter> HeaderParameters { get; }
        bool HasHeaderParameters { get; }
    }
}
