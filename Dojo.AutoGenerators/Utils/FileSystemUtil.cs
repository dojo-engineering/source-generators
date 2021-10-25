using System;
using System.IO;
using System.Linq;

namespace Dojo.AutoGenerators.Utils
{
    internal class FileSystemUtil
    {
        internal static string[] FindFiles(string folder, string name)
        {
            string extension = Path.GetExtension(name);

            // Since on .netstandard2.0 EnumerationOptions cannot be used
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
    }
}
