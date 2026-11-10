using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// ReSharper disable ALL

namespace Roslyn
{
    /// <summary>
    ///     Formats the project and solution files of a directory tree to the line endings of the repository.
    /// </summary>
    /// <remarks>
    ///     The rewriter pipeline normalizes the line breaks of C# files as its final step, and it does so
    ///     in the syntax tree, which preserves the raw line breaks inside a multi-line verbatim or raw
    ///     string literal. Project and solution files are not C#, so they cannot pass through the syntax
    ///     pipeline; this pipeline normalizes them on the text level instead. The replacement preserves
    ///     the byte order mark and skips the generated build outputs.
    /// </remarks>
    internal static class Pipeline001_File
    {
        /// <summary>
        ///     Normalizes every project and solution file below the root directory to <c>\r\n</c>.
        /// </summary>
        /// <param name="rootDirectory">The directory below which the files are formatted.</param>
        /// <returns>The number of files whose content changed.</returns>
        public static int NormalizeLineEndings(string rootDirectory)
        {
            var changed = 0;
            foreach (var path in EnumerateFiles(rootDirectory))
            {
                var bytes = File.ReadAllBytes(path);
                var byteOrderMarkLength = HasByteOrderMark(bytes) ? 3 : 0;
                var text = new UTF8Encoding(false).GetString(bytes, byteOrderMarkLength, bytes.Length - byteOrderMarkLength);
                var normalized = text.Replace("\r\n", "\n").Replace("\n", "\r\n");
                if (normalized == text)
                    continue;

                File.WriteAllText(path, normalized, new UTF8Encoding(HasByteOrderMark(bytes)));
                ++changed;
            }

            return changed;
        }

        /// <summary>
        ///     Enumerates the project and solution files below the root directory.
        /// </summary>
        /// <param name="rootDirectory">The directory below which the files are enumerated.</param>
        /// <returns>The paths of the files, with the generated build outputs skipped.</returns>
        private static IEnumerable<string> EnumerateFiles(string rootDirectory)
        {
            foreach (var pattern in new[] { "*.csproj", "*.sln" })
            {
                foreach (var path in Directory.EnumerateFiles(rootDirectory, pattern, SearchOption.AllDirectories))
                {
                    var normalized = path.Replace('\\', '/');
                    if (normalized.Contains("/bin/", StringComparison.Ordinal) || normalized.Contains("/obj/", StringComparison.Ordinal))
                        continue;

                    yield return path;
                }
            }
        }

        /// <summary>
        ///     Determines whether the bytes begin with the UTF-8 byte order mark.
        /// </summary>
        /// <param name="bytes">The bytes of the file.</param>
        /// <returns>
        ///     <see langword="true" /> when the bytes begin with the byte order mark; otherwise, <see langword="false" />.
        /// </returns>
        private static bool HasByteOrderMark(byte[] bytes) => bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
    }
}