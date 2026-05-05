using System;
using System.Linq;
using NDesk.Options;
using NUnit.Framework;

namespace stampver.Tests.Options
{
    // Pins every Add overload, KeyedCollection-derived lookup behaviour, the
    // localizer plumbing, and the typed Add<T>/Add<TKey,TValue> conversion paths.
    // The cross-culture tests near the bottom highlight the locale-coupling
    // baked into TypeDescriptor.GetConverter — exactly the kind of hidden
    // behaviour the refactor will need to make explicit.
    [TestFixture]
    internal sealed class OptionSetAddTests
    {
        #region Constructors and localizer

        [Test]
        public void DefaultConstructor_ProvidesIdentityLocalizer()
        {
            var set = new OptionSet();

            // The default localizer round-trips its input unchanged. Pin so the
            // refactor doesn't accidentally substitute a "translate" stub.
            Assert.That(set.MessageLocalizer("hello"), Is.EqualTo("hello"));
            Assert.That(set.MessageLocalizer(string.Empty), Is.EqualTo(string.Empty));
        }

        [Test]
        public void CustomLocalizerConstructor_ExposesTheSameDelegateInstance()
        {
            Converter<string, string> localizer = s => $"[{s}]";

            var set = new OptionSet(localizer);

            Assert.That(set.MessageLocalizer, Is.SameAs(localizer));
            Assert.That(set.MessageLocalizer("x"), Is.EqualTo("[x]"));
        }

        #endregion

        #region Add(Option)

        [Test]
        public void AddOption_ReturnsSameOptionSetToEnableChaining()
        {
            var set = new OptionSet();
            var first = MakeOption("a");
            var second = MakeOption("b");

            var returned = set.Add(first).Add(second);

            Assert.That(returned, Is.SameAs(set));
            Assert.That(set.Count, Is.EqualTo(2));
        }

        [Test]
        public void AddOption_WithNull_Throws()
        {
            var set = new OptionSet();

            // KeyedCollection allows null insertion at the base; OptionSet's
            // override (AddImpl) is what catches it. Pin so the refactor doesn't
            // drop the null-guard and leak nulls into the collection.
            Assert.Throws<ArgumentNullException>(() => set.Add((Option)null!));
        }

        [Test]
        public void AddOption_RegistersEveryAliasInTheKeyedCollection()
        {
            var set = new OptionSet();
            var option = MakeOption("h|?|help");

            set.Add(option);

            Assert.That(set.Contains("h"), Is.True);
            Assert.That(set.Contains("?"), Is.True);
            Assert.That(set.Contains("help"), Is.True);
            Assert.That(set["h"], Is.SameAs(option));
            Assert.That(set["?"], Is.SameAs(option));
            Assert.That(set["help"], Is.SameAs(option));
        }

        [Test]
        public void AddOption_ContainsForUnknownName_ReturnsFalse()
        {
            var set = new OptionSet { MakeOption("known") };

            Assert.That(set.Contains("unknown"), Is.False);
        }

        #endregion

        #region Add(prototype, Action<string>) — single-value untyped

        [Test]
        public void AddPrototypeAction_RegistersOptionWithMaxValueCountOne()
        {
            var set = new OptionSet { { "name=", _ => { } } };

            var option = set["name"];
            Assert.That(option.MaxValueCount, Is.EqualTo(1));
            Assert.That(option.OptionValueType, Is.EqualTo(OptionValueType.Required));
        }

        [Test]
        public void AddPrototypeAction_DefaultsDescriptionToNull()
        {
            var set = new OptionSet { { "name=", _ => { } } };

            Assert.That(set["name"].Description, Is.Null);
        }

        [Test]
        public void AddPrototypeDescriptionAction_PreservesDescription()
        {
            var set = new OptionSet { { "name=", "the name", _ => { } } };

            Assert.That(set["name"].Description, Is.EqualTo("the name"));
        }

        [Test]
        public void AddPrototypeAction_WithNullAction_Throws()
        {
            var set = new OptionSet();

            Assert.Throws<ArgumentNullException>(() => set.Add("name=", (Action<string>)null!));
        }

        [Test]
        public void AddPrototypeAction_InvokesActionDuringParse()
        {
            string? captured = null;
            var set = new OptionSet { { "name=", v => captured = v } };

            set.Parse(new[] { "--name=value" });

            Assert.That(captured, Is.EqualTo("value"));
        }

        #endregion

        #region Add(prototype, OptionAction<string,string>) — two-value untyped

        [Test]
        public void AddPrototypeOptionAction_RegistersOptionWithMaxValueCountTwo()
        {
            var set = new OptionSet { { "p=", (k, v) => { _ = k; _ = v; } } };

            Assert.That(set["p"].MaxValueCount, Is.EqualTo(2));
        }

        [Test]
        public void AddPrototypeOptionAction_WithNullAction_Throws()
        {
            var set = new OptionSet();

            Assert.Throws<ArgumentNullException>(() => set.Add("p=", (OptionAction<string, string>)null!));
        }

        [Test]
        public void AddPrototypeOptionAction_ReceivesBothValuesDuringParse()
        {
            string? key = null;
            string? value = null;
            var set = new OptionSet { { "D=", (k, v) => { key = k; value = v; } } };

            set.Parse(new[] { "-Dkey=val" });

            Assert.That(key, Is.EqualTo("key"));
            Assert.That(value, Is.EqualTo("val"));
        }

        #endregion

        #region Add<T> — typed conversions

        [Test]
        public void AddTypedAction_ConvertsStringToTargetTypeViaTypeConverter()
        {
            int captured = 0;
            var set = new OptionSet { { "count=", (int v) => captured = v } };

            set.Parse(new[] { "--count=42" });

            Assert.That(captured, Is.EqualTo(42));
        }

        [Test]
        public void AddTypedAction_OnConversionFailure_ThrowsOptionExceptionWithInnerException()
        {
            var set = new OptionSet { { "count=", (int _) => { } } };

            var ex = Assert.Throws<OptionException>(() => set.Parse(new[] { "--count=notanumber" }))!;
            Assert.That(ex.OptionName, Is.EqualTo("--count"));
            Assert.That(ex.InnerException, Is.Not.Null,
                "Pin: the conversion exception is preserved as the InnerException so callers can diagnose root cause.");
            Assert.That(ex.Message, Does.Contain("Could not convert"));
        }

        [Test]
        public void AddTypedAction_WithNullAction_Throws()
        {
            // The wrapping ActionOption<T> ctor enforces non-null action.
            var set = new OptionSet();

            Assert.Throws<ArgumentNullException>(() => set.Add("count=", (Action<int>)null!));
        }

        [Test]
        public void AddTypedOptionAction_ReceivesBothValuesConvertedToTargetTypes()
        {
            string? capturedKey = null;
            int capturedValue = 0;
            var set = new OptionSet { { "p=", (string k, int v) => { capturedKey = k; capturedValue = v; } } };

            set.Parse(new[] { "-pkey=7" });

            Assert.That(capturedKey, Is.EqualTo("key"));
            Assert.That(capturedValue, Is.EqualTo(7));
        }

        #endregion

        #region Cross-culture quirk pinning

        [Test]
        [SetCulture("tr-TR")]
        public void AddTypedAction_DoubleConversion_UsesCurrentCultureForParsing()
        {
            // QUIRK: Parse<T> calls TypeConverter.ConvertFromString without an
            // explicit CultureInfo, which defaults to CultureInfo.CurrentCulture.
            // In tr-TR the decimal separator is ',', so "1,5" parses as 1.5.
            // In invariant culture the same input would either fail or be
            // mis-parsed. The modernised parser should make this an explicit
            // choice (most likely InvariantCulture) — pinning the current
            // locale-coupled behaviour so the swap is visible.
            double captured = 0;
            var set = new OptionSet { { "ratio=", (double v) => captured = v } };

            set.Parse(new[] { "--ratio=1,5" });

            Assert.That(captured, Is.EqualTo(1.5));
        }

        #endregion

        #region KeyedCollection lookup and removal

        [Test]
        public void Indexer_ForUnknownKey_ThrowsKeyNotFoundException()
        {
            var set = new OptionSet();

            // Inherited from KeyedCollection — pin so callers (or the refactor)
            // know the indexer doesn't return null on a miss.
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _ = set["missing"]);
        }

        [Test]
        public void Remove_OnSoleOptionInSet_ThrowsArgumentOutOfRangeException_KnownBug()
        {
            // KNOWN BUG. OptionSet.RemoveItem(index) calls base.RemoveItem(index)
            // FIRST and then dereferences Items[index] to find the alias names —
            // but the item has already been removed, so Items[index] is either
            // out of range (this case) or the wrong option (test below).
            // Pinning the current observable behaviour so the modernisation
            // refactor surfaces an explicit fix (capture names BEFORE removal).
            var set = new OptionSet { { "h|?|help", _ => { } } };
            var option = set["h"];

            Assert.Throws<ArgumentOutOfRangeException>(() => set.Remove(option));
        }

        [Test]
        public void Remove_OptionWithAliasesAndTrailingItem_LeavesStaleAliasEntries_KnownBug()
        {
            // Continuation of the bug pinned above: when a following option
            // exists, RemoveItem reads its Names instead of the removed option's,
            // so the original option's alias entries stay live in the lookup
            // dictionary. KeyedCollection still removes the FIRST name correctly
            // because that path runs inside base.RemoveItem before the broken
            // post-call dereference.
            var set = new OptionSet
            {
                { "h|?|help", _ => { } },
                { "v", _ => { } },
            };
            var helpOption = set["h"];

            set.Remove(helpOption);

            Assert.That(set.Contains("h"), Is.False, "first name is correctly removed by base.RemoveItem.");
            Assert.That(set.Contains("?"), Is.True, "stale alias — refactor target.");
            Assert.That(set.Contains("help"), Is.True, "stale alias — refactor target.");
        }

        [Test]
        public void Remove_AliaslessOptionWithTrailingItem_RemovesCleanly()
        {
            // Sanity check: when the removed option has no aliases, the buggy
            // post-removal dereference walks an empty loop body, so the dictionary
            // ends up consistent. This is the only Remove path that currently works.
            var set = new OptionSet
            {
                { "first", _ => { } },
                { "second", _ => { } },
            };
            var firstOption = set["first"];

            set.Remove(firstOption);

            Assert.That(set.Contains("first"), Is.False);
            Assert.That(set.Contains("second"), Is.True);
        }

        [Test]
        public void Add_DuplicateAliasAcrossOptions_Throws()
        {
            var set = new OptionSet { { "name=", _ => { } } };

            // Pin: alias collisions are surfaced eagerly, not lazily at parse time.
            // The exact exception type isn't documented — pinning that *something*
            // throws so the modernised implementation has a freedom-versus-contract
            // checkpoint here.
            Assert.That(() => set.Add("name=", _ => { }), Throws.Exception);
        }

        [Test]
        public void OptionSet_IsEnumerableInInsertionOrder()
        {
            var set = new OptionSet
            {
                { "a", _ => { } },
                { "b", _ => { } },
                { "c", _ => { } },
            };

            // OptionSet inherits IEnumerable<Option> from Collection<T>; the
            // WriteOptionDescriptions code path depends on insertion order.
            Assert.That(set.Select(o => o.Prototype).ToArray(), Is.EqualTo(new[] { "a", "b", "c" }));
        }

        #endregion

        private static ActionOption MakeOption(string prototype) => new(prototype);

        // Tiny test-only Option subclass to feed Add(Option). The action-based
        // Add overloads cover the everyday paths; this exists purely to give the
        // Option-based overload coverage without duplicating a full subclass per file.
        private sealed class ActionOption : Option
        {
            public ActionOption(string prototype) : base(prototype, null!) { }
            protected override void OnParseComplete(OptionContext c) { }
        }
    }
}
