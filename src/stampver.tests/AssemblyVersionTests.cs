using System;
using NUnit.Framework;

namespace stampver.Tests
{
    [TestFixture]
    public class AssemblyVersionTests
    {
        #region Constructor tests
        [TestCase("")]
        [TestCase("1")]
        [TestCase("1.2")]
        public void Constructor_WithFewerThanThreeParts_ThrowsArgumentException(string versionString)
        {
            // The Stampver regex always extracts at least three dot-separated parts,
            // so this guard is unreachable through the normal pipeline. This direct
            // unit test pins the contract for any future caller that bypasses the
            // regex (e.g. a programmatic API consumer).
            var ex = Assert.Throws<ArgumentException>(() => new AssemblyVersion(versionString));
            Assert.That(ex.Message, Is.EqualTo("versionString does not contain at least three parts."));
        }
        #endregion

        #region Decrement tests
        [Test]
        public void Decrement_WithPatchPartAndPositivePatch_DecrementsPatchByOne()
        {
            // Every default Stampver test fixture has patch = 0, so the actual
            // decrement line in DecrementPatch never executes through that path.
            // This test exercises the positive-patch branch directly.
            // Arrange
            var sut = new AssemblyVersion("1.2.5");

            // Act
            sut.Decrement(VersionNumberPart.Patch);

            // Assert
            Assert.That(sut.GetVersionString(), Is.EqualTo("1.2.4"));
        }
        #endregion
    }
}
