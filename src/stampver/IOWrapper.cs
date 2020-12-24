using System;
using System.Collections.Generic;
using System.IO;

namespace stampver
{
    public class IoWrapper : IIOWrapper
    {
        public IEnumerable<string> EnumerateFiles(string fileToSearch)
        {
            return Directory.EnumerateFiles(Directory.GetCurrentDirectory(), fileToSearch, SearchOption.AllDirectories);
        }

        public string[] ReadAllLinesFromFile(string file)
        {
            return File.ReadAllLines(file);
        }

        public void WriteFileLinesToFile(IEnumerable<string> fileLines, string file)
        {
            var tempFileName = Path.GetTempFileName();
            File.WriteAllLines(tempFileName, fileLines);
            File.Copy(tempFileName, file, true);
            File.Delete(tempFileName);
        }

        public void WriteToStdOut(string output)
        {
            Console.WriteLine(output);
        }
    }
}
