using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "error:"));
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "must specify a valid version number command"));
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
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "stampver by Craig Phillips <craig@craigtp.co.uk>"));
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "A small command-line utility"));
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "Usage"));
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
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "error:"));
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "Missing required value for option '-i'."));
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
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "error:"));
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "Missing required value for option '-d'."));
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
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "error:"));
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "Missing required value for option '-e'."));
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
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "error:"));
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "must specify a valid version number command"));
        }

        [Test]
        public void CallingStampverWithIncrementMinorCommand_IncrementsAndOutputsNewVersion()
        {
            // Arrange
            var fakeIOWrapper = new FakeIOWrapper();
            var sut = new Stampver(fakeIOWrapper, new[] { "-i", "build" });

            // Act
            sut.Run();

            // Assert
            Assert.True(fakeIOWrapper.StdOutputLines.Count > 0);
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "1.0.1.0"));
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.0.1.0\")]"));

            // TODO:  Looks as though the Console output doesn't output all version numbers that have changed.  i.e. If it updates
            // two files and each file has two different version numbers, it'll update the versions correctly (eg. 1.0.0 updates to
            // 1.0.1 and 1.3.0 updates to 1.3.1) however, it only outputs the last updated version number to the console (eg. 1.3.1).
            // Need to improve the functionality of Stampver to potentially show all vesion number that have been updated but then
            // might need to display filenames too?

            //Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.StdOutputLines, "1.3.1"));
            Assert.True(TestHelpers.ListContainsSubstring(fakeIOWrapper.FileLinesOutput, "[assembly: AssemblyVersion(\"1.3.1\")]"));
        }
    }
}
