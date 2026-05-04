using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace stampver
{
    internal interface IIOWrapper
    {
        IEnumerable<string> EnumerateFiles(string fileToSearch);
        string[] ReadAllLinesFromFile(string file);
        void WriteFileLinesToFile(IEnumerable<string> fileLines, string file);
        void WriteToStdOut(string output);
        void WriteToStdErr(string output);
    }
}