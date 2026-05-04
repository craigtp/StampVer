using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NDesk.Options;

namespace stampver
{
    public class Stampver
    {
        // Compiled once at first use and reused for every line of every file.
        // Hoisting this out of ProcessFileLine avoids re-parsing the pattern
        // on every iteration; RegexOptions.Compiled emits IL for faster matching.
        private static readonly Regex VersionRegex = new(
            @"Assembly(?:|File)Version\(""(?<version>\d{1,5}\.\d{1,5}\.(?:\d{1,5}|\*|)(?:\.|)(?:\d{1,5}|\*|))""\)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private readonly IIOWrapper _ioWrapper;
        private readonly string[] _programArgs;

        public Stampver(IIOWrapper ioWrapper, string[] programArgs)
        {
            _ioWrapper = ioWrapper;
            _programArgs = programArgs;
        }

        private const string DefaultFilePattern = "AssemblyInfo.cs";
        private const string CommentLineMarker = "//";

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

            var pattern = string.IsNullOrEmpty(versionArgs.FilePattern)
                ? DefaultFilePattern
                : versionArgs.FilePattern;

            var updatedVersionNumbers = ProcessFiles(pattern, versionArgs);

            if (versionArgs.OutputType == OutputType.NotSet)
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
                var extra = p.Parse(_programArgs);
                if (extra.Count > 1)
                {
                    // Surface dropped patterns on stderr so users notice when only
                    // the first one is honoured (e.g. "stampver -i patch *.cs *.vb").
                    _ioWrapper.WriteToStdErr($"warning: ignoring extra arguments after '{extra[0]}'.");
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
                _ioWrapper.WriteToStdErr($"error: {e.Message}{System.Environment.NewLine}Try 'stampver --help' for more information.");
                versionArgs = args;
                return false;
            }
        }

        private List<VersionUpdate> ProcessFiles(string pattern, VersionArgs versionArgs)
        {
            var updatedVersionNumbers = new List<VersionUpdate>();
            foreach (var file in _ioWrapper.EnumerateFiles(pattern))
            {
                ProcessSingleFile(file, versionArgs, updatedVersionNumbers);
            }
            return updatedVersionNumbers;
        }

        private void ProcessSingleFile(string file, VersionArgs versionArgs, List<VersionUpdate> updatedVersionNumbers)
        {
            LogIfVerbose($"Processing file: {file}", versionArgs);

            var fileLines = _ioWrapper.ReadAllLinesFromFile(file);
            var fileHasBeenModified = false;

            for (var i = 0; i < fileLines.Length; i++)
            {
                var result = ProcessFileLine(fileLines[i], i + 1, versionArgs);
                if (result.LineWasModified)
                {
                    fileHasBeenModified = true;
                    updatedVersionNumbers.Add(new VersionUpdate(result.NewVersionNumber, file));
                }
                fileLines[i] = result.Line;
            }

            if (versionArgs.IsDryrun || !fileHasBeenModified)
            {
                return;
            }

            _ioWrapper.WriteFileLinesToFile(fileLines, file);
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
                _ioWrapper.WriteToStdOut($"{result.VersionNumber} ({occurrences} in {files})");
            }
        }

        private static string Pluralize(int count, string singular, string plural)
            => $"{count} {(count == 1 ? singular : plural)}";

        private ProcessedLineResult ProcessFileLine(string fileLine, int fileLineNumber, VersionArgs versionArgs)
        {
            // Ignore comment lines.
            if (fileLine.Trim().StartsWith(CommentLineMarker))
            {
                return new ProcessedLineResult(fileLine, false, null);
            }
            var match = VersionRegex.Match(fileLine);
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
                _ioWrapper.WriteToStdOut(output);
            }
        }

        private void DisplayHelpText()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionString = $"{version.Major}.{version.Minor}.{version.Build}";
            var helpText = @"
stampver by Craig Phillips <craig@craigtp.co.uk>
================================================

A small command-line utility that will iterate through all of the
AssemblyInfo.cs files (or other specified files) below the current folder and
update the AssemblyVersion and AssemblyFileVersion attributes with a version
compliant with Semantic Versioning (See: http://semver.org/).
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
(rather than AssemblyInfo.cs) in order to try to make version changes.
Note that the way the utility matches within the file is exactly the same,
so file must still have a string matching [assembly: AssemblyVersion(""x.y.z"")]
or [assembly: AssemblyFileVersion(""x.y.z"")] within the file before version
number changes will be made.

This help text is always able to be displayed by passing --help to the program.

This is version: " + versionString;
            _ioWrapper.WriteToStdOut(helpText);
        }
    }
}
