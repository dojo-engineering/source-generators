using System;

namespace Dojo.Generators.Abstractions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AutoInterfaceAttribute : Attribute
    {
        public AutoInterfaceLifetime Lifetime { get; set; } = AutoInterfaceLifetime.Scoped;
    }
}