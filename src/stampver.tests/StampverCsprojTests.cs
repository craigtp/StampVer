using System.Collections.Generic;
using NUnit.Framework;
using static stampver.Tests.TestHelpers;

namespace stampver.Tests
{
    // Pins the SDK-style csproj support added alongside the existing
    // AssemblyInfo.cs handling. Two synthetic csproj fixtures cover:
    //   - All four supported element forms (<AssemblyVersion>, <FileVersion>,
    //     <Version>, <VersionPrefix>) at the same version in one PropertyGroup.
    //   - Conditional attributes on the element (Condition="...").
    //   - A line-commented version element that must not be matched.
    [TestFixture]
    internal sealed class StampverCsprojTests
    {
        // A representative csproj exercising every element variant + a
        // line-commented entry that the parser must skip.
        private const string Project1Csproj = """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <AssemblyVersion>2.4.6</AssemblyVersion>
    <FileVersion>2.4.6</FileVersion>
    <Version>2.4.6</Version>
    <VersionPrefix>2.4.6</VersionPrefix>
    <!-- old: <Version>9.9.9</Version> -->
  </PropertyGroup>
</Project>
""";

        // A csproj exercising the conditional-attribute path
        // (`<AssemblyVersion Condition="...">...</AssemblyVersion>`).
        private const string Project2Csproj = """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <AssemblyVersion Condition="'$(Custom)' == 'true'">3.5.7</AssemblyVersion>
  </PropertyGroup>
</Project>
""";

        // A csproj with no recognised version elements at all — the parser
        // must leave it untouched.
        private const string ProjectWithNoVersionCsproj = """
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
</Project>
""";

        private static FakeIOWrapper NewCsprojFakeIO()
        {
            return new FakeIOWrapper(
                files: new[] { "Project1.csproj", "Project2.csproj" },
                fileContents: new Dictionary<string, string>
                {
                    { "Project1.csproj", Project1Csproj },
                    { "Project2.csproj", Project2Csproj },
                },
                filesPattern: "*.csproj");
        }

        #region Increment

        [Test]
        public void CallingStampverAgainstCsproj_WithIncrementPatch_UpdatesEveryElementVariantAndReportsCounts()
        {
            // Project1.csproj has 4 elements at 2.4.6 (one commented out — must
            // NOT be touched). Project2.csproj has 1 conditional-attribute
            // element at 3.5.7. Increment-patch produces 2.4.7 and 3.5.8.
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "2.4.7 (4 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.StdOutputLines, "3.5.8 (1 occurrence in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "    <AssemblyVersion>2.4.7</AssemblyVersion>");
            AssertContains(fakeIOWrapper.FileLinesOutput, "    <FileVersion>2.4.7</FileVersion>");
            AssertContains(fakeIOWrapper.FileLinesOutput, "    <Version>2.4.7</Version>");
            AssertContains(fakeIOWrapper.FileLinesOutput, "    <VersionPrefix>2.4.7</VersionPrefix>");
            AssertContains(fakeIOWrapper.FileLinesOutput, "    <AssemblyVersion Condition=\"'$(Custom)' == 'true'\">3.5.8</AssemblyVersion>");
        }

        [Test]
        public void CallingStampverAgainstCsproj_WithIncrementMinor_CascadesPatchToZero()
        {
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "minor" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            // 2.4.6 → 2.5.0 (patch reset) ; 3.5.7 → 3.6.0 (patch reset).
            AssertContains(fakeIOWrapper.StdOutputLines, "2.5.0 (4 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.StdOutputLines, "3.6.0 (1 occurrence in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "    <Version>2.5.0</Version>");
        }

        [Test]
        public void CallingStampverAgainstCsproj_WithIncrementMajor_CascadesMinorAndPatchToZero()
        {
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "major" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            // 2.4.6 → 3.0.0 ; 3.5.7 → 4.0.0.
            AssertContains(fakeIOWrapper.StdOutputLines, "3.0.0 (4 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.StdOutputLines, "4.0.0 (1 occurrence in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "    <Version>3.0.0</Version>");
        }

        #endregion

        #region Decrement

        [Test]
        public void CallingStampverAgainstCsproj_WithDecrementPatch_DoesNotCascade()
        {
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "2.4.5 (4 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.StdOutputLines, "3.5.6 (1 occurrence in 1 file)");
        }

        [Test]
        public void CallingStampverAgainstCsproj_WithDecrementMinor_DoesNotCascade()
        {
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "minor" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            // Decrement does NOT cascade — patch stays where it was.
            AssertContains(fakeIOWrapper.StdOutputLines, "2.3.6 (4 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.StdOutputLines, "3.4.7 (1 occurrence in 1 file)");
        }

        #endregion

        #region Explicit set

        [Test]
        public void CallingStampverAgainstCsproj_WithExplicitSet_RewritesEveryRecognisedElementInBothFiles()
        {
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "9.8.7" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            // 5 total occurrences across the two files all rewritten to 9.8.7.
            AssertContains(fakeIOWrapper.StdOutputLines, "9.8.7 (5 occurrences in 2 files)");
        }

        #endregion

        #region Commented lines and unsupported elements

        [Test]
        public void CallingStampverAgainstCsproj_WithLineCommentedVersionElement_DoesNotMatchTheCommentedLine()
        {
            // The synthetic Project1.csproj contains `<!-- old: <Version>9.9.9</Version> -->`.
            // After increment-patch, NO output should reference 9.9.x.
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch" });

            sut.Run();

            AssertDoesNotContain(fakeIOWrapper.StdOutputLines, "9.9.");
            AssertDoesNotContain(fakeIOWrapper.FileLinesOutput, "9.9.10");
            // The commented line itself must round-trip unchanged.
            AssertContains(fakeIOWrapper.FileLinesOutput, "    <!-- old: <Version>9.9.9</Version> -->");
        }

        [Test]
        public void CallingStampverAgainstCsproj_WithNoRecognisedVersionElement_LeavesFileUntouched()
        {
            var fakeIOWrapper = new FakeIOWrapper(
                files: new[] { "Empty.csproj" },
                fileContents: new Dictionary<string, string>
                {
                    { "Empty.csproj", ProjectWithNoVersionCsproj },
                },
                filesPattern: "*.csproj");
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            // No matches → no writes and no summary lines about new versions.
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
        }

        #endregion

        #region Output mode coverage

        [Test]
        public void CallingStampverAgainstCsproj_WithQuietMode_SuppressesStdoutButStillWritesFiles()
        {
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--quiet" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "    <Version>2.4.7</Version>");
        }

        [Test]
        public void CallingStampverAgainstCsproj_WithVerboseMode_LogsEveryChangedLine()
        {
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--verbose" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: Project1.csproj");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line");
        }

        [Test]
        public void CallingStampverAgainstCsproj_WithDryRun_LogsButDoesNotWrite()
        {
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--dryrun" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Would Change (Line");
        }

        #endregion

        #region Default-pattern coverage (the dual-default scan)

        [Test]
        public void CallingStampverAgainstCsproj_WithNoPositionalArgument_StillFindsCsprojViaDefaultPattern()
        {
            // The most important integration test for this enhancement: with NO
            // positional file argument, Stampver scans both "AssemblyInfo.cs"
            // and "*.csproj" by default. The fake IO returns nothing for the
            // first pattern and our csproj fixture for the second.
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            // 5 csproj writes (one per modified file: Project1 has 9 lines
            // total in the WriteFileLinesToFile call, Project2 has 4) plus
            // possibly more — assert via the visible summary instead.
            AssertContains(fakeIOWrapper.StdOutputLines, "2.4.7 (4 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.StdOutputLines, "3.5.8 (1 occurrence in 1 file)");
        }

        [Test]
        public void CallingStampverAgainstCsproj_WithExplicitCsprojPositionalArgument_RestrictsScanToCsproj()
        {
            // Explicit positional argument suppresses the AssemblyInfo.cs
            // default and uses only the given pattern.
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "*.csproj" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "2.4.7 (4 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.StdOutputLines, "3.5.8 (1 occurrence in 1 file)");
        }

        [Test]
        public void CallingStampverAgainstCsproj_WithDirFlag_StillProcessesViaDefaultPatterns()
        {
            // --dir composes with the dual-default scan: with no positional pattern, the
            // csproj fixture is still picked up via the "*.csproj" default, and the
            // starting directory is forwarded to enumeration.
            var fakeIOWrapper = NewCsprojFakeIO();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--dir", "somedir" });

            var exitCode = sut.Run();

            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "2.4.7 (4 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.StdOutputLines, "3.5.8 (1 occurrence in 1 file)");
            Assert.That(fakeIOWrapper.EnumeratedDirectories, Has.Member("somedir"));
        }

        #endregion
    }
}
