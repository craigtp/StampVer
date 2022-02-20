using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NDesk.Options;

namespace stampver
{
    public class Stampver
    {
        private readonly IIOWrapper _ioWrapper;
        private readonly string[] _programArgs;

        public Stampver(IIOWrapper ioWrapper, string[] programArgs)
        {
            _ioWrapper = ioWrapper;
            _programArgs = programArgs;
        }

        public void Run()
        {
            var versionArgs = new VersionArgs();

            var p = new OptionSet()
            {
                {"i=", "command to increment the version number", v => versionArgs.SetIncrement(v) },
                {"d=", "command to decrement the version number", v => versionArgs.SetDecrement(v) },
                {"e=", "command to explicitly set the complete version number", v => versionArgs.SetExplicit(v) },
                {"quiet", "do not output anything to the console", v => versionArgs.SetQuiet() },
                {"verbose", "output verbose information to the console", v => versionArgs.SetVerbose() },
                {"dryrun", "perform a dry run and don't update any files", v => versionArgs.SetDryrun() },
                {"help", "command to increment the version number", v => versionArgs.SetDisplayHelp() }
            };

            try
            {
                var extra = p.Parse(_programArgs);
                if (extra.Count > 0)
                {
                    versionArgs.SetFilePattern(extra.First());
                }
                versionArgs.ValidateArgs();
            }
            catch (OptionException e)
            {
                _ioWrapper.WriteToStdOut("error: ");
                _ioWrapper.WriteToStdOut(e.Message);
                _ioWrapper.WriteToStdOut("Try 'stampver --help' for more information.");
                return;
            }

            if (versionArgs.DisplayHelp)
            {
                DisplayHelpText();
                return;
            }

            var fileToSearch = "AssemblyInfo.cs";
            if (!string.IsNullOrEmpty(versionArgs.FilePattern))
            {
                fileToSearch = versionArgs.FilePattern;
            }

            var updatedVersionNumbers = new List<Tuple<string, string>>();
            foreach (var file in _ioWrapper.EnumerateFiles(fileToSearch))
            {
                LogIfVerbose($"Processing file: {file}", versionArgs);

                var fileLines = _ioWrapper.ReadAllLinesFromFile(file);
                var fileHasBeenModified = false;

                for (var i = 0; i < fileLines.Length; i++)
                {
                    var result = ProcessFileLine(fileLines[i], i+1, versionArgs);
                    if (result.LineWasModified)
                    {
                        fileHasBeenModified = true;
                        updatedVersionNumbers.Add(new Tuple<string, string>(result.NewVersionNumber, file));
                    }
                    fileLines[i] = result.Line;
                }

                if (versionArgs.IsDryrun || !fileHasBeenModified)
                {
                    continue;
                }

                _ioWrapper.WriteFileLinesToFile(fileLines, file);
            }
            if (versionArgs.OutputType == OutputType.NotSet)
            {
                // We're neither in quiet mode nor verbose mode, so output all new
                // version numbers generated along with the occurence count and file count.
                // i.e.
                // v0.3.0 (2 occurrences in 1 file)
                // v1.0.1 (4 occurrences in 2 files)
                // v1.1.0 (1 occurence in 1 file)
                var results = updatedVersionNumbers.GroupBy(v => v)
                    .Select(v => new { VersionNumber = v.Key.Item1, FileName = v.Key.Item2, CountVers = v.Count() })
                    .GroupBy(v => v.VersionNumber)
                    .Select(v => new { VersionNumber = v.Key, FileCount = v.Count(), OccurenceCount = v.Sum(f => f.CountVers) });

                foreach (var result in results)
                {
                    // We could use string interpolation here but it looks messy.  string.Format is much more readable.
                    // ReSharper disable once UseStringInterpolation
                    _ioWrapper.WriteToStdOut(string.Format("{0} ({1} {2} in {3} {4})",
                            result.VersionNumber,
                            result.OccurenceCount, 
                            result.OccurenceCount > 1 ? "occurrences" : "occurence",
                            result.FileCount,
                            result.FileCount > 1 ? "files" : "file"));
                }
            }
        }

        private ProcessedLineResult ProcessFileLine(string fileLine, int fileLineNumber, VersionArgs versionArgs)
        {
            var regex = new Regex(@"Assembly(?:|File)Version\(""(?<version>\d{1,5}\.\d{1,5}\.(?:\d{1,5}|\*|)(?:\.|)(?:\d{1,5}|\*|))""\)");

            // Ignore comment lines.
            if (fileLine.Trim().StartsWith(@"//"))
            {
                return new ProcessedLineResult(fileLine, false, null);
            }
            var match = regex.Match(fileLine);
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
