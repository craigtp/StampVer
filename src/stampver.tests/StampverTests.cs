using System;
using System.Collections.Generic;
using NUnit.Framework;
using static stampver.Tests.TestHelpers;

namespace stampver.Tests
{
    [TestFixture]
    public class StampverTests
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
        public void CallingStampverWithDecrementPatchCommand_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "1.0.0.0 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "1.3.0 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommand_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
           AssertContains(fakeIOWrapper.StdOutputLines, "1.0.0.0 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "1.3.0 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
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
            AssertContains(fakeIOWrapper.StdOutputLines, "1.0.0.0 (2 occurrences in 1 file)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "1.2.0 (4 occurrences in 2 files)");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.0\")]");
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
        public void CallingStampverWithDecrementPatchCommandWithQuiet_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWithQuiet_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build", "--quiet" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
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
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.0\")]");
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
        public void CallingStampverWithDecrementPatchCommandWithVerbose_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));

            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWithVerbose_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build", "--verbose" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
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
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.2.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.2.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Changed (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
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
        public void CallingStampverWithDecrementPatchCommandWithDryrun_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWithDryrun_DecrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build", "--dryrun" });

            // Act
            var exitCode = sut.Run();

            // Assert
            Assert.That(exitCode, Is.EqualTo(ExitCodes.Success));
            Assert.That(fakeIOWrapper.StdOutputLines.Count, Is.GreaterThan(0));
            Assert.That(fakeIOWrapper.FileLinesOutput.Count, Is.EqualTo(0));
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 36): [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 37): [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
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
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 34): [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            AssertContains(fakeIOWrapper.StdOutputLines, "Would change (Line 35): [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
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
    }
}
