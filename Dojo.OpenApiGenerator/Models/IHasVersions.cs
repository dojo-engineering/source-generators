using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    public interface IHasVersions
    {
        HashSet<string> SupportedVersions { get; set; }
    }
}
