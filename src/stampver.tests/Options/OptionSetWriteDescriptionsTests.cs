using System;
using System.IO;
using NDesk.Options;
using NUnit.Framework;

namespace stampver.Tests.Options
{
    // Pins WriteOptionDescriptions output line-by-line. Two implementation
    // constants drive every layout decision and are reflected in the test
    // expectations:
    //   OptionWidth = 29  — column at which descriptions begin
    //   line wrap   = 80 - OptionWidth - 2 = 49 chars per description line
    // Continuation lines are indented OptionWidth+2 = 31 spaces.
    [TestFixture]
    internal sealed class OptionSetWriteDescriptionsTests
    {
        // Helper: write to a StringWriter with a fixed "\n" line ending so the
        // expected strings are platform-stable (Environment.NewLine on Windows
        // would otherwise be "\r\n").
        private static string Render(OptionSet set)
        {
            var writer = new StringWriter { NewLine = "\n" };
            set.WriteOptionDescriptions(writer);
            return writer.ToString();
        }

        #region Layout — option-prototype rendering

        [Test]
        public void WriteOptionDescriptions_OnEmptyOptionSet_ProducesNoOutput()
        {
            var set = new OptionSet();

            Assert.That(Render(set), Is.Empty);
        }

        [Test]
        public void WriteOptionDescriptions_BoolOptionWithSingleCharName_RendersWithTwoSpaceIndent()
        {
            var set = new OptionSet { { "a", "set the a flag", _ => { } } };

            // Prototype "  -a" is 4 chars; pad with (29-4)=25 spaces; description follows.
            Assert.That(Render(set), Is.EqualTo("  -a" + new string(' ', 25) + "set the a flag\n"));
        }

        [Test]
        public void WriteOptionDescriptions_BoolOptionWithLongName_RendersWithSixSpaceIndentAndDoubleDash()
        {
            var set = new OptionSet { { "verbose", "be loud", _ => { } } };

            // "      --verbose" = 6 spaces + "--" + "verbose" = 15 chars. Pad (29-15)=14.
            Assert.That(Render(set), Is.EqualTo("      --verbose" + new string(' ', 14) + "be loud\n"));
        }

        [Test]
        public void WriteOptionDescriptions_AliasedShortAndLongNames_RendersBothCommaSeparated()
        {
            var set = new OptionSet { { "h|help", "show help", _ => { } } };

            // "  -h" (4) + ", --help" (8) = 12. Pad (29-12)=17.
            Assert.That(Render(set), Is.EqualTo("  -h, --help" + new string(' ', 17) + "show help\n"));
        }

        [Test]
        public void WriteOptionDescriptions_OptionWithRequiredValue_AppendsEqualsValuePlaceholder()
        {
            var set = new OptionSet { { "name=", "give a name", _ => { } } };

            // "      --name" (12) + "=VALUE" (6) = 18. Pad (29-18)=11.
            Assert.That(Render(set), Is.EqualTo("      --name=VALUE" + new string(' ', 11) + "give a name\n"));
        }

        [Test]
        public void WriteOptionDescriptions_OptionWithOptionalValue_WrapsValuePlaceholderInBrackets()
        {
            var set = new OptionSet { { "name:", "optionally name", _ => { } } };

            // "      --name" (12) + "[" + "=VALUE" + "]" = 20. Pad (29-20)=9.
            Assert.That(Render(set), Is.EqualTo("      --name[=VALUE]" + new string(' ', 9) + "optionally name\n"));
        }

        [Test]
        public void WriteOptionDescriptions_DefaultHandlerEntryIsHiddenFromOutput()
        {
            // <> is excluded from the visible help by GetNextOptionIndex skipping
            // any name == "<>". Pin so the refactor doesn't accidentally surface
            // the default handler row.
            var set = new OptionSet
            {
                { "<>", _ => { } },
                { "a", "visible flag", _ => { } },
            };

            Assert.That(Render(set), Is.EqualTo("  -a" + new string(' ', 25) + "visible flag\n"));
        }

        [Test]
        public void WriteOptionDescriptions_PrototypeWiderThanOptionWidth_BreaksToNextLineAtFullColumnIndent()
        {
            // When the prototype is too wide to fit in 29 chars, WriteOptionDescriptions
            // emits a newline and re-indents 29 spaces before the description.
            var set = new OptionSet
            {
                { "name1|name2|name3|name4=", "stretchy", _ => { } },
            };

            // Prototype renders as "      --name1, --name2, --name3, --name4=VALUE"
            //   "      --name1"     = 13
            //   ", --name2"         = + 9  → 22
            //   ", --name3"         = + 9  → 31  (already past 29 here)
            //   ", --name4"         = + 9  → 40
            //   "=VALUE"            = + 6  → 46
            // Then a newline + 29 spaces + description.
            const string expected = "      --name1, --name2, --name3, --name4=VALUE\n"
                                  + "                             stretchy\n";
            Assert.That(Render(set), Is.EqualTo(expected));
        }

        #endregion

        #region Multi-value layout

        [Test]
        public void WriteOptionDescriptions_MultiValueOptionUsesFirstSeparatorBetweenPlaceholders()
        {
            // Prototype "p=" with the OptionAction overload sets MaxValueCount=2
            // and the default separators to [":", "="]. The display picks
            // ValueSeparators[0], so the colon shows up between placeholders.
            var set = new OptionSet
            {
                { "p=", "two-value", (_, _) => { } },
            };

            // Single-char first name → short form "  -p" (4 chars), not "      --p".
            // 4 + "=VALUE1" (7) + ":VALUE2" (7) = 18. Pad (29-18)=11.
            Assert.That(Render(set), Is.EqualTo("  -p=VALUE1:VALUE2" + new string(' ', 11) + "two-value\n"));
        }

        [Test]
        public void WriteOptionDescriptions_MultiValueOptionWithEmptyBraceSeparatorRendersWithSpace()
        {
            // Prototype "p={}" disables value splitting (ValueSeparators == null)
            // — the display falls back to a single space between placeholders.
            var option = new TwoValueOption("p={}", (_, _) => { }, "two-with-space");
            var set = new OptionSet { option };

            // "  -p" (4) + "=VALUE1" (7) + " VALUE2" (7) = 18. Pad (29-18)=11.
            Assert.That(Render(set), Is.EqualTo("  -p=VALUE1 VALUE2" + new string(' ', 11) + "two-with-space\n"));
        }

        #endregion

        #region Description-driven argument naming

        [Test]
        public void WriteOptionDescriptions_BraceTokenInDescriptionRenamesValuePlaceholder()
        {
            var set = new OptionSet
            {
                { "name=", "Specify the {NAME}.", _ => { } },
            };

            // GetArgumentName extracts "NAME" from {NAME}; GetDescription strips
            // the braces in the rendered description text. The argument name is
            // also routed through the localizer (default identity) before display.
            // "      --name=NAME" = 17. Pad (29-17)=12.
            Assert.That(Render(set), Is.EqualTo("      --name=NAME" + new string(' ', 12) + "Specify the NAME.\n"));
        }

        [Test]
        public void WriteOptionDescriptions_IndexedBraceTokensRenameEachValuePlaceholder()
        {
            // Indexed form {0:KEY}/{1:VALUE} for multi-value options. The rendered
            // description shows them with the colon stripped (just "KEY"/"VALUE").
            var set = new OptionSet
            {
                { "p=", "Map {0:KEY} to {1:VALUE}.", (_, _) => { } },
            };

            // "  -p" (4) + "=KEY" (4) + ":VALUE" (6) = 14. Pad (29-14)=15.
            const string expected = "  -p=KEY:VALUE" + "               Map KEY to VALUE.\n";
            Assert.That(Render(set), Is.EqualTo(expected));
        }

        [Test]
        public void WriteOptionDescriptions_DoubleOpenBraceInDescriptionEscapesToLiteralOpenBrace()
        {
            // {{ → literal { ; the placeholder still defaults to VALUE since no
            // bare {NAME} pattern matches.
            var set = new OptionSet
            {
                { "a", "Use {{ and }} for grouping.", _ => { } },
            };

            Assert.That(Render(set), Is.EqualTo("  -a" + new string(' ', 25) + "Use { and } for grouping.\n"));
        }

        [Test]
        public void WriteOptionDescriptions_StrayCloseBraceInDescription_Throws()
        {
            // QUIRK: a single unbalanced "}" trips an InvalidOperationException
            // from GetDescription. Pin so the refactor decides whether this
            // panic-on-malformed-description behaviour stays.
            var set = new OptionSet { { "a", "broken } description", _ => { } } };

            Assert.Throws<InvalidOperationException>(() => Render(set));
        }

        #endregion

        #region Description wrapping

        [Test]
        public void WriteOptionDescriptions_DescriptionLongerThan49Chars_WrapsOntoContinuationLineWithDeepIndent()
        {
            // GetLines wraps at OptionWidth columns of remaining width — i.e.
            // 80 - 29 - 2 = 49 chars per description line. Continuation lines
            // are indented OptionWidth + 2 = 31 spaces.
            var set = new OptionSet
            {
                { "a", "alpha bravo charlie delta echo foxtrot golf hotel india juliet", _ => { } },
            };

            var output = Render(set);

            // Pin structural properties rather than the precise wrap point —
            // GetLineEnd's choice of break char depends on input, but the
            // continuation-line indent is invariant.
            Assert.That(output, Does.Contain("\n" + new string(' ', 31)),
                "continuation line should be indented 31 spaces.");
            Assert.That(output.Split('\n').Length, Is.GreaterThan(2),
                "long description should produce more than one rendered line.");
        }

        [Test]
        public void WriteOptionDescriptions_NewlineInDescription_ForcesABreakBetweenLines()
        {
            // GetLineEnd treats '\n' as a hard break.
            var set = new OptionSet
            {
                { "a", "first line\nsecond line", _ => { } },
            };

            const string expected = "  -a" + "                         first line\n"
                                  + "                               second line\n";
            Assert.That(Render(set), Is.EqualTo(expected));
        }

        #endregion

        #region Localizer integration

        [Test]
        public void WriteOptionDescriptions_LocalizerWrapsBracketAndEqualsTokensIndependentlyOfTheName()
        {
            // QUIRK worth knowing: WriteOptionPrototype runs the localizer on
            // "[", "]", and "=VALUE" SEPARATELY but NOT on the option name
            // itself. A localizer used as a poor-man's debug tracer would see
            // each piece individually. Pin so the refactor either keeps the
            // structure or documents a deliberate consolidation.
            var localized = new OptionSet(s => $"<{s}>")
            {
                { "name:", "wrapped", _ => { } },
            };

            var output = Render(localized);

            Assert.That(output, Does.Contain("<[>"),
                "the '[' marker is wrapped by the localizer in isolation.");
            Assert.That(output, Does.Contain("<=VALUE>"),
                "the '=VALUE' fragment is wrapped by the localizer in isolation.");
            Assert.That(output, Does.Contain("<]>"),
                "the ']' marker is wrapped by the localizer in isolation.");
            Assert.That(output, Does.Contain("--name"),
                "the option name itself is NOT routed through the localizer.");
        }

        #endregion

        // Inline two-value Option subclass that lets us test the multi-value
        // separator-display path with explicit prototypes.
        private sealed class TwoValueOption : Option
        {
            private readonly OptionAction<string, string> _action;

            public TwoValueOption(string prototype, OptionAction<string, string> action, string description)
                : base(prototype, description, 2)
            {
                _action = action;
            }

            protected override void OnParseComplete(OptionContext c)
            {
                _action(c.OptionValues[0], c.OptionValues[1]);
            }
        }
    }
}
