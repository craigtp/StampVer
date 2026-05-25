using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using stampver.Options;
using NUnit.Framework;

namespace stampver.Tests.Options
{
    // Pins the observable behaviour of NDesk.Options.OptionValueCollection.
    // The constructor is internal, so every test reaches the collection via
    // OptionContext.OptionValues — this is the same path the parser uses.
    [TestFixture]
    internal sealed class OptionValueCollectionTests
    {
        private static OptionContext NewContext()
        {
            return new OptionContext(new OptionSet());
        }

        private static (OptionContext context, OptionValueCollection values) WithBoundOption(
            string prototype = "name=", string optionName = "--name")
        {
            var set = new OptionSet { { prototype, _ => { } } };
            var context = new OptionContext(set) { Option = set.First(), OptionName = optionName };
            return (context, context.OptionValues);
        }

        #region Empty-state contract

        [Test]
        public void NewlyCreatedCollection_IsEmptyAndMutable()
        {
            var values = NewContext().OptionValues;

            Assert.That(values.Count, Is.EqualTo(0));
            Assert.That(values.IsReadOnly, Is.False);
        }

        [Test]
        public void NewlyCreatedCollection_AsIList_IsNotFixedSize()
        {
            // Pinning the IList legacy face — IsFixedSize is one of the few
            // members the typed IList<string> face doesn't expose.
            IList values = NewContext().OptionValues;

            Assert.That(values.IsFixedSize, Is.False);
        }

        #endregion

        #region Mutation API

        [Test]
        public void Add_AppendsItemAndIncrementsCount()
        {
            var values = NewContext().OptionValues;

            values.Add("first");
            values.Add("second");

            Assert.That(values.Count, Is.EqualTo(2));
            Assert.That(values.IndexOf("first"), Is.EqualTo(0));
            Assert.That(values.IndexOf("second"), Is.EqualTo(1));
        }

        [Test]
        public void Add_AcceptsNull()
        {
            // The parser explicitly stores null when an optional value is absent
            // (see ParseBool: "string v = n[n.Length-1] == '+' ? option : null").
            // Refactor must keep null-tolerance.
            var values = NewContext().OptionValues;

            values.Add(null!);

            Assert.That(values.Count, Is.EqualTo(1));
            Assert.That(values.Contains(null!), Is.True);
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            var values = NewContext().OptionValues;
            values.Add("a");
            values.Add("b");

            values.Clear();

            Assert.That(values.Count, Is.EqualTo(0));
        }

        [Test]
        public void Remove_ReturnsTrueWhenItemFoundAndFalseOtherwise()
        {
            var values = NewContext().OptionValues;
            values.Add("a");

            Assert.That(values.Remove("a"), Is.True);
            Assert.That(values.Remove("a"), Is.False);
            Assert.That(values.Count, Is.EqualTo(0));
        }

        [Test]
        public void Insert_PutsItemAtSpecifiedIndex()
        {
            var values = NewContext().OptionValues;
            values.Add("a");
            values.Add("c");

            values.Insert(1, "b");

            Assert.That(values.ToArray(), Is.EqualTo(new[] { "a", "b", "c" }));
        }

        [Test]
        public void RemoveAt_RemovesItemAtSpecifiedIndex()
        {
            var values = NewContext().OptionValues;
            values.Add("a");
            values.Add("b");
            values.Add("c");

            values.RemoveAt(1);

            Assert.That(values.ToArray(), Is.EqualTo(new[] { "a", "c" }));
        }

        [Test]
        public void Contains_ReturnsTrueOnlyForPreviouslyAddedItems()
        {
            var values = NewContext().OptionValues;
            values.Add("present");

            Assert.That(values.Contains("present"), Is.True);
            Assert.That(values.Contains("absent"), Is.False);
        }

        [Test]
        public void CopyTo_CopiesAllValuesIntoTargetArrayAtSpecifiedOffset()
        {
            var values = NewContext().OptionValues;
            values.Add("a");
            values.Add("b");
            var target = new string[4];

            values.CopyTo(target, 1);

            Assert.That(target, Is.EqualTo(new[] { null, "a", "b", null }));
        }

        #endregion

        #region Enumeration

        [Test]
        public void GetEnumerator_YieldsItemsInInsertionOrder()
        {
            var values = NewContext().OptionValues;
            values.Add("one");
            values.Add("two");
            values.Add("three");

            var collected = new List<string>();
            foreach (var v in values)
            {
                collected.Add(v);
            }

            Assert.That(collected, Is.EqualTo(new[] { "one", "two", "three" }));
        }

        #endregion

        #region Indexer — getter triggers AssertValid

        [Test]
        public void IndexerGetter_WithoutBoundOption_ThrowsInvalidOperationException()
        {
            var values = NewContext().OptionValues;
            values.Add("ignored");

            var ex = Assert.Throws<InvalidOperationException>(() => _ = values[0])!;
            Assert.That(ex.Message, Is.EqualTo("OptionContext.Option is null."));
        }

        [Test]
        public void IndexerGetter_BeyondMaxValueCount_ThrowsArgumentOutOfRangeException()
        {
            var (_, values) = WithBoundOption("name=");
            values.Add("v");

            // MaxValueCount for "name=" is 1, so index 1 trips the guard before
            // the values-list bounds check.
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = values[1]);
        }

        [Test]
        public void IndexerGetter_WithRequiredTypeAndMissingValue_ThrowsOptionException()
        {
            var (_, values) = WithBoundOption("name=", "--name");

            var ex = Assert.Throws<OptionException>(() => _ = values[0])!;
            Assert.That(ex.OptionName, Is.EqualTo("--name"));
            Assert.That(ex.Message, Is.EqualTo("Missing required value for option '--name'."));
        }

        [Test]
        public void IndexerGetter_WithOptionalTypeAndMissingValue_ReturnsNull()
        {
            // Optional-type options don't trip the OptionException path; the
            // documented behaviour is "return null past the end of values".
            var (_, values) = WithBoundOption("name:", "--name");

            Assert.That(values[0], Is.Null);
        }

        [Test]
        public void IndexerGetter_WithValidIndexBelowCount_ReturnsStoredValue()
        {
            var (_, values) = WithBoundOption("name=");
            values.Add("hello");

            Assert.That(values[0], Is.EqualTo("hello"));
        }

        [Test]
        public void IndexerSetter_DoesNotGoThroughAssertValid()
        {
            // Quirk: the setter delegates straight to the underlying List<string>
            // and skips AssertValid entirely. Pinning so the refactor decides
            // explicitly whether to keep this asymmetry.
            var values = NewContext().OptionValues;
            values.Add("original");

            values[0] = "replaced";

            // Reading back via ToArray bypasses the AssertValid path on get.
            Assert.That(values.ToArray()[0], Is.EqualTo("replaced"));
        }

        #endregion

        #region Snapshot helpers

        [Test]
        public void ToList_ReturnsACopyThatIsDecoupledFromTheCollection()
        {
            var values = NewContext().OptionValues;
            values.Add("a");
            values.Add("b");

            var snapshot = values.ToList();
            snapshot.Add("c");

            Assert.That(snapshot, Is.EqualTo(new[] { "a", "b", "c" }));
            Assert.That(values.Count, Is.EqualTo(2));
        }

        [Test]
        public void ToArray_ReturnsACopyThatIsDecoupledFromTheCollection()
        {
            var values = NewContext().OptionValues;
            values.Add("a");
            values.Add("b");

            var snapshot = values.ToArray();
            snapshot[0] = "mutated";

            Assert.That(values.ToArray()[0], Is.EqualTo("a"));
        }

        [Test]
        public void ToString_JoinsValuesWithCommaSpace()
        {
            var values = NewContext().OptionValues;
            values.Add("one");
            values.Add("two");
            values.Add("three");

            Assert.That(values.ToString(), Is.EqualTo("one, two, three"));
        }

        [Test]
        public void ToString_OnEmptyCollection_ReturnsEmptyString()
        {
            var values = NewContext().OptionValues;

            Assert.That(values.ToString(), Is.EqualTo(string.Empty));
        }

        #endregion
    }
}
