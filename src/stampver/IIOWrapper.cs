using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace stampver
{
    public interface IIOWrapper
    {
        IEnumerable<string> EnumerateFiles(string fileToSearch);
        string[] ReadAllLinesFromFile(string file);
        void WriteFileLinesToFile(IEnumerable<string> fileLines, string file);
        void WriteToStdOut(string output);
    }
}