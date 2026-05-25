using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using stampver.Options;

namespace stampver
{
    internal sealed class Stampver(IIOWrapper ioWrapper, string[] programArgs)
    {
        // Bundles the version-matching regex, comment marker, and default
        // file-name pattern for one supported source-file form. The set of
        // formats is closed (picked at runtime by file extension); there is
        // no extensibility hook.
        private sealed class FileFormat
        {
            public required Regex VersionPattern { get; init; }
            public required string CommentMarker { get; init; }
            public required string DefaultFilePattern { get; init; }
        }

        // Legacy attribute form: [assembly: AssemblyVersion("x.y.z")] and
        // [assembly: AssemblyFileVersion("x.y.z")]. Wildcard "*" tokens are
        // accepted in the patch/revision positions and preserved verbatim.
        private static readonly FileFormat AssemblyInfoFormat = new()
        {
            VersionPattern = new Regex(
                @"Assembly(?:|File)Version\(""(?<version>\d{1,5}\.\d{1,5}\.(?:\d{1,5}|\*|)(?:\.|)(?:\d{1,5}|\*|))""\)",
                RegexOptions.Compiled | RegexOptions.CultureInvariant),
            CommentMarker = "//",
            DefaultFilePattern = "AssemblyInfo.cs",
        };

        // SDK-style csproj MSBuild properties: <AssemblyVersion>, <FileVersion>,
        // <Version>, <VersionPrefix>. The optional `(?:\s[^>]*)?` allows
        // arbitrary attributes on the element (e.g. Condition="..."). Wildcards
        // are not supported in csproj versions, so only digits are accepted.
        private static readonly FileFormat CsprojFormat = new()
        {
            VersionPattern = new Regex(
                @"<(?<el>AssemblyVersion|FileVersion|Version|VersionPrefix)(?:\s[^>]*)?>(?<version>\d{1,5}\.\d{1,5}\.\d{1,5}(?:\.\d{1,5})?)</\k<el>>",
                RegexOptions.Compiled | RegexOptions.CultureInvariant),
            CommentMarker = "<!--",
            DefaultFilePattern = "*.csproj",
        };

        // Scanned in turn when no positional file-pattern argument is given.
        // Existing AssemblyInfo.cs users keep working unchanged; modern
        // SDK-style csprojs are picked up automatically with no flag.
        private static readonly FileFormat[] DefaultFormats = [AssemblyInfoFormat, CsprojFormat];

        private static FileFormat FormatForFile(string filePath) =>
            filePath.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
                ? CsprojFormat
                : AssemblyInfoFormat;

        // One entry is added per modified line. Multiple entries with the same
        // (VersionNumber, FileName) pair are expected — e.g. when a file has
        // both AssemblyVersion and AssemblyFileVersion attributes.
        private readonly record struct VersionUpdate(string VersionNumber, string FileName);

        public int Run()
        {
            if (!TryParseArguments(out var versionArgs))
            {
                return ExitCodes.UsageError;
            }

            if (versionArgs.DisplayHelp)
            {
                DisplayHelpText();
                return ExitCodes.Success;
            }

            var updatedVersionNumbers = ProcessFiles(versionArgs);

            if (versionArgs.OutputType == OutputType.Normal)
            {
                WriteSummary(updatedVersionNumbers);
            }

            return ExitCodes.Success;
        }

        private bool TryParseArguments(out VersionArgs versionArgs)
        {
            // The OptionSet lambdas need to close over a real local — out parameters
            // can't be captured by anonymous methods. We assign back to versionArgs
            // before each return path.
            var args = new VersionArgs();

            var p = new OptionSet
            {
                {"i=", "command to increment the version number", v => args.SetIncrement(v) },
                {"d=", "command to decrement the version number", v => args.SetDecrement(v) },
                {"e=", "command to explicitly set the complete version number", v => args.SetExplicit(v) },
                {"quiet", "do not output anything to the console", _ => args.SetQuiet() },
                {"verbose", "output verbose information to the console", _ => args.SetVerbose() },
                {"dryrun", "perform a dry run and don't update any files", _ => args.SetDryrun() },
                {"help", "command to increment the version number", _ => args.SetDisplayHelp() }
            };

            try
            {
                var extra = p.Parse(programArgs);
                if (extra.Count > 1)
                {
                    // Surface dropped patterns on stderr so users notice when only
                    // the first one is honoured (e.g. "stampver -i patch *.cs *.vb").
                    ioWrapper.WriteToStdErr($"warning: ignoring extra arguments after '{extra[0]}'.");
                }
                if (extra.Count > 0)
                {
                    args.SetFilePattern(extra[0]);
                }
                args.ValidateArgs();
                versionArgs = args;
                return true;
            }
            catch (OptionException e)
            {
                // Errors go to stderr (not stdout) so callers can pipe stdout cleanly,
                // and so that --quiet doesn't suppress error visibility. The combined
                // single message replaces three separate WriteToStdOut calls that
                // previously fragmented the diagnostic across multiple lines.
                ioWrapper.WriteToStdErr($"error: {e.Message}{System.Environment.NewLine}Try 'stampver --help' for more information.");
                versionArgs = args;
                return false;
            }
        }

        private List<VersionUpdate> ProcessFiles(VersionArgs versionArgs)
        {
            // Explicit positional pattern wins outright; otherwise scan every
            // default format's pattern in turn. The fake-IO test wrapper
            // dispatches by pattern, so unrelated patterns don't double-count.
            var patterns = string.IsNullOrEmpty(versionArgs.FilePattern)
                ? DefaultFormats.Select(f => f.DefaultFilePattern).ToArray()
                : [versionArgs.FilePattern];

            var updatedVersionNumbers = new List<VersionUpdate>();
            foreach (var pattern in patterns)
            {
                foreach (var file in ioWrapper.EnumerateFiles(pattern))
                {
                    ProcessSingleFile(file, versionArgs, updatedVersionNumbers);
                }
            }
            return updatedVersionNumbers;
        }

        private void ProcessSingleFile(string file, VersionArgs versionArgs, List<VersionUpdate> updatedVersionNumbers)
        {
            LogIfVerbose($"Processing file: {file}", versionArgs);

            var format = FormatForFile(file);
            var fileLines = ioWrapper.ReadAllLinesFromFile(file);
            var fileHasBeenModified = false;

            for (var i = 0; i < fileLines.Length; i++)
            {
                var result = ProcessFileLine(fileLines[i], i + 1, format, versionArgs);
                if (result.LineWasModified)
                {
                    fileHasBeenModified = true;
                    // ProcessFileLine guarantees NewVersionNumber is non-null whenever
                    // LineWasModified is true (see the modified-line return path).
                    updatedVersionNumbers.Add(new VersionUpdate(result.NewVersionNumber!, file));
                }
                fileLines[i] = result.Line;
            }

            if (versionArgs.IsDryrun || !fileHasBeenModified)
            {
                return;
            }

            ioWrapper.WriteFileLinesToFile(fileLines, file);
        }

        private void WriteSummary(List<VersionUpdate> updatedVersionNumbers)
        {
            // We're neither in quiet mode nor verbose mode, so output all new
            // version numbers generated along with the occurrence count and file count.
            // i.e.
            // v0.3.0 (2 occurrences in 1 file)
            // v1.0.1 (4 occurrences in 2 files)
            // v1.1.0 (1 occurrence in 1 file)
            // For each new version, FileCount is the number of distinct files it
            // landed in, and OccurrenceCount is the total number of attribute
            // matches replaced (a single file can contribute >1 occurrence).
            var results = updatedVersionNumbers
                .GroupBy(u => u.VersionNumber)
                .Select(g => new
                {
                    VersionNumber = g.Key,
                    FileCount = g.Select(u => u.FileName).Distinct().Count(),
                    OccurrenceCount = g.Count()
                });

            foreach (var result in results)
            {
                var occurrences = Pluralize(result.OccurrenceCount, "occurrence", "occurrences");
                var files = Pluralize(result.FileCount, "file", "files");
                ioWrapper.WriteToStdOut($"{result.VersionNumber} ({occurrences} in {files})");
            }
        }

        private static string Pluralize(int count, string singular, string plural)
            => $"{count} {(count == 1 ? singular : plural)}";

        private ProcessedLineResult ProcessFileLine(string fileLine, int fileLineNumber, FileFormat format, VersionArgs versionArgs)
        {
            // Skip lines whose first non-whitespace character begins a comment
            // for this file format ("//" for C#, "<!--" for XML). Multi-line
            // XML block comments containing a version element would still be
            // matched — in practice nobody comments out version elements.
            if (fileLine.Trim().StartsWith(format.CommentMarker, StringComparison.Ordinal))
            {
                return new ProcessedLineResult(fileLine, false, null);
            }
            var match = format.VersionPattern.Match(fileLine);
            if (!match.Success) return new ProcessedLineResult(fileLine, false, null);

            string replacedVersionNumber;
            var originalVersionNumber = match.Groups["version"].Value;
            if (versionArgs.VersionNumberCommand == VersionNumberCommand.ExplicitSet)
            {
                replacedVersionNumber = versionArgs.ExplicitVersionNumber;
            }
            else
            {
                var originalAssemblyVersion = new AssemblyVersion(originalVersionNumber);
                if (versionArgs.VersionNumberCommand == VersionNumberCommand.Increment)
                {
                    originalAssemblyVersion.Increment(versionArgs.VersionNumberPart);
                }
                else
                {
                    originalAssemblyVersion.Decrement(versionArgs.VersionNumberPart);
                }
                replacedVersionNumber = originalAssemblyVersion.GetVersionString();
            }
            var newFileLine = fileLine.Replace(originalVersionNumber, replacedVersionNumber);
            var prefix = versionArgs.IsDryrun ? "Would Change" : "Changed";
            LogIfVerbose($"{prefix} (Line {fileLineNumber}): {fileLine} to {newFileLine}", versionArgs);
            return new ProcessedLineResult(newFileLine, true, replacedVersionNumber);
        }

        private void LogIfVerbose(string output, VersionArgs versionArgs)
        {
            if (versionArgs.OutputType == OutputType.Verbose)
            {
                ioWrapper.WriteToStdOut(output);
            }
        }

        private void DisplayHelpText()
        {
            // <AssemblyVersion> is set in the csproj, so Version is never null in
            // practice — but the BCL contract is nullable, so guard defensively.
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionString = version is null ? "unknown" : $"{version.Major}.{version.Minor}.{version.Build}";
            var helpText = @"
stampver by Craig Phillips <craig@craigtp.co.uk>
================================================

A small command-line utility that updates version numbers in .NET source
files below the current folder. By default it scans both legacy
AssemblyInfo.cs files (updating [assembly: AssemblyVersion] and
[assembly: AssemblyFileVersion] attributes) and modern SDK-style *.csproj
files (updating <AssemblyVersion>, <FileVersion>, <Version>, and
<VersionPrefix> MSBuild properties). Versions are kept compliant with
Semantic Versioning (See: http://semver.org/).
The utility can automatically increment or decrement specific parts of the
version number or can explicitly set the entire version string.

Usage
-----
stampver.exe [command] [version part or specific version number]
             [(optional) filepattern]

where:

[command] is:
-i           = Increment the specified version number part by 1.
-d           = Decrement the specified version number part by 1.
-e           = Replace the entire version number string with the specified
               version number

[version part or specific version number] is:
MAJOR        = Perform increment or decrement on the Major version number part.
MINOR        = Perform increment or decrement on the Minor version number part.
PATCH        = Perform increment or decrement on the Patch version number part.
BUILD        = Synonym for PATCH. Perform increment or decrement on the Patch
               version number part.
x.y.z        = A specific version number where x, y  and z are integer numbers
               in the range 0 to 65535, separated by a period.

Note that the specific version number parameter value (x.y.z) is only usable
with the -e command, and the MAJOR, MINOR and PATCH/BUILD parameter values are
only usable with the -i or -d commands.  Attempting to use commands and version
parameters that are incompatible will cause the program to display an error.

Additional commands that can be specified are as follows:
--quiet      = Don't write out anything to the console.
--verbose    = Display full logging information of the files and changes made
               to the console.
--dryrun     = Don't actually make any file changes.

Note that --quiet and --verbose parameters are mutually exclusive and that
specifying the --dryrun parameter automatically enables verbose output.

[filepattern] is:
Any valid file pattern that can be passed to the .NET Directory.EnumerateFiles
method. See here for details:
https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles
Specifying a filepattern will search for files matching the file pattern
(rather than the default AssemblyInfo.cs + *.csproj scan) in order to try
to make version changes. The file format is detected from the extension
(.csproj uses the MSBuild element matcher, everything else uses the C#
attribute matcher), so a file must still contain a matching
[assembly: AssemblyVersion(""x.y.z"")] /
[assembly: AssemblyFileVersion(""x.y.z"")] attribute, or a matching
<AssemblyVersion>x.y.z</AssemblyVersion>, <FileVersion>x.y.z</FileVersion>,
<Version>x.y.z</Version>, or <VersionPrefix>x.y.z</VersionPrefix> element
before any changes will be made.

This help text is always able to be displayed by passing --help to the program.

This is version: " + versionString;
            ioWrapper.WriteToStdOut(helpText);
        }
    }
}
