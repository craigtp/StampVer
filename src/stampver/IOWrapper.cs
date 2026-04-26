using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace stampver
{
    public class IoWrapper : IIOWrapper
    {
        public IEnumerable<string> EnumerateFiles(string fileToSearch)
        {
            return Directory.EnumerateFiles(Directory.GetCurrentDirectory(), fileToSearch, SearchOption.AllDirectories);
        }

        public string[] ReadAllLinesFromFile(string file)
        {
            return File.ReadAllLines(file);
        }

        public void WriteFileLinesToFile(IEnumerable<string> fileLines, string file)
        {
            // Place the temp file in the same directory as the target so that File.Replace
            // can swap it atomically (File.Replace requires same-volume operands).
            var directory = Path.GetDirectoryName(Path.GetFullPath(file));
            if (string.IsNullOrEmpty(directory))
            {
                directory = ".";
            }
            var tempFileName = Path.Combine(directory, $".{Path.GetFileName(file)}.{Guid.NewGuid():N}.tmp");

            try
            {
                // Preserve the original encoding (including any BOM) so we don't silently
                // change the file's byte preamble on first write.
                var originalEncoding = DetectEncoding(file);
                File.WriteAllLines(tempFileName, fileLines, originalEncoding);

                if (File.Exists(file))
                {
                    File.Replace(tempFileName, file, destinationBackupFileName: null);
                }
                else
                {
                    // The target was deleted between read and write; fall back to a plain move.
                    File.Move(tempFileName, file);
                }
            }
            catch
            {
                TryDelete(tempFileName);
                throw;
            }
        }

        public void WriteToStdOut(string output)
        {
            Console.WriteLine(output);
        }

        // Uses StreamReader's built-in BOM detection. CurrentEncoding only reflects the BOM
        // after the first read, so we Peek() once before reading it. Falls back to UTF-8
        // without BOM when no recognised BOM is present.
        private static Encoding DetectEncoding(string file)
        {
            var fallback = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
            using var reader = new StreamReader(file, fallback, detectEncodingFromByteOrderMarks: true);
            _ = reader.Peek();
            return reader.CurrentEncoding;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best-effort cleanup. Swallowing here is intentional: we're already in a
                // failure path and don't want to mask the original exception.
            }
        }
    }
}
