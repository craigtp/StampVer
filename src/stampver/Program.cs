using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NDesk.Options;

namespace stampver
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var versionArgs = new VersionArgs();

            var p = new OptionSet()
            {
                {"i=", "command to increment the version number", v => versionArgs.SetIncrement(v) },
                {"d=", "command to decrement the version number", v => versionArgs.SetDecrement(v) },
                {"e=", "command to explicly set the complete version number", v => versionArgs.SetExplicit(v) },
                {"quiet", "do not output anything to the console", v => versionArgs.SetQuiet() },
                {"verbose", "output verbose information to the console", v => versionArgs.SetVerbose() },
                {"dryrun", "perform a dryrun and don't update any files", v => versionArgs.SetDryrun() },
                {"help", "command to increment the version number", v => versionArgs.SetDisplayHelp() }
            };

            try
            {
                var extra = p.Parse(args);
                if (extra.Count > 0)
                {
                    versionArgs.SetFilePattern(extra.First());
                }
                versionArgs.ValidateArgs();
            }
            catch (OptionException e)
            {
                Console.Write("error: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try 'stampver --help' for more information.");
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

            var newVersionNumber = string.Empty;
            foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory(), fileToSearch, SearchOption.AllDirectories))
            {
                LogIfVerbose($"Processing file: {file}", versionArgs);

                var fileLines = File.ReadAllLines(file);
                var fileHasBeenModified = false;

                for (var i = 0; i < fileLines.Length; i++)
                {
                    var result = ProcessFileLine(fileLines[i], versionArgs);
                    if (result.LineWasModified)
                    {
                        fileHasBeenModified = true;
                        newVersionNumber = result.NewVersionNumber;
                    }
                    fileLines[i] = result.Line;
                }

                if (versionArgs.IsDryrun || !fileHasBeenModified)
                {
                    continue;
                }

                var tempFileName = Path.GetTempFileName();
                File.WriteAllLines(tempFileName, fileLines);
                File.Copy(tempFileName, file, true);
                File.Delete(tempFileName);               
            }
            if (versionArgs.OutputType == OutputType.NotSet)
            {
                // We're neither in quiet mode nor verbose mode,
                //so simply return the most recent new version number.
                Console.WriteLine(newVersionNumber);
            }
        }

        private static ProcessedLineResult ProcessFileLine(string fileLine, VersionArgs versionArgs)
        {
            var regex = new Regex(@"Assembly(?:|File)Version\(""(?<version>\d{1,5}\.\d{1,5}\.(?:\d{1,5}|\*|)(?:\.|)(?:\d{1,5}|\*|))""\)");

            // Ignore comment lines.
            if (fileLine.Trim().StartsWith(@"//"))
            {
                return new ProcessedLineResult(fileLine,false,null);
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
            var prefix = versionArgs.IsDryrun ? "Would Change:" : "Changed:";
            LogIfVerbose($"{prefix} {fileLine} to {newFileLine}", versionArgs);
            return new ProcessedLineResult(newFileLine, true, replacedVersionNumber);
        }

        private static void LogIfVerbose(string output, VersionArgs versionArgs)
        {
            if (versionArgs.OutputType == OutputType.Verbose)
            {
                Console.WriteLine(output);
            }
        }

        private static void DisplayHelpText()
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
only usable with the -i or -d commands.  Attemping to use commands and version
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
https://msdn.microsoft.com/en-us/library/dd383571(v=vs.110).aspx#Anchor_2
Specifying a filepattern will search for files matching the file pattern
(rather than AssemblyInfo.cs) in order to try to make version changes.
Note that the way the utility matches within the file is exactly the same,
so file must still have a string matching [assembly: AssemblyVersion(""x.y.z"")]
within the file before version number changes will be made.

This help text is always able to be displayed by passing --help to the program.

This is version: " + versionString;
            Console.WriteLine(helpText);
        }
    }
}
