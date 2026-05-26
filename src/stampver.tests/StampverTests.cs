using System;
using System.Collections.Generic;
using NUnit.Framework;
using static stampver.Tests.TestHelpers;

namespace stampver.Tests
{
    [TestFixture]
    internal sealed class StampverTests
    {
        #region Miscellaneous Tests
        [Test]
        public void CallingStampverWithNoArguments_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { string.Empty });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "must specify a valid version number command");
        }

        [Test]
        public void CallingStampverWithHelpArgument_OutputsHelpText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "--help" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "stampver by Craig Phillips <craig@craigtp.co.uk>");
            AssertContains(fakeIOWrapper.StdOutputLines, "A small command-line utility");
            AssertContains(fakeIOWrapper.StdOutputLines, "Usage");
        }

        [Test]
        public void CallingStampverWithIncrementCommandButNoVersionPartArgument_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Missing required value for option '-i'.");
        }

        [Test]
        public void CallingStampverWithDecrementCommandButNoVersionPartArgument_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Missing required value for option '-d'.");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButNoVersionPartArgument_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Missing required value for option '-e'.");
        }

        [Test]
        public void CallingStampverWithInvalidCommand_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-x" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "must specify a valid version number command");
        }
        
        [Test]
        public void CallingStampverWithQuietAndVerboseOptions_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "1.0.0", "--quiet", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Quiet and Verbose options are mutually exclusive!");
        }
        #endregion

        #region Increment version number tests
        [Test]
        public void CallingStampverWithIncrementPatchCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.0.1.0 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "1.3.1 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
        }

        [Test]
        public void CallingStampverWithIncrementBuildCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "build" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.0.1.0 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "1.3.1 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMinorCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "minor" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.1.0.0 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.1.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "1.4.0 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.4.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMajorCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "major" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "2.0.0.0 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "2.0.0 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementButIncorrectVersionNumberPartCommand_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "incorrect" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number part specified");
        }
        #endregion

        #region Decrement version number tests
        [Test]
        public void CallingStampverWithDecrementPatchCommandWhenPatchAlreadyZero_MakesNoChangeAndReportsNothing()
        {
            // The default fixtures all sit at patch 0, so decrementing patch is a no-op.
            // A no-op must NOT be written back or reported (previously this test pinned
            // the buggy behaviour where the unchanged version was still counted and the
            // whole file rewritten). Real patch decrement is covered by
            // CallingStampverWithDecrementPatchCommandWhenPatchIsNonZero_*.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWhenPatchAlreadyZero_MakesNoChangeAndReportsNothing()
        {
            // BUILD is a synonym for PATCH and the fixtures sit at patch 0,
            // so this is a no-op — nothing written or reported.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
        }

        [Test]
        public void CallingStampverWithDecrementPatchCommandWhenPatchIsNonZero_DecrementsPatch()
        {
            // When patch is non-zero, decrement actually lowers it. The default fixtures
            // can't show this (they sit at patch 0), so use a custom fixture at 1.3.5.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper(
                files: new[] { "Custom.cs" },
                fileContents: new Dictionary<string, string>
                {
                    { "Custom.cs", "[assembly: AssemblyVersion(\"1.3.5\")]\n[assembly: AssemblyFileVersion(\"1.3.5\")]\n" },
                });
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.3.4 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.4\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyFileVersion(\"1.3.4\")]");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWhenPatchIsNonZero_DecrementsPatch()
        {
            // Positive coverage that BUILD behaves as a synonym for PATCH on a real change.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper(
                files: new[] { "Custom.cs" },
                fileContents: new Dictionary<string, string>
                {
                    { "Custom.cs", "[assembly: AssemblyVersion(\"1.3.5\")]\n[assembly: AssemblyFileVersion(\"1.3.5\")]\n" },
                });
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.3.4 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.4\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMinorCommand_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "minor" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.2.0 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.0\")]");
            // File2 sits at 1.0.0.0, so its minor part is already 0 —
            // decrement there is a no-op and is no longer counted in the summary nor written back.
            AssertDoesNotContain(fakeIOWrapper.StdOutputLines, "1.0.0.0 (");
            AssertDoesNotContain(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMajorCommand_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "major" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "0.0.0.0 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "0.3.0 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementButIncorrectVersionNumberPartCommand_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "incorrect" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number part specified");
        }
        #endregion

        #region Explicit version number tests
        [Test]
        public void CallingStampverWithExplicitVersionCommandAndValidVersionNumber_SetVersionNumberAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "5.6.7" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "5.6.7 (6 occurrences in 3 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"5.6.7\")]");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButInvalidVersionNumber_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "THIS.IS.NOT.A.VERSION.NUMBER" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number specified");
        }

        [Test]
        public void CallingStampverWithExplicitCommandAndTooManyParts_OutputsErrorText()
        {
            // Regression test for the unanchored regex bug. Without "^...$", the
            // pattern "[\d]{1,5}\.[\d]{1,5}\.[\d]{1,5}" matches the "1.2.3" prefix
            // of "1.2.3.4.5.6", so the value passes validation and gets slammed
            // into AssemblyInfo.cs verbatim — producing invalid attribute values
            // like [assembly: AssemblyVersion("1.2.3.4.5.6")]. Anchoring rejects
            // this upfront.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "1.2.3.4.5.6" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number specified");
        }

        [Test]
        public void CallingStampverWithExplicitCommandAndOutOfRangeVersionPart_OutputsErrorText()
        {
            // The "[\d]{1,5}" regex in VersionArgs.SetExplicit accepts up to 5 digits
            // per part, so values above UInt16.MaxValue (65535) pass the regex and
            // are caught only by the explicit "> 65535" guard. This test exercises
            // that guard — without it, stampver would silently accept invalid
            // assembly versions and write them to disk.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "65536.0.0" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number specified");
        }
        #endregion

        #region Increment version number tests with quiet
        [Test]
        public void CallingStampverWithIncrementPatchCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
        }

        [Test]
        public void CallingStampverWithIncrementBuildCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "build", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMinorCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "minor", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.1.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.4.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMajorCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "major", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementButIncorrectVersionNumberPartCommandWithQuiet_OutputsErrorText()
        {
            // Even when using the quiet parameter, error conditions will output to standard out.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "incorrect", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number part specified");
        }
        #endregion

        #region Decrement version number tests with quiet
        [Test]
        public void CallingStampverWithDecrementPatchCommandWithQuietWhenPatchAlreadyZero_MakesNoChange()
        {
            // No-op decrement (patch already 0) writes nothing. Quiet already suppresses stdout;
            // the point here is that no file is written either.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWithQuietWhenPatchAlreadyZero_MakesNoChange()
        {
            // BUILD synonym, no-op, nothing written.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
        }

        [Test]
        public void CallingStampverWithDecrementMinorCommandWithQuiet_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "minor", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.0\")]");
            // File2's 1.0.0.0 minor decrement is a no-op, so that file is not written.
            AssertDoesNotContain(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMajorCommandWithQuiet_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "major", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementButIncorrectVersionNumberPartCommandWithQuiet_OutputsErrorText()
        {
            // Even when using the quiet parameter, error conditions will output to standard out.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "incorrect", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number part specified");
        }
        #endregion

        #region Explicit version number tests with quiet
        [Test]
        public void CallingStampverWithExplicitVersionCommandAndValidVersionNumberWithQuiet_SetVersionNumberAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "5.6.7", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"5.6.7\")]");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButInvalidVersionNumberWithQuiet_OutputsErrorText()
        {
            // Even when using the quiet parameter, error conditions will output to standard out.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "THIS.IS.NOT.A.VERSION.NUMBER", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number specified");
        }
        #endregion

        #region Increment version number tests with verbose
        [Test]
        public void CallingStampverWithIncrementPatchCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.1.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementBuildCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "build", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.1.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMinorCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "minor", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.1.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.4.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.4.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.4.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.1.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.1.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMajorCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "major", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"2.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"2.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"2.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"2.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementButIncorrectVersionNumberPartCommandWithVerbose_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "incorrect", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number part specified");
        }
        #endregion

        #region Decrement version number tests with verbose
        [Test]
        public void CallingStampverWithDecrementPatchCommandWithVerboseWhenPatchAlreadyZero_LogsProcessingButNoChange()
        {
            // A no-op decrement (patch already 0) is no longer logged as
            // "Changed ... 1.3.0 to 1.3.0" and the file is not written.
            // Verbose still logs which files were processed.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertDoesNotContain(fakeIOWrapper.StdOutputLines, "Changed");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWithVerboseWhenPatchAlreadyZero_LogsProcessingButNoChange()
        {
            // BUILD synonym, no-op — no "Changed" line and nothing written.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertDoesNotContain(fakeIOWrapper.StdOutputLines, "Changed");
        }

        [Test]
        public void CallingStampverWithDecrementMinorCommandWithVerbose_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "minor", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.2.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.2.0\")]");
            // File2's 1.0.0.0 minor decrement is a no-op — no
            // "Changed ... to ...1.0.0.0" line, and File2 is not written.
            AssertDoesNotContain(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertDoesNotContain(fakeIOWrapper.StdOutputLines, "to [assembly: AssemblyVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMajorCommandWithVerbose_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "major", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"0.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"0.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"0.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"0.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementButIncorrectVersionNumberPartCommandWithVerbose_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "incorrect", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number part specified");
        }
        #endregion

        #region Explicit version number tests with verbose
        [Test]
        public void CallingStampverWithExplicitVersionCommandAndValidVersionNumberWithVerbose_SetVersionNumberAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "5.6.7", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"5.6.7\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyFileVersion(\"5.6.7\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"5.6.7\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"5.6.7\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"5.6.7\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"5.6.7\")]");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButInvalidVersionNumberWithVerbose_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "THIS.IS.NOT.A.VERSION.NUMBER", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number specified");
        }
        #endregion

        #region Increment version number tests with dryrun
        [Test]
        public void CallingStampverWithIncrementPatchCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.1.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementBuildCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "build", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.1\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.1.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.1.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMinorCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "minor", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.4.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.4.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.1.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.1.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMajorCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "major", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"2.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"2.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"2.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"2.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementButIncorrectVersionNumberPartCommandWithDryrun_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "incorrect", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number part specified");
        }
        #endregion

        #region Decrement version number tests with dryrun
        [Test]
        public void CallingStampverWithDecrementPatchCommandWithDryrunWhenPatchAlreadyZero_LogsProcessingButNoChange()
        {
            // A no-op decrement (patch already 0) no longer produces a
            // "Would change ... 1.3.0 to 1.3.0" line. Dryrun never writes.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertDoesNotContain(fakeIOWrapper.StdOutputLines, "Would change");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWithDryrunWhenPatchAlreadyZero_LogsProcessingButNoChange()
        {
            // BUILD synonym, no-op — no "Would change" line.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertDoesNotContain(fakeIOWrapper.StdOutputLines, "Would change");
        }

        [Test]
        public void CallingStampverWithDecrementMinorCommandWithDryrun_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "minor", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.2.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.2.0\")]");
            // File2's 1.0.0.0 minor decrement is a no-op — no "Would change ... to ...1.0.0.0" line.
            AssertDoesNotContain(fakeIOWrapper.StdOutputLines, "to [assembly: AssemblyVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMajorCommandWithDryrun_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "major", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"0.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"0.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"0.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"0.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementButIncorrectVersionNumberPartCommandWithDryrun_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "incorrect", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number part specified");
        }
        #endregion

        #region Explicit version number tests with dryrun
        [Test]
        public void CallingStampverWithExplicitVersionCommandAndValidVersionNumberWithDryrun_SetVersionNumberAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "5.6.7", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"5.6.7\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"5.6.7\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"5.6.7\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"5.6.7\")]");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButInvalidVersionNumberWithDryrun_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "THIS.IS.NOT.A.VERSION.NUMBER", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number specified");
        }
        #endregion

        #region Pluralisation tests
        [Test]
        public void CallingStampverAgainstSingleAttributeInSingleFile_OutputsSingularPluralisation()
        {
            // Regression test for the misspelt "occurence" string and the pluralisation logic.
            // The default FakeIOWrapper fixture has 2 attributes per file, so the singular
            // form ("1 occurrence in 1 file") is never exercised by any other test. This
            // test uses a custom one-file/one-attribute fixture to pin the singular path
            // and to ensure the misspelt "occurence" never reappears in user-facing output.

            // Arrange
            const string singleAttributeFile = @"using System.Reflection;
[assembly: AssemblyVersion(""2.4.6"")]
";
            var fakeIOWrapper = new FakeIOWrapper(
                files: new[] { "OnlyFile" },
                fileContents: new Dictionary<string, string> { { "OnlyFile", singleAttributeFile } });
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "2.4.7 (1 occurrence in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.4.7\")]");
            AssertDoesNotContain(fakeIOWrapper.StdOutputLines, "occurence");
        }
        #endregion

        #region Extra arguments tests
        [Test]
        public void CallingStampverWithMultiplePositionalArguments_EmitsWarningButContinuesUsingFirst()
        {
            // Regression test for the silent-drop bug. Previously, a command like
            // "stampver -i patch *.cs *.vb" would silently use only "*.cs" with no
            // indication to the user that "*.vb" had been ignored. Now we warn on
            // stderr but still proceed with the first pattern so that valid
            // single-pattern usage is unaffected.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "AssemblyInfo.cs", "Extra.cs" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdErrorLines, "warning: ignoring extra arguments after 'AssemblyInfo.cs'");
            // The first pattern still drives the run, so the FakeIOWrapper's
            // default fixture of three files is still processed.
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.GreaterThan(0));
        }
        #endregion

        #region Culture handling tests
        [Test]
        [SetCulture("tr-TR")]
        public void CallingStampverWithIncrementMinorInTurkishCulture_StillIncrementsMinor()
        {
            // Regression test for the dotted-i / dotless-i bug. In tr-TR,
            // "MINOR".ToLower() returns "mınor" (with dotless ı), which fails to
            // match the "minor" case label, so VersionNumberPart is never set
            // and the increment becomes a silent no-op. ToLowerInvariant fixes it.
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "MINOR" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.4.0 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.StdOutputLines, "1.1.0.0 (2 occurrences in 1 file)");
        }
        #endregion

        #region Unhandled exception tests
        [Test]
        public void WhenStampverThrowsUnhandledException_ProgramReturnsUnexpectedErrorExitCodeAndWritesToStdErr()
        {
            // Regression test for the top-level catch in Program.RunWithIoWrapper.
            // Forces EnumerateFiles to throw so we can verify the catch path
            // converts the exception into ExitCodes.UnexpectedError, surfaces
            // ex.Message on stderr, and does NOT leak a stack trace.
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper
            {
                ExceptionToThrowOnEnumerate = new UnauthorizedAccessException("simulated permission denied")
            };

            // Act
            var exitCode = Program.RunWithIoWrapper(fakeIOWrapper, new[] { "-i", "MAJOR" });

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UnexpectedError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "stampver: unexpected error: simulated permission denied");
            AssertDoesNotContain(fakeIOWrapper.StdErrorLines, "at stampver.");
        }
        #endregion

        #region Start directory (--dir) tests
        [Test]
        public void CallingStampverWithDirFlag_ForwardsDirectoryToEnumerateAndProcessesFiles()
        {
            // --dir sets the starting directory. The fake dispatches on pattern only, so
            // the default three-file fixture is still processed; the point of this test is
            // that the directory we asked for is the one forwarded to EnumerateFiles.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--dir", "somedir" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.EnumeratedDirectories, Has.Member("somedir"));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
        }

        [Test]
        public void CallingStampverWithDirectoryAliasFlag_ForwardsDirectoryToEnumerate()
        {
            // The long-form --directory alias is equivalent to --dir.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--directory", "somedir" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.EnumeratedDirectories, Has.Member("somedir"));
        }

        [Test]
        public void CallingStampverWithDirFlagAndExplicitPattern_UsesBothDirectoryAndPattern()
        {
            // --dir composes with an explicit positional pattern: the pattern still
            // suppresses the AssemblyInfo.cs/*.csproj defaults (so EnumerateFiles is
            // called exactly once, for *.txt), and the directory is forwarded with it.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper(
                files: new[] { "Notes.txt" },
                fileContents: new Dictionary<string, string>
                {
                    { "Notes.txt", "[assembly: AssemblyVersion(\"1.2.3\")]\n" },
                },
                filesPattern: "*.txt");
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--dir", "somedir", "*.txt" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.2.4 (1 occurrence in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.4\")]");
            // Explicit pattern suppresses the dual defaults, so only one sweep happened.
            Assert.That(fakeIOWrapper.EnumeratedDirectories, Is.EqualTo(new[] { "somedir" }));
        }

        [Test]
        public void CallingStampverWithDirFlagContainingInvalidCharacter_OutputsErrorText()
        {
            // A NUL is invalid in any path, so this is rejected at validation time
            // (no filesystem access), mirroring the file-pattern validation.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--dir", "bad\0dir" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid directory specified");
        }

        [Test]
        public void CallingStampverWithDirFlagForNonExistentDirectory_OutputsErrorText()
        {
            // When the directory passes the character check but does not exist, Stampver
            // reports a clean usage error rather than letting enumeration throw into the
            // top-level catch-all.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper { StartDirectoryExists = false };
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--dir", "nonexistent-dir" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "starting directory not found");
        }

        [Test]
        public void CallingStampverWithDirFlagButNoValue_OutputsErrorText()
        {
            // --dir takes a required value; omitting it is a usage error from the parser.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "--dir" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "Missing required value for option '--dir'.");
        }

        [Test]
        public void CallingStampverWithoutDirFlag_EnumeratesCurrentDirectory()
        {
            // Backward-compatibility pin: with no --dir, every enumeration uses the
            // empty-string sentinel that IoWrapper resolves to the current directory.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.EnumeratedDirectories.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.EnumeratedDirectories, Has.All.EqualTo(string.Empty));
        }
        #endregion

        [TestCase("Patcher")]
        [TestCase("xmajor")]
        [TestCase("majorette")]
        [TestCase("build something")]
        public void CallingStampverWithVersionPartThatMerelyContainsAValidToken_OutputsErrorText(string versionPart)
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", versionPart });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "error:");
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid version number part specified");
        }

        [Test]
        public void CallingStampverWithFilePatternContainingInvalidCharacter_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "bad\0pattern.cs" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.UsageError));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdErrorLines, "Invalid file pattern specified");
        }

        [Test]
        public void CallingStampverWithValidNonDefaultFilePattern_DoesNotErrorOnValidation()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "patch", "*.txt" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdErrorLines.Count, Is.EqualTo(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
        }

        [Test]
        public void CallingStampver_OnlyReplacesTheMatchedVersionNotOtherIdenticalSubstrings()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper(
                files: new[] { "Custom.cs" },
                fileContents: new Dictionary<string, string>
                {
                    { "Custom.cs", "[assembly: AssemblyVersion(\"1.3.5\")] // previously 1.3.5\n" },
                });
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.3.4 (1 occurrence in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.4\")] // previously 1.3.5");
            AssertDoesNotContain(fakeIOWrapper.FileLinesOutput, "previously 1.3.4");
        }

        [Test]
        public void CallingStampver_DoesNotTreatAVersionWithATrailingEmptyPartAsAMatch()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper(
                files: new[] { "Custom.cs" },
                fileContents: new Dictionary<string, string>
                {
                    { "Custom.cs", "[assembly: AssemblyVersion(\"1.2.\")]\n[assembly: AssemblyFileVersion(\"2.0.0\")]\n" },
                });
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "5.6.7" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            // The valid FileVersion was rewritten; the malformed "1.2." was not matched.
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyFileVersion(\"5.6.7\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.\")]");
            AssertDoesNotContain(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"5.6.7\")]");
        }
    }
}
