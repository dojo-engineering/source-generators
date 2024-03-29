﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Dojo.AutoGenerators")]
[assembly: InternalsVisibleTo("Dojo.OpenApiGenerator")]
namespace Dojo.Generators.Core.Utils
{
    internal static class AssemblyUtils
    {
        public static string ReadEmbeddedResource(string resourceName)
        {
            Assembly assembly = Assembly.GetCallingAssembly();

            using Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
            {
                throw new InvalidOperationException($"Embedded resource not found '{resourceName}'! " +
                    $"Make sure '{resourceName}' marked as embedded resource.");
            }

            using StreamReader reader = new StreamReader(stream);

            return reader.ReadToEnd();
        }

        public static string AssemblyVersion { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
