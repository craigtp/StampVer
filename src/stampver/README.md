Version Stamper
===============

A small command-line utility that will iterate through all of the AssemblyInfo.cs files (or other specified file) below the current folder and update the AssemblyVersion and AssemblyFileVersion attributes with a version compliant with [Semantic Versioning](http://semver.org/).  The utility can automatically increment or decrement specific parts of the version number or can explicitly set the entire version string.

Usage
-----
stampver.exe [command] [version part or specific version number] [(optional) filename to update]

where:

[command] is:
-i				=	Increment the specified version number part by 1.
-d				=	Decrement the specified version number part by 1.
-e				=	Replace the entire version number string with the specified version number
-help			=	Displays this help text.


[version part or specific version number] is:

MAJOR			=	Perform increment or decrement on the Major version number part.
MINOR			=	Perform increment or decrement on the Minor version number part.
PATCH			=	Perform increment or decrement on the Patch version number part.
BUILD			=	Synonym for PATCH. Perform increment or decrement on the Patch version number part.
x.y.z			=	A specific version number where x, y  and z are integer numbers separated by a period.

Note that the specific version number parameter value (x.y.z) is only usable with the -e command,
and the MAJOR, MINOR and PATCH/BUILD parameter values are only usable with the -i or -d commands.
Attemping to use commands and version parameters that are incompatible will cause the program to
simply terminate.

