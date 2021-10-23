using Dojo.Generators.Abstractions;

namespace ClassLibrary1.Test
{
    public static class St
    {
        public const string Const = "lol";
    }

    [AutoAppSettings(ExcludedSections = new[] { "Lol" })]
    public partial class AppSettings
    {
    }
}