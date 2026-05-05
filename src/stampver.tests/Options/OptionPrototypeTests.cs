using System;
using System.Collections.Generic;
using NDesk.Options;
using NUnit.Framework;

namespace stampver.Tests.Options
{
    // Pins the Option ctor validation guards and ParsePrototype behaviour.
    // Option is abstract, so every test constructs CapturingOption — a minimal
    // subclass that records what OnParseComplete sees, which doubles for the
    // Invoke() lifecycle assertions at the end of the file.
    [TestFixture]
    internal sealed class OptionPrototypeTests
    {
        private sealed class CapturingOption : Option
        {
            public CapturingOption(string prototype) : base(prototype, null!) { }
            public CapturingOption(string prototype, string description) : base(prototype, description) { }
            public CapturingOption(string prototype, string description, int maxValueCount)
                : base(prototype, description, maxValueCount) { }

            public int InvokeCount;
            public string? CapturedOptionName;
            public Option? CapturedOption;
            public readonly List<string> CapturedValues = new();

            protected override void OnParseComplete(OptionContext c)
            {
                InvokeCount++;
                CapturedOptionName = c.OptionName;
                CapturedOption = c.Option;
                CapturedValues.AddRange(c.OptionValues.ToList());
            }
        }

        #region Constructor argument validation

        [Test]
        public void Constructor_WithNullPrototype_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CapturingOption(null!));
        }

        [Test]
        public void Constructor_WithEmptyPrototype_ThrowsArgumentExceptionWithDescriptiveMessage()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption(string.Empty))!;
            Assert.That(ex.Message, Does.StartWith("Cannot be the empty string."));
            Assert.That(ex.ParamName, Is.EqualTo("prototype"));
        }

        [Test]
        public void Constructor_WithNegativeMaxValueCount_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CapturingOption("name", null!, -1));
        }

        [Test]
        public void Constructor_WithMaxValueCountZeroAndRequiredType_Throws()
        {
            // Pinning the friendly message — refactor must keep the diagnostic legible.
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("name=", null!, 0))!;
            Assert.That(ex.Message, Does.Contain("Cannot provide maxValueCount of 0"));
        }

        [Test]
        public void Constructor_WithMaxValueCountZeroAndOptionalType_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("name:", null!, 0))!;
            Assert.That(ex.Message, Does.Contain("Cannot provide maxValueCount of 0"));
        }

        [Test]
        public void Constructor_WithMaxValueCountZeroAndNoType_Succeeds()
        {
            // count=0 + None type is the one combination NDesk allows.
            var option = new CapturingOption("name", null!, 0);

            Assert.That(option.MaxValueCount, Is.EqualTo(0));
            Assert.That(option.OptionValueType, Is.EqualTo(OptionValueType.None));
        }

        [Test]
        public void Constructor_WithNoTypeAndMaxValueCountGreaterThanOne_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("name", null!, 2))!;
            Assert.That(ex.Message, Does.Contain("Cannot provide maxValueCount of 2 for OptionValueType.None"));
        }

        #endregion

        #region Default handler "<>" rules

        [Test]
        public void Constructor_WithSoloDefaultHandlerAndRequiredType_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("<>="))!;
            Assert.That(ex.Message, Does.Contain("default option handler '<>' cannot require values"));
        }

        [Test]
        public void Constructor_WithSoloDefaultHandlerAndOptionalType_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CapturingOption("<>:"));
        }

        [Test]
        public void Constructor_WithSoloDefaultHandlerAndNoType_Succeeds()
        {
            // "<>" alone is a valid no-value handler — the Parse path uses it as
            // the destination for unmatched arguments.
            var option = new CapturingOption("<>");

            Assert.That(option.GetNames(), Is.EqualTo(new[] { "<>" }));
            Assert.That(option.OptionValueType, Is.EqualTo(OptionValueType.None));
        }

        [Test]
        public void Constructor_WithDefaultHandlerAliasAndSingleValue_Succeeds()
        {
            // "<>" can co-exist with a sibling alias provided the option only
            // takes a single value.
            var option = new CapturingOption("<>|name=", null!, 1);

            Assert.That(option.GetNames(), Is.EqualTo(new[] { "<>", "name" }));
            Assert.That(option.MaxValueCount, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_WithDefaultHandlerAliasAndMultipleValues_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("<>|name=", null!, 2))!;
            Assert.That(ex.Message, Does.Contain("default option handler '<>' cannot require values"));
        }

        #endregion

        #region Prototype parsing — types and aliases

        [Test]
        public void Constructor_PlainNameWithNoTerminator_ProducesNoneType()
        {
            var option = new CapturingOption("verbose");

            Assert.That(option.GetNames(), Is.EqualTo(new[] { "verbose" }));
            Assert.That(option.OptionValueType, Is.EqualTo(OptionValueType.None));
        }

        [Test]
        public void Constructor_NameEndingInEquals_ProducesRequiredType()
        {
            var option = new CapturingOption("name=");

            Assert.That(option.GetNames(), Is.EqualTo(new[] { "name" }));
            Assert.That(option.OptionValueType, Is.EqualTo(OptionValueType.Required));
        }

        [Test]
        public void Constructor_NameEndingInColon_ProducesOptionalType()
        {
            var option = new CapturingOption("name:");

            Assert.That(option.GetNames(), Is.EqualTo(new[] { "name" }));
            Assert.That(option.OptionValueType, Is.EqualTo(OptionValueType.Optional));
        }

        [Test]
        public void Constructor_PipeDelimitedAliases_StripsTerminatorFromEachName()
        {
            var option = new CapturingOption("h|?|help");

            Assert.That(option.GetNames(), Is.EqualTo(new[] { "h", "?", "help" }));
            Assert.That(option.OptionValueType, Is.EqualTo(OptionValueType.None));
        }

        [Test]
        public void Constructor_TypeOnSingleAliasAppliesToWholeOption()
        {
            // The terminator only needs to appear on one alias for the option
            // to be Required/Optional. This is documented behaviour.
            var option = new CapturingOption("a|b=");

            Assert.That(option.GetNames(), Is.EqualTo(new[] { "a", "b" }));
            Assert.That(option.OptionValueType, Is.EqualTo(OptionValueType.Required));
        }

        [Test]
        public void Constructor_ConsistentTypeOnMultipleAliasesIsAccepted()
        {
            var option = new CapturingOption("a=|b=");

            Assert.That(option.OptionValueType, Is.EqualTo(OptionValueType.Required));
        }

        [Test]
        public void Constructor_ConflictingTypesAcrossAliases_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("a=|b:"))!;
            Assert.That(ex.Message, Does.Contain("Conflicting option types"));
        }

        [Test]
        public void Constructor_EmptyAliasInMiddle_Throws()
        {
            // "a||b" splits into ["a", "", "b"] — the empty alias is rejected.
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("a||b"))!;
            Assert.That(ex.Message, Does.StartWith("Empty option names are not supported."));
        }

        [Test]
        public void Constructor_PrototypeOfJustPipe_Throws()
        {
            // "|" splits into ["", ""] — both aliases are empty.
            Assert.Throws<ArgumentException>(() => new CapturingOption("|"));
        }

        [Test]
        public void Constructor_WhitespaceOnlyPrototype_IsAccepted()
        {
            // Quirk: " " has length 1, so the empty-string guard doesn't fire.
            // The space becomes the option's literal name. Pinning so the
            // refactor decides whether to keep this loophole.
            var option = new CapturingOption(" ");

            Assert.That(option.GetNames(), Is.EqualTo(new[] { " " }));
        }

        #endregion

        #region Prototype parsing — separators

        [Test]
        public void Constructor_SeparatorWithMaxValueCountOne_Throws()
        {
            // Separators are only meaningful when an option takes >1 value.
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("name=,"))!;
            Assert.That(ex.Message, Does.Contain("Cannot provide key/value separators"));
        }

        [Test]
        public void Constructor_DefaultSeparatorsApplyWhenNoneSpecifiedAndMaxValueCountGreaterThanOne()
        {
            var option = new CapturingOption("name=", null!, 2);

            Assert.That(option.GetValueSeparators(), Is.EqualTo(new[] { ":", "=" }));
        }

        [Test]
        public void Constructor_SinglePunctuationSeparator_IsCapturedVerbatim()
        {
            var option = new CapturingOption("name=,", null!, 2);

            Assert.That(option.GetValueSeparators(), Is.EqualTo(new[] { "," }));
        }

        [Test]
        public void Constructor_MultiCharSeparatorViaCurlyBraces_IsCapturedAsSingleSeparator()
        {
            // "{,;}" is ONE separator literal ",;", not two single-char separators.
            // This is the only way to declare a multi-character separator.
            var option = new CapturingOption("name={,;}", null!, 2);

            Assert.That(option.GetValueSeparators(), Is.EqualTo(new[] { ",;" }));
        }

        [Test]
        public void Constructor_EmptyCurlyBraceSeparator_DisablesSplittingEntirely()
        {
            // "{}" is the documented way to opt out of value splitting altogether
            // — values must be passed as separate argv tokens. The implementation
            // signals this by setting the internal separators field to null,
            // which surfaces as an empty array via GetValueSeparators.
            var option = new CapturingOption("name={}", null!, 2);

            Assert.That(option.GetValueSeparators(), Is.Empty);
        }

        [Test]
        public void Constructor_UnbalancedOpenBraceSeparator_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("name={", null!, 2))!;
            Assert.That(ex.Message, Does.Contain("Ill-formed name/value separator"));
        }

        [Test]
        public void Constructor_UnbalancedCloseBraceSeparator_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => new CapturingOption("name=}", null!, 2))!;
            Assert.That(ex.Message, Does.Contain("Ill-formed name/value separator"));
        }

        [Test]
        public void Constructor_NestedOpenBraceInSeparator_Throws()
        {
            Assert.Throws<ArgumentException>(() => new CapturingOption("name={a{b}", null!, 2));
        }

        #endregion

        #region Property accessors

        [Test]
        public void Prototype_ReturnsExactStringPassedToConstructor()
        {
            // Prototype is the raw input — terminators and separators stay attached,
            // even though Names strips them.
            var option = new CapturingOption("name=,", null!, 2);

            Assert.That(option.Prototype, Is.EqualTo("name=,"));
        }

        [Test]
        public void Description_DefaultsToNullWhenOmitted()
        {
            var option = new CapturingOption("name");

            Assert.That(option.Description, Is.Null);
        }

        [Test]
        public void Description_ReflectsConstructorArgument()
        {
            var option = new CapturingOption("name", "describe");

            Assert.That(option.Description, Is.EqualTo("describe"));
        }

        [Test]
        public void MaxValueCount_DefaultsToOneInTwoArgConstructor()
        {
            // The (prototype, description) overload chains to (prototype, description, 1).
            var option = new CapturingOption("name=", "desc");

            Assert.That(option.MaxValueCount, Is.EqualTo(1));
        }

        [Test]
        public void ToString_ReturnsPrototype()
        {
            var option = new CapturingOption("a|b=");

            Assert.That(option.ToString(), Is.EqualTo("a|b="));
        }

        [Test]
        public void GetNames_ReturnsACopyDecoupledFromTheOption()
        {
            var option = new CapturingOption("a|b|c");

            var names = option.GetNames();
            names[0] = "tampered";

            Assert.That(option.GetNames()[0], Is.EqualTo("a"));
        }

        [Test]
        public void GetValueSeparators_OnNoneTypeOption_ReturnsEmptyArray()
        {
            var option = new CapturingOption("verbose");

            Assert.That(option.GetValueSeparators(), Is.Empty);
        }

        [Test]
        public void GetValueSeparators_ReturnsACopyDecoupledFromTheOption()
        {
            var option = new CapturingOption("name=,;", null!, 2);

            var seps = option.GetValueSeparators();
            seps[0] = "X";

            Assert.That(option.GetValueSeparators()[0], Is.Not.EqualTo("X"));
        }

        #endregion

        #region Invoke lifecycle

        [Test]
        public void Invoke_CallsOnParseCompleteThenClearsContextState()
        {
            // Invoke runs the user-supplied callback (OnParseComplete) and then
            // resets the context so the next argument starts clean. Pinning so
            // the refactor doesn't accidentally swap the order or skip the reset.
            var set = new OptionSet();
            var option = new CapturingOption("name=");
            set.Add(option);

            var context = new OptionContext(set) { Option = option, OptionName = "--name" };
            context.OptionValues.Add("payload");

            option.Invoke(context);

            Assert.That(option.InvokeCount, Is.EqualTo(1));
            Assert.That(option.CapturedOptionName, Is.EqualTo("--name"));
            Assert.That(option.CapturedOption, Is.SameAs(option));
            Assert.That(option.CapturedValues, Is.EqualTo(new[] { "payload" }));

            // Post-invoke, the context is cleared.
            Assert.That(context.Option, Is.Null);
            Assert.That(context.OptionName, Is.Null);
            Assert.That(context.OptionValues.Count, Is.EqualTo(0));
        }

        #endregion
    }
}
