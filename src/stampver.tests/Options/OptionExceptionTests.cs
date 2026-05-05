using System;
using NDesk.Options;
using NUnit.Framework;

namespace stampver.Tests.Options
{
    // Pins the observable behaviour of NDesk.Options.OptionException ahead of a
    // planned modernisation of Options.cs. Serialization (the protected
    // SerializationInfo ctor and overridden GetObjectData) is intentionally NOT
    // covered: BinaryFormatter is removed in modern .NET and the SYSLIB0051
    // diagnostic on those members is already suppressed at the main-project
    // level. The refactor is expected to drop them entirely.
    [TestFixture]
    internal sealed class OptionExceptionTests
    {
        [Test]
        public void DefaultConstructor_LeavesOptionNameNullAndNoInnerException()
        {
            var ex = new OptionException();

            Assert.That(ex.OptionName, Is.Null);
            Assert.That(ex.InnerException, Is.Null);
        }

        [Test]
        public void MessageAndOptionNameConstructor_PopulatesBothAndLeavesInnerExceptionNull()
        {
            var ex = new OptionException("Missing required value for option '--foo'.", "--foo");

            Assert.That(ex.Message, Is.EqualTo("Missing required value for option '--foo'."));
            Assert.That(ex.OptionName, Is.EqualTo("--foo"));
            Assert.That(ex.InnerException, Is.Null);
        }

        [Test]
        public void FullConstructor_PopulatesMessageOptionNameAndInnerException()
        {
            var inner = new InvalidOperationException("conversion failed");

            var ex = new OptionException("Could not convert value.", "--count", inner);

            Assert.That(ex.Message, Is.EqualTo("Could not convert value."));
            Assert.That(ex.OptionName, Is.EqualTo("--count"));
            Assert.That(ex.InnerException, Is.SameAs(inner));
        }

        [Test]
        public void Constructor_AcceptsNullOptionName()
        {
            // The library uses string.Empty for "no option" in some Parse paths but
            // accepts a null OptionName too. Refactor must preserve the null-tolerant
            // contract — callers (including stampver itself) pass string.Empty here,
            // but third-party callers may pass null.
            var ex = new OptionException("boom", optionName: null!);

            Assert.That(ex.OptionName, Is.Null);
        }

        [Test]
        public void OptionException_IsThrowableAndCarriesOptionNameThroughCatch()
        {
            // End-to-end: throw and catch as Exception, downcast, read OptionName.
            // Pins the production usage pattern in Stampver.TryParseArguments.
            try
            {
                throw new OptionException("nope", "-x");
            }
            catch (OptionException caught)
            {
                Assert.That(caught.Message, Is.EqualTo("nope"));
                Assert.That(caught.OptionName, Is.EqualTo("-x"));
            }
        }

        [Test]
        public void OptionException_IsAssignableToException()
        {
            // Pins the public type hierarchy. The catch in Stampver.TryParseArguments
            // narrows on OptionException specifically; downstream consumers may catch
            // System.Exception. Both must continue to work post-refactor.
            Assert.That(new OptionException(), Is.InstanceOf<Exception>());
        }
    }
}
