using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace stampver.Tests
{
    // Exercises the REAL IoWrapper against a temporary directory. This is the only
    // fixture that touches disk — it has to, because newline/encoding preservation
    // (code review #3) is a property of the real filesystem writer, which the
    // FakeIOWrapper deliberately does not model. Each test cleans up after itself.
    [TestFixture]
    internal sealed class IOWrapperTests
    {
        private string _tempDir = string.Empty;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "stampver-iotests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, recursive: true);
            }
        }

        [Test]
        public void WriteFileLinesToFile_PreservesLfAndNoTrailingNewline()
        {
            // Arrange: an LF file with NO trailing newline — common in cross-platform repos
            // with .gitattributes enforcing LF. The old File.WriteAllLines would have rewritten
            // it entirely as CRLF (on Windows) and appended a trailing newline.
            var file = Path.Combine(_tempDir, "AssemblyInfo.cs");
            File.WriteAllText(file, "line1\nline2\nline3", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var sut = new IoWrapper();
            var lines = sut.ReadAllLinesFromFile(file);
            lines[1] = "CHANGED";

            // Act
            sut.WriteFileLinesToFile(lines, file);

            // Assert: still LF, still no trailing newline.
            Assert.That(File.ReadAllText(file), Is.EqualTo("line1\nCHANGED\nline3"));
        }

        [Test]
        public void WriteFileLinesToFile_PreservesCrlfAndTrailingNewline()
        {
            // Arrange: a CRLF file WITH a trailing newline.
            var file = Path.Combine(_tempDir, "AssemblyInfo.cs");
            File.WriteAllText(file, "line1\r\nline2\r\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            var sut = new IoWrapper();
            var lines = sut.ReadAllLinesFromFile(file);
            lines[0] = "CHANGED";

            // Act
            sut.WriteFileLinesToFile(lines, file);

            // Assert: still CRLF, trailing newline kept.
            Assert.That(File.ReadAllText(file), Is.EqualTo("CHANGED\r\nline2\r\n"));
        }

        [Test]
        public void WriteFileLinesToFile_PreservesUtf8Bom()
        {
            // Arrange: a UTF-8-with-BOM file. The wrapper detects and re-emits the BOM, and the
            // newline handling must not disturb that preamble.
            var file = Path.Combine(_tempDir, "AssemblyInfo.cs");
            File.WriteAllText(file, "line1\r\nline2\r\n", new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            var sut = new IoWrapper();
            var lines = sut.ReadAllLinesFromFile(file);
            lines[0] = "CHANGED";

            // Act
            sut.WriteFileLinesToFile(lines, file);

            // Assert: the first three bytes are still the UTF-8 BOM (EF BB BF).
            var bytes = File.ReadAllBytes(file);
            Assert.That(bytes.Length, Is.GreaterThanOrEqualTo(3));
            Assert.That(bytes[0], Is.EqualTo(0xEF));
            Assert.That(bytes[1], Is.EqualTo(0xBB));
            Assert.That(bytes[2], Is.EqualTo(0xBF));
            Assert.That(File.ReadAllText(file), Is.EqualTo("CHANGED\r\nline2\r\n"));
        }
    }
}
