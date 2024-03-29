﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Dojo.AutoGenerators")]
[assembly: InternalsVisibleTo("Dojo.OpenApiGenerator")]
[assembly: InternalsVisibleTo("Dojo.Generators.Tests")]
namespace Dojo.Generators.Core.Utils
{
    internal class FileSystemUtils
    {
        internal static string[] FindFiles(string folder, string name)
        {
            var extension = Path.GetExtension(name);
            var files = FindFilesWithExtension(folder, extension);

            return files
                .Where(x => x.EndsWith(name, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        internal static string[] FindFilesWithExtension(string folder, string extension)
        {
            // Since on .netstandard2.0 EnumerationOptions cannot be used
            var files = Directory.GetFiles(
                folder,
                $"*{extension}",
                SearchOption.AllDirectories);

            return files;
        }

        internal static IEnumerable<string> FindFilesWithExtensions(string folder, params string[] extensions)
        {
            // Since on .netstandard2.0 EnumerationOptions cannot be used
            var files = extensions.SelectMany(extension => Directory.GetFiles(
                folder,
                $"*{extension}",
                SearchOption.AllDirectories));

            return files;
        }
    }
}
