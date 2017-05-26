using System.Collections.Generic;
using NUnit.Framework;
// ReSharper disable InconsistentNaming

namespace stampver.Tests
{
    [TestFixture]
    public class stampverTests
    {
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Quiet and Verbose options are mutually exclusive!");
        }

        // ==========================================================================================================================
        // Increment version number tests
        // ==========================================================================================================================
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.1.0 (2 occurences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.3.1 (4 occurences in 2 files)");
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.1.0 (2 occurences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.3.1 (4 occurences in 2 files)");
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.1.0.0 (2 occurences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.1.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.4.0 (4 occurences in 2 files)");
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "2.0.0.0 (2 occurences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"2.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "2.0.0 (4 occurences in 2 files)");
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
        }

        // ==========================================================================================================================
        // Decrement version number tests
        // ==========================================================================================================================
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.0.0 (2 occurences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.3.0 (4 occurences in 2 files)");
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.0.0 (2 occurences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.3.0 (4 occurences in 2 files)");
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.0.0.0 (2 occurences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "1.2.0 (4 occurences in 2 files)");
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "0.0.0.0 (2 occurences in 1 file)");
            TestHelpers.AssertContains(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"0.0.0.0\")]");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "0.3.0 (4 occurences in 2 files)");
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number part specified");
        }

        // ==========================================================================================================================
        // Explicit version number tests
        // ==========================================================================================================================
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "5.6.7 (6 occurences in 3 files)");
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
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "error:");
            TestHelpers.AssertContains(fakeIOWrapper.StdOutputLines, "Invalid version number specified");
        }

    }
}
