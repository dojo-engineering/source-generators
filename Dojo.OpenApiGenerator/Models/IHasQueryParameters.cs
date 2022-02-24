using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    internal interface IHasQueryParameters
    {
        IList<ApiQueryParameter> QueryParameters { get; }
        bool HasQueryParameters { get; }
    }
}
