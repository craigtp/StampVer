using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace stampver
{
    internal sealed class IoWrapper : IIOWrapper
    {
        public IEnumerable<string> EnumerateFiles(string startDirectory, string fileToSearch)
        {
            // An empty start directory means "search from the current working directory",
            // preserving the original behaviour for callers that don't pass --dir.
            var searchRoot = string.IsNullOrEmpty(startDirectory)
                ? Directory.GetCurrentDirectory()
                : startDirectory;
            return Directory.EnumerateFiles(searchRoot, fileToSearch, SearchOption.AllDirectories);
        }

        public bool DirectoryExists(string directory)
        {
            // An empty directory denotes the current working directory, which always
            // exists — so only a non-empty path needs a real filesystem check.
            return string.IsNullOrEmpty(directory) || Directory.Exists(directory);
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

                // Preserve the original newline style and trailing-newline state too.
                // ReadAllLinesFromFile strips line endings, so re-joining with
                // File.WriteAllLines would force Environment.NewLine (rewriting an LF
                // file as CRLF on Windows) and always append a trailing newline. We
                // only touch one or two version lines, so the rest of the file — line
                // endings included — must come back out byte-for-byte.
                var (newline, hadTrailingNewline) = DetectNewlineStyle(file);
                var body = string.Join(newline, fileLines);
                if (hadTrailingNewline)
                {
                    body += newline;
                }
                File.WriteAllText(tempFileName, body, originalEncoding);

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

        public void WriteToStdErr(string output)
        {
            Console.Error.WriteLine(output);
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

        // Returns the file's newline style (first one encountered wins, matching the
        // "join everything with one style" approach) and whether the file ended with a
        // trailing newline. An empty file, or one with no newline at all, reports the
        // platform default and no trailing newline (so a no-newline file stays that way).
        private static (string Newline, bool HadTrailingNewline) DetectNewlineStyle(string file)
        {
            using var reader = new StreamReader(file);
            string? newline = null;
            var lastWasNewline = false;
            int ch;
            while ((ch = reader.Read()) != -1)
            {
                if (ch == '\r')
                {
                    // Decide pairing from the actual next char, not the stored style,
                    // so a bare '\r' in an otherwise CRLF file doesn't swallow the
                    // following character.
                    var pairedWithLf = reader.Peek() == '\n';
                    if (pairedWithLf)
                    {
                        reader.Read(); // consume the paired '\n'
                    }
                    newline ??= pairedWithLf ? "\r\n" : "\r";
                    lastWasNewline = true;
                }
                else if (ch == '\n')
                {
                    newline ??= "\n";
                    lastWasNewline = true;
                }
                else
                {
                    lastWasNewline = false;
                }
            }
            return (newline ?? Environment.NewLine, lastWasNewline);
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
