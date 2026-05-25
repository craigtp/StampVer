using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using stampver.Options;

namespace stampver
{
    internal sealed class VersionArgs
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

        // Anchored so substrings can't slip through — without ^...$, "1.2.3.4.5.6"
        // would match its "1.2.3" prefix and be silently accepted.
        private static readonly Regex ExplicitVersionRegex = new(
            @"^\d{1,5}\.\d{1,5}\.\d{1,5}$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public void SetExplicit(string versionNumber)
        {
            AssertVersionNumberCommandNotAlreadySet();
            if (!ExplicitVersionRegex.IsMatch(versionNumber))
            {
                throw new OptionException("Invalid version number specified", "-e");
            }
            // The regex already guarantees three 1-5 digit numeric parts, so the only
            // remaining check is the per-part upper bound. ushort.TryParse rejects
            // anything above ushort.MaxValue (65535) in a single call; NumberStyles.None
            // keeps it strict (the regex has already excluded signs/whitespace anyway).
            foreach (var part in versionNumber.Split('.'))
            {
                if (!ushort.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out _))
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
            // Promote the sentinel default so callers downstream can rely on a
            // concrete mode (Quiet/Normal/Verbose) without re-checking NotSet.
            if (OutputType == OutputType.NotSet)
            {
                OutputType = OutputType.Normal;
            }

            // Reject patterns containing characters Directory.EnumerateFiles forbids,
            // without touching the filesystem. The real enumeration runs behind
            // IIOWrapper and surfaces any genuine filesystem errors at processing time,
            // so validating here keeps VersionArgs a pure intent object (no disk walk).
            if (!string.IsNullOrEmpty(FilePattern) && FilePattern.IndexOfAny(InvalidPatternChars) >= 0)
            {
                throw new OptionException("Invalid file pattern specified!", string.Empty);
            }
        }

        // Characters Directory.EnumerateFiles rejects in a search pattern. The two
        // wildcard characters '*' and '?' are explicitly allowed through. Note this
        // set is platform-dependent: on Windows it includes '<', '>', '|', ':' etc.;
        // on Unix it is just '\0' and '/'. '\0' is invalid on every platform.
        private static readonly char[] InvalidPatternChars =
            Path.GetInvalidFileNameChars().Where(c => c is not ('*' or '?')).ToArray();

        #region Private Helper Methods
        private void SetVersionNumberPart(string versionPart)
        {
            // Total mapping: the default arm means an unmatched value can never be
            // silently accepted (which previously left VersionNumberPart at NotSet,
            // turning a typo into a no-op success). The anchored regex already guards
            // this, but keeping the switch total is cheap defence-in-depth.
            VersionNumberPart = versionPart.ToLowerInvariant() switch
            {
                "major" => VersionNumberPart.Major,
                "minor" => VersionNumberPart.Minor,
                "patch" or "build" => VersionNumberPart.Patch,
                _ => throw new OptionException("Invalid version number part specified", string.Empty),
            };
        }

        // Anchored so substrings can't slip through — without ^...$, values like
        // "Patcher", "xmajor" or "build something" would match an embedded token and
        // be silently accepted, then no-op downstream (AssemblyVersion.Adjust maps
        // an unset part to index -1 and returns). Mirrors the anchoring fix for -e.
        private static readonly Regex VersionPartRegex = new(
            @"^(?:MAJOR|MINOR|PATCH|BUILD)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        private static void AssertVersionNumberPartIsValid(string versionPart)
        {
            ArgumentNullException.ThrowIfNull(versionPart);

            if (!VersionPartRegex.IsMatch(versionPart))
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

    internal enum VersionNumberCommand
    {
        NotSet = 0,
        Increment = 1,
        Decrement = 2,
        ExplicitSet = 3
    }

    internal enum VersionNumberPart
    {
        NotSet = 0,
        Major = 1,
        Minor = 2,
        Patch = 3
    }

    internal enum OutputType
    {
        // Sentinel: parser hasn't observed --quiet/--verbose yet. ValidateArgs
        // upgrades this to Normal so the rest of the pipeline never sees NotSet.
        NotSet = 0,
        Quiet = 1,
        Verbose = 2,
        Normal = 3,
    }
}
