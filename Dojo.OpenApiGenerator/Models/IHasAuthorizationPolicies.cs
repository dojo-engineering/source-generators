using System.Collections.Generic;

namespace Dojo.OpenApiGenerator.Models
{
    internal interface IHasAuthorizationPolicies
    {
        public IEnumerable<string> AuthorizationPolicies { get; set; }
    }
}