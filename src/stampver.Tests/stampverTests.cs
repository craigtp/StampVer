using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace stampver.Tests
{
    [TestFixture]
    public class stampverTests
    {
        #region Miscellaneous Tests
        [Test]
        public void CallingStampverWithNoArguments_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { string.Empty });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "must specify a valid version number command");
        }

        [Test]
        public void CallingStampverWithHelpArgument_OutputsHelpText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "--help" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "stampver by Craig Phillips <craig@craigtp.co.uk>");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "A small command-line utility");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Usage");
        }

        [Test]
        public void CallingStampverWithIncrementCommandButNoVersionPartArgument_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Missing required value for option '-i'.");
        }

        [Test]
        public void CallingStampverWithDecrementCommandButNoVersionPartArgument_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Missing required value for option '-d'.");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButNoVersionPartArgument_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Missing required value for option '-e'.");
        }

        [Test]
        public void CallingStampverWithInvalidCommand_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-x" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "must specify a valid version number command");
        }
        
        [Test]
        public void CallingStampverWithQuietAndVerboseOptions_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "1.0.0", "--quiet", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Quiet and Verbose options are mutually exclusive!");
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
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.1.0 (2 occurrences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.3.1 (4 occurrences in 2 files)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
        }

        [Test]
        public void CallingStampverWithIncrementBuildCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "build" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.1.0 (2 occurrences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.3.1 (4 occurrences in 2 files)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMinorCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "minor" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.1.0.0 (2 occurrences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.1.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.4.0 (4 occurrences in 2 files)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.4.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMajorCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "major" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "2.0.0.0 (2 occurrences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "2.0.0 (4 occurrences in 2 files)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementButIncorrectVersionNumberPartCommand_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "incorrect" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
        }
        #endregion

        #region Decrement version number tests
        [Test]
        public void CallingStampverWithDecrementPatchCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.0.0 (2 occurrences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.3.0 (4 occurrences in 2 files)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.0.0 (2 occurrences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.3.0 (4 occurrences in 2 files)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMinorCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "minor" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.0.0 (2 occurrences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.2.0 (4 occurrences in 2 files)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMajorCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "major" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "0.0.0.0 (2 occurrences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "0.3.0 (4 occurrences in 2 files)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementButIncorrectVersionNumberPartCommand_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "incorrect" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
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
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "5.6.7 (6 occurrences in 3 files)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"5.6.7\")]");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButInvalidVersionNumber_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "THIS.IS.NOT.A.VERSION.NUMBER" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number specified");
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
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
        }

        [Test]
        public void CallingStampverWithIncrementBuildCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "build", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMinorCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "minor", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.1.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.4.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMajorCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "major", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementButIncorrectVersionNumberPartCommandWithQuiet_OutputsErrorText()
        {
            // Even when using the quiet parameter, error conditions will output to standard out.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "incorrect", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
        }
        #endregion

        #region Decrement version number tests with quiet
        [Test]
        public void CallingStampverWithDecrementPatchCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMinorCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "minor", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMajorCommandWithQuiet_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "major", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.3.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementButIncorrectVersionNumberPartCommandWithQuiet_OutputsErrorText()
        {
            // Even when using the quiet parameter, error conditions will output to standard out.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "incorrect", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
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
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"5.6.7\")]");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButInvalidVersionNumberWithQuiet_OutputsErrorText()
        {
            // Even when using the quiet parameter, error conditions will output to standard out.

            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "THIS.IS.NOT.A.VERSION.NUMBER", "--quiet" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number specified");
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
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.1.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementBuildCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "build", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.1.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMinorCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "minor", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.1.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.4.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.4.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.4.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.1.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.1.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMajorCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "major", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"2.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"2.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"2.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"2.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementButIncorrectVersionNumberPartCommandWithVerbose_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "incorrect", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
        }
        #endregion

        #region Decrement version number tests with verbose
        [Test]
        public void CallingStampverWithDecrementPatchCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMinorCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "minor", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.2.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.2.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.2.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMajorCommandWithVerbose_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "major", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"0.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"0.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"0.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"0.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementButIncorrectVersionNumberPartCommandWithVerbose_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "incorrect", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
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
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"5.6.7\")]");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyFileVersion(\"5.6.7\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"5.6.7\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"5.6.7\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"5.6.7\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Changed: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"5.6.7\")]");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButInvalidVersionNumberWithVerbose_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "THIS.IS.NOT.A.VERSION.NUMBER", "--verbose" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number specified");
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
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.1.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementBuildCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "build", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.1\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.1.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMinorCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "minor", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.4.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.4.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.1.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.1.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementMajorCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "major", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"2.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"2.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"2.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"2.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithIncrementButIncorrectVersionNumberPartCommandWithDryrun_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "incorrect", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
        }
        #endregion

        #region Decrement version number tests with dryrun
        [Test]
        public void CallingStampverWithDecrementPatchCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "patch", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementBuildCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "build", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMinorCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "minor", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"1.2.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"1.2.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"1.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementMajorCommandWithDryrun_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "major", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"0.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"0.3.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"0.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"0.0.0.0\")]");
        }

        [Test]
        public void CallingStampverWithDecrementButIncorrectVersionNumberPartCommandWithDryrun_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-d", "incorrect", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
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
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File1");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File2");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Processing file: File3");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.3.0\")] to [assembly: AssemblyVersion(\"5.6.7\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.3.0\")] to [assembly: AssemblyFileVersion(\"5.6.7\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyVersion(\"1.0.0.0\")] to [assembly: AssemblyVersion(\"5.6.7\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Would change: [assembly: AssemblyFileVersion(\"1.0.0.0\")] to [assembly: AssemblyFileVersion(\"5.6.7\")]");
        }

        [Test]
        public void CallingStampverWithExplicitCommandButInvalidVersionNumberWithDryrun_OutputsErrorText()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-e", "THIS.IS.NOT.A.VERSION.NUMBER", "--dryrun" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(fakeIOWrapper.FileLinesOutput.Count == 0);
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number specified");
        }
        #endregion
    }
}
