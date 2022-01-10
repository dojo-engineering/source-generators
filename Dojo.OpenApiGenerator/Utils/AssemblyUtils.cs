using System;
using System.IO;
using System.Reflection;

namespace Dojo.OpenApiGenerator.Utils
{
    public static class AssemblyUtils
    {
        public static string ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetCallingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                throw new InvalidOperationException($"Embedded resource not found '{resourceName}'! " +
                    $"Make sure '{resourceName}' marked as embedded resource.");
            }

            using var reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static string AssemblyVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
