using System;
using System.IO;
using System.Text.RegularExpressions;
using NDesk.Options;

namespace stampver
{
    public class VersionArgs
    {
        public VersionNumberCommand VersionNumberCommand { get; private set; } = VersionNumberCommand.NotSet;
        public VersionNumberPart VersionNumberPart { get; private set; } = VersionNumberPart.NotSet;
        public string ExplicitVersionNumber { get; private set; } = string.Empty;
        public bool DisplayHelp { get; private set; }
        public OutputType OutputType { get; private set; } = OutputType.NotSet;
        public string FilePattern { get; private set; } = string.Empty;
        public bool IsDryrun { get; private set; }

        public void SetDisplayHelp()
        {
            DisplayHelp = true;
        }

        public void SetDryrun()
        {
            IsDryrun = true;
        }

        public void SetFilePattern(string filePattern)
        {
            FilePattern = filePattern;
        }

        public void SetIncrement(string versionPart)
        {
            AssertVersionNumberCommandNotAlreadySet();
            AssertVersionNumberPartIsValid(versionPart);
            SetVersionNumberPart(versionPart);
            VersionNumberCommand = VersionNumberCommand.Increment;
        }
        
        public void SetDecrement(string versionPart)
        {
            AssertVersionNumberCommandNotAlreadySet();
            AssertVersionNumberPartIsValid(versionPart);
            SetVersionNumberPart(versionPart);
            VersionNumberCommand = VersionNumberCommand.Decrement;
        }

        public void SetExplicit(string versionNumber)
        {
            AssertVersionNumberCommandNotAlreadySet();
            if (!Regex.IsMatch(versionNumber, @"[\d]{1,5}\.[\d]{1,5}\.[\d]{1,5}", RegexOptions.IgnoreCase))
            {
                throw new OptionException("Invalid version number specified", "-e");
            }
            var versionNumbers = versionNumber.Split('.');
            foreach (var number in versionNumbers)
            {
                int versionNumberInteger;
                if (!int.TryParse(number, out versionNumberInteger))
                {
                    throw new OptionException("Invalid version number specified", "-e");
                }
                if (versionNumberInteger < 0 || versionNumberInteger > 65535)
                {
                    throw new OptionException("Invalid version number specified", "-e");
                }
            }
            ExplicitVersionNumber = versionNumber;
            VersionNumberCommand = VersionNumberCommand.ExplicitSet;
        }

        public void SetQuiet()
        {
            AssertOutputTypeIsNotAlreadySet();
            OutputType = OutputType.Quiet;
        }

        public void SetVerbose()
        {
            AssertOutputTypeIsNotAlreadySet();
            OutputType = OutputType.Verbose;
        }
        
        public void ValidateArgs()
        {
            if (VersionNumberCommand == VersionNumberCommand.NotSet && !DisplayHelp)
            {
                throw new OptionException("Must specify a valid version number command!", string.Empty);
            }
            if (IsDryrun)
            {
                OutputType = OutputType.Verbose;
            }
            if (string.IsNullOrEmpty(FilePattern)) return;
            try
            {
                Directory.EnumerateFiles(Directory.GetCurrentDirectory(), FilePattern, SearchOption.AllDirectories);
            }
            catch (Exception)
            {
                throw new OptionException("Invalid file pattern specified!", string.Empty);
            }
        }

        #region Private Helper Methods
        private void SetVersionNumberPart(string versionPart)
        {
            switch (versionPart.ToLower())
            {
                case "major":
                    VersionNumberPart = VersionNumberPart.Major;
                    break;
                case "minor":
                    VersionNumberPart = VersionNumberPart.Minor;
                    break;
                case "patch":
                case "build":
                    VersionNumberPart = VersionNumberPart.Patch;
                    break;
            }
        }

        private static void AssertVersionNumberPartIsValid(string versionPart)
        {
            if (versionPart == null) throw new ArgumentNullException(nameof(versionPart));

            if (!Regex.IsMatch(versionPart, @"MAJOR|MINOR|PATCH|BUILD", RegexOptions.IgnoreCase))
            {
                throw new OptionException("Invalid version number part specified", string.Empty);
            }
        }

        private void AssertVersionNumberCommandNotAlreadySet()
        {
            if (VersionNumberCommand != VersionNumberCommand.NotSet)
            {
                throw new OptionException("Increment, decrement or explicit commands are mutually exclusive!", string.Empty);
            }
        }

        private void AssertOutputTypeIsNotAlreadySet()
        {
            if(OutputType != OutputType.NotSet)
            {
                throw new OptionException("Quiet and Verbose options are mutually exclusive!", string.Empty);
            }
        }
        #endregion
    }

    public enum VersionNumberCommand
    {
        NotSet = 0,
        Increment = 1,
        Decrement = 2,
        ExplicitSet = 3
    }

    public enum VersionNumberPart
    {
        NotSet = 0,
        Major = 1,
        Minor = 2,
        Patch = 3
    }

    public enum OutputType
    {
        NotSet = 0,
        Quiet = 1,
        Verbose = 2,
    }
}
