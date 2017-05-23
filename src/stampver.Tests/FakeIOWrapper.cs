using System;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace stampver.Tests
{
    public class FakeIOWrapper : IIOWrapper
    {
        public List<string> FileLinesOutput { get; set; }
        public List<string> StdOutputLines { get; set; }

        public FakeIOWrapper()
        {
            FileLinesOutput = new List<string>();
            StdOutputLines = new List<string>();
        }

        public IEnumerable<string> EnumerateFiles(string fileToSearch)
        {
            return new List<string>
            {
                "File1", "File2", "File3"
            };
        }

        public string[] ReadAllLinesFromFile(string file)
        {
            string fileText;
            switch(file)
            {
                case "File3":
                    fileText = @"using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""stampver"")]
[assembly: AssemblyDescription(""A small utility to help maintain version numbers for .NET projects"")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""stampver"")]
[assembly: AssemblyCopyright(""Copyright © 2016"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid(""f2b7ed31-a1f4-4650-98a1-3a9e2d3bea47"")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion(""1.0.*"")]

#pragma warning disable CS7035
[assembly: AssemblyVersion(""1.3.0"")]
[assembly: AssemblyFileVersion(""1.3.0"")]
#pragma warning restore CS7035
";
                    break;
                case "File2":
                    fileText = @"using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""stampver.Tests"")]
[assembly: AssemblyDescription("""")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""stampver.Tests"")]
[assembly: AssemblyCopyright(""Copyright ©  2017"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid(""b1ab82a4-79b0-4238-94d2-2f5988f2e222"")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion(""1.0.*"")]
[assembly: AssemblyVersion(""1.0.0.0"")]
[assembly: AssemblyFileVersion(""1.0.0.0"")]";
                    break;
                default:                // File1 and all others
                    fileText = @"using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(""stampver"")]
[assembly: AssemblyDescription(""A small utility to help maintain version numbers for .NET projects"")]
[assembly: AssemblyConfiguration("""")]
[assembly: AssemblyCompany("""")]
[assembly: AssemblyProduct(""stampver"")]
[assembly: AssemblyCopyright(""Copyright © 2016"")]
[assembly: AssemblyTrademark("""")]
[assembly: AssemblyCulture("""")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid(""f2b7ed31-a1f4-4650-98a1-3a9e2d3bea47"")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion(""1.0.*"")]

#pragma warning disable CS7035
[assembly: AssemblyVersion(""1.3.0"")]
[assembly: AssemblyFileVersion(""1.3.0"")]
#pragma warning restore CS7035
";
                    break;
            }
            return fileText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        public void WriteFileLinesToFile(IEnumerable<string> fileLines, string file)
        {
            // We don't write anything to the file system, we simply append the fileLines input
            // to the public FileLinesOutput property for inspection by code outside this class.
            FileLinesOutput.AddRange(fileLines.ToList());
        }

        public void WriteToStdOut(string output)
        {
            StdOutputLines.Add(output);
        }
    }
}
