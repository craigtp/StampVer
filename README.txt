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

This help text is always able to be displayed by passing --help to the program.

Examples:
stamperver -i MINOR --verbose
would update AssemblyInfo.cs files with versions of 1.1.3 to 1.2.0 with full
output to the console of all changes made.


