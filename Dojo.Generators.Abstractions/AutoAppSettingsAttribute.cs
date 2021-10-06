using System;

namespace Dojo.Generators.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]

    public class AutoAppSettingsAttribute: Attribute
    {
        public string FileName { get; set; }
        public AutoAppSettingsAttribute(string fileName)
        {
            this.FileName = fileName;
        }
    }
}