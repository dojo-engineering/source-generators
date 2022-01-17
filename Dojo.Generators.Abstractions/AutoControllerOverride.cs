using System;

namespace Dojo.Generators.Abstractions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoControllerOverride : Attribute
    {
        private readonly string _route;
        public string Route { get; set; }

        public AutoControllerOverride(string route)
        {
            _route = route;
        }
    }
}
