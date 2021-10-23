using System;

namespace Dojo.Generators.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]

    public class AutoAppSettingsAttribute : Attribute
    {
        public AutoAppSettingsAttribute(string fileName = null)
        {
            FileName = fileName;
        }
        
        public string FileName { get; set; }

        public bool UseConventionalTypes { get; set; }

        public string[] ExcludedSections { get; set; } = new[] { "Logging" };
    }
}