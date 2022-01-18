using System;

namespace Dojo.Generators.Abstractions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoControllerOverride : Attribute
    {
        public string ApiName { get; set; }

        public AutoControllerOverride(string apiName)
        {
            ApiName = apiName;
        }
    }
}
