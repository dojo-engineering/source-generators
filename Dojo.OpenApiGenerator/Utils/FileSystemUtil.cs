using System;
using System.IO;
using System.Linq;

namespace Dojo.OpenApiGenerator.Utils
{
    internal class FileSystemUtil
    {
        internal static string FindFile(string folder, string name)
        {
            var extension = Path.GetExtension(name);

            // Since on .netstandard2.0 EnumerationOptions cannot be used
            var files = FindFilesWithExtension(folder, extension);

            return files
                .First(x => x.EndsWith(name, StringComparison.OrdinalIgnoreCase));
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
    }
}
