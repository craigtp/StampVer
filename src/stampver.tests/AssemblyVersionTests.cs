using System;
using NUnit.Framework;

namespace stampver.Tests
{
    [TestFixture]
    internal sealed class AssemblyVersionTests
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
            var ex = Assert.Throws<ArgumentException>(() => new AssemblyVersion(versionString))!;
            Assert.That(ex.Message, Is.EqualTo("versionString does not contain at least three parts."));
        }

        [Test]
        public void Constructor_WithAsteriskInPart_PreservesOriginalTokenInVersionString()
        {
            // CLAUDE.md flags this as load-bearing: when a part fails int.TryParse,
            // GetVersionString falls back to the original token so that "*" survives
            // round-tripping for files that use [assembly: AssemblyVersion("1.0.*")].
            // Arrange
            var sut = new AssemblyVersion("1.0.*");

            // Act
            var result = sut.GetVersionString();

            // Assert
            Assert.That(result, Is.EqualTo("1.0.*"));
        }
        #endregion

        #region Increment tests
        [Test]
        public void Increment_WithMajorPartAndAsteriskPatch_PreservesAsteriskAndResetsMinor()
        {
            // The cascade resets in IncrementMajor are guarded by null-checks on each
            // sibling. If those guards were removed, an unparseable part (here, "*")
            // would be silently overwritten with "0". This test pins the preservation.
            // Arrange
            var sut = new AssemblyVersion("1.5.*");

            // Act
            sut.Increment(VersionNumberPart.Major);

            // Assert
            Assert.That(sut.GetVersionString(), Is.EqualTo("2.0.*"));
        }

        [Test]
        public void Increment_WithMinorPartAndPositivePatch_ResetsPatchToZero()
        {
            // Pins the IncrementMinor cascade rule. Every default Stampver fixture
            // has patch=0, so the reset is invisible through the pipeline tests —
            // a regression that dropped the cascade reset on minor would not be
            // caught at the integration level.
            // Arrange
            var sut = new AssemblyVersion("1.5.10");

            // Act
            sut.Increment(VersionNumberPart.Minor);

            // Assert
            Assert.That(sut.GetVersionString(), Is.EqualTo("1.6.0"));
        }

        [Test]
        public void Increment_WithMajorPartAnd4PartVersion_DoesNotResetRevision()
        {
            // Pins the cascade upper-bound at index 2 (patch). Revision is at
            // index 3 and must NOT be reset, even though it's a numeric part —
            // historical AssemblyVersion semantics. Default 4-part fixture has
            // revision=0, so this contract isn't observable via the pipeline tests.
            // Arrange
            var sut = new AssemblyVersion("1.5.10.20");

            // Act
            sut.Increment(VersionNumberPart.Major);

            // Assert
            Assert.That(sut.GetVersionString(), Is.EqualTo("2.0.0.20"));
        }

        [TestCase("65535.5.10", VersionNumberPart.Major)]
        [TestCase("5.65535.10", VersionNumberPart.Minor)]
        [TestCase("5.10.65535", VersionNumberPart.Patch)]
        public void Increment_WhenPartAtUInt16MaxValue_LeavesAllPartsUnchanged(string input, VersionNumberPart part)
        {
            // Each Increment* method clamps via "_xxxInt < UInt16.MaxValue" and the
            // cascade resets sit INSIDE that guard. A regression that moves the
            // resets outside the guard would silently zero the sibling parts even
            // though the targeted part didn't change. The non-clamp parts in the
            // input are deliberately non-zero so this regression would be caught.
            // Arrange
            var sut = new AssemblyVersion(input);

            // Act
            sut.Increment(part);

            // Assert
            Assert.That(sut.GetVersionString(), Is.EqualTo(input));
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

        [Test]
        public void Decrement_WithMinorPartAndPositivePatch_PreservesPatch()
        {
            // Pins the no-cascade contract for decrement. Decrement must NEVER
            // reset siblings, even when the targeted part is reduced. Every
            // default fixture has patch=0, so a regression that introduced a
            // cascade reset on decrement-minor would be invisible at the
            // pipeline level.
            // Arrange
            var sut = new AssemblyVersion("1.5.10");

            // Act
            sut.Decrement(VersionNumberPart.Minor);

            // Assert
            Assert.That(sut.GetVersionString(), Is.EqualTo("1.4.10"));
        }

        [Test]
        public void Decrement_WithMajorPartAndMajorAlreadyZero_LeavesMajorAtZero()
        {
            // No default Stampver fixture has major = 0, so the "_majorInt > 0"
            // guard's no-op branch is otherwise untested. (The analogous guards
            // for minor and patch are exercised by the existing fixtures.)
            // Arrange
            var sut = new AssemblyVersion("0.5.10");

            // Act
            sut.Decrement(VersionNumberPart.Major);

            // Assert
            Assert.That(sut.GetVersionString(), Is.EqualTo("0.5.10"));
        }
        #endregion
    }
}
