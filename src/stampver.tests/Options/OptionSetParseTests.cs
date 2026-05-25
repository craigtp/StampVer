using System;
using System.Collections.Generic;
using stampver.Options;
using NUnit.Framework;

namespace stampver.Tests.Options
{
    // End-to-end argv-shaped scenarios. This is the most behaviour-dense file in
    // the suite — it covers every parsing path a real CLI hits:
    //   long/short forms · flag prefixes (-- / - / /) · value separators (= / :)
    //   space-separated values · bool ± toggles · bundling · -- terminator
    //   <> default handler · multi-value with separators · error paths
    [TestFixture]
    internal sealed class OptionSetParseTests
    {
        #region Long-form options

        [Test]
        public void Parse_LongOptionWithEquals_PassesValueToAction()
        {
            string? captured = null;
            var set = new OptionSet { { "name=", v => captured = v } };

            var unprocessed = set.Parse(new[] { "--name=hello" });

            Assert.That(captured, Is.EqualTo("hello"));
            Assert.That(unprocessed, Is.Empty);
        }

        [Test]
        public void Parse_LongOptionWithColon_PassesValueToAction()
        {
            string? captured = null;
            var set = new OptionSet { { "name=", v => captured = v } };

            set.Parse(new[] { "--name:hello" });

            Assert.That(captured, Is.EqualTo("hello"));
        }

        [Test]
        public void Parse_LongOptionFollowedBySeparateValueArg_PassesValueToAction()
        {
            string? captured = null;
            var set = new OptionSet { { "name=", v => captured = v } };

            set.Parse(new[] { "--name", "hello" });

            Assert.That(captured, Is.EqualTo("hello"));
        }

        [Test]
        public void Parse_OptionalTypeWithoutValue_InvokesActionWithNull()
        {
            // Optional-type options invoke immediately even when no value follows.
            // The action receives null. Pinning so refactor can't quietly upgrade
            // this to require a value.
            string? captured = "<unset>";
            int invokeCount = 0;
            var set = new OptionSet
            {
                { "name:", v => { captured = v; invokeCount++; } },
            };

            set.Parse(new[] { "--name" });

            Assert.That(invokeCount, Is.EqualTo(1));
            Assert.That(captured, Is.Null);
        }

        #endregion

        #region Flag-prefix variants

        [Test]
        public void Parse_RecognisesAllThreeFlagPrefixesForLongOptions()
        {
            // The regex at OptionSet.ValueOption accepts "--", "-", or "/" as the
            // flag prefix. All three feed the same option.
            var captured = new List<string>();
            var set = new OptionSet { { "name=", v => captured.Add(v) } };

            set.Parse(new[] { "--name=a", "-name=b", "/name=c" });

            Assert.That(captured, Is.EqualTo(new[] { "a", "b", "c" }));
        }

        #endregion

        #region Bool / no-value options

        [Test]
        public void Parse_BoolOptionInvokesActionWithStrippedName()
        {
            // QUIRK: For OptionValueType.None, the action's value parameter is
            // the option NAME (without flag prefix), not the original token and
            // not a literal "true"/null. Refactor likely wants to revisit this.
            string? captured = null;
            var set = new OptionSet { { "verbose", v => captured = v } };

            set.Parse(new[] { "--verbose" });

            Assert.That(captured, Is.EqualTo("verbose"));
        }

        [Test]
        public void Parse_PlusSuffixOnBoolOption_InvokesActionWithFullOriginalToken()
        {
            // QUIRK: for "-a+", the action receives the entire original argv token
            // (including flag prefix and trailing '+'), not the stripped name.
            // This asymmetry with the bare-name path above is one of the
            // observable behaviours flagged for review during modernisation.
            string? captured = null;
            var set = new OptionSet { { "a", v => captured = v } };

            set.Parse(new[] { "-a+" });

            Assert.That(captured, Is.EqualTo("-a+"));
        }

        [Test]
        public void Parse_MinusSuffixOnBoolOption_InvokesActionWithNull()
        {
            string? captured = "<unset>";
            int invokeCount = 0;
            var set = new OptionSet { { "a", v => { captured = v; invokeCount++; } } };

            set.Parse(new[] { "-a-" });

            Assert.That(invokeCount, Is.EqualTo(1));
            Assert.That(captured, Is.Null);
        }

        #endregion

        #region Bundling (only for the "-" prefix)

        [Test]
        public void Parse_BundlesMultipleSingleCharBoolOptionsBehindOneDash()
        {
            var fired = new List<string>();
            var set = new OptionSet
            {
                { "a", _ => fired.Add("a") },
                { "b", _ => fired.Add("b") },
                { "c", _ => fired.Add("c") },
            };

            set.Parse(new[] { "-abc" });

            Assert.That(fired, Is.EqualTo(new[] { "a", "b", "c" }));
        }

        [Test]
        public void Parse_BundledTrailingValueOption_TakesRemainderAsValue()
        {
            // For "-Dkey=val": prototype "D=" takes a value, ParseBundledValue
            // hands it the entire remainder ("key=val") which the 2-value
            // option splits on '=' / ':' default separators.
            string? capturedKey = null;
            string? capturedValue = null;
            var set = new OptionSet
            {
                { "D=", (k, v) => { capturedKey = k; capturedValue = v; } },
            };

            set.Parse(new[] { "-Dkey=val" });

            Assert.That(capturedKey, Is.EqualTo("key"));
            Assert.That(capturedValue, Is.EqualTo("val"));
        }

        [Test]
        public void Parse_BundlingNotAttemptedForDoubleDashPrefix()
        {
            // Critical contract: "--abc" looks for an option literally named
            // "abc", it does NOT decompose into -a -b -c. This is what lets
            // long flag names coexist with bundling.
            var set = new OptionSet
            {
                { "a", _ => Assert.Fail("a should not have fired for --abc") },
                { "b", _ => Assert.Fail("b should not have fired for --abc") },
                { "c", _ => Assert.Fail("c should not have fired for --abc") },
            };

            var unprocessed = set.Parse(new[] { "--abc" });

            Assert.That(unprocessed, Is.EqualTo(new[] { "--abc" }));
        }

        [Test]
        public void Parse_BundlingNotAttemptedForSlashPrefix()
        {
            // Same contract for the Windows-style "/" prefix.
            var set = new OptionSet { { "a", _ => Assert.Fail("a should not have fired for /abc") } };

            var unprocessed = set.Parse(new[] { "/abc" });

            Assert.That(unprocessed, Is.EqualTo(new[] { "/abc" }));
        }

        [Test]
        public void Parse_BundleWithUnregisteredFollower_ThrowsOptionException()
        {
            var set = new OptionSet { { "a", _ => { } } };

            // First char "a" is registered, second char "x" is not — message
            // pins the diagnostic verbatim so callers can match on it.
            var ex = Assert.Throws<OptionException>(() => set.Parse(new[] { "-ax" }))!;
            Assert.That(ex.Message, Does.Contain("Cannot bundle unregistered option"));
            Assert.That(ex.OptionName, Is.EqualTo("-x"));
        }

        [Test]
        public void Parse_BundleWithUnregisteredLeader_ReturnsUnprocessedRatherThanThrowing()
        {
            // QUIRK: when the FIRST char of a bundle isn't registered, parsing
            // gives up rather than throwing. The argument falls through to the
            // unprocessed list. Pin this asymmetry.
            var set = new OptionSet { { "a", _ => { } } };

            var unprocessed = set.Parse(new[] { "-xa" });

            Assert.That(unprocessed, Is.EqualTo(new[] { "-xa" }));
        }

        #endregion

        #region Multi-value options

        [Test]
        public void Parse_MultiValueWithDefaultSeparators_SplitsOnEqualsAndColon()
        {
            // Prototype "p=" via OptionAction overload sets MaxValueCount=2
            // and the default separators to [":", "="].
            string? key = null;
            string? value = null;
            var set = new OptionSet { { "p=", (k, v) => { key = k; value = v; } } };

            set.Parse(new[] { "--p=k=v" });

            Assert.That(key, Is.EqualTo("k"));
            Assert.That(value, Is.EqualTo("v"));
        }

        [Test]
        public void Parse_MultiValueWithExplicitCommaSeparator_SplitsOnComma()
        {
            // Prototype "p=," count=2 — declared via Add(Option) since the
            // higher-level Add overloads don't expose a separator-config knob.
            string? key = null;
            string? value = null;
            var option = new TwoValueOption("p=,", (k, v) => { key = k; value = v; });
            var set = new OptionSet { option };

            set.Parse(new[] { "--p=alpha,beta" });

            Assert.That(key, Is.EqualTo("alpha"));
            Assert.That(value, Is.EqualTo("beta"));
        }

        [Test]
        public void Parse_MultiValueWithEmptyBraceSeparator_RequiresDistinctArgvTokens()
        {
            // "{}" tells NDesk "do not split — values must be separate args".
            string? key = null;
            string? value = null;
            var option = new TwoValueOption("p={}", (k, v) => { key = k; value = v; });
            var set = new OptionSet { option };

            set.Parse(new[] { "--p", "alpha", "beta" });

            Assert.That(key, Is.EqualTo("alpha"));
            Assert.That(value, Is.EqualTo("beta"));
        }

        [Test]
        public void Parse_MultiValueWithTooManyValues_ThrowsOptionException()
        {
            var option = new TwoValueOption("p=,", (_, _) => { });
            var set = new OptionSet { option };

            // The localizer is invoked on the message — pin the unwrapped
            // diagnostic so the modernised parser's message stays diff-able.
            var ex = Assert.Throws<OptionException>(() => set.Parse(new[] { "--p=a,b,c" }))!;
            Assert.That(ex.Message, Does.Contain("Found 3 option values when expecting 2"));
        }

        #endregion

        #region "--" terminator

        [Test]
        public void Parse_DoubleDashStopsOptionProcessingAndPassesRemainderToUnprocessed()
        {
            string? captured = null;
            var set = new OptionSet { { "name=", v => captured = v } };

            var unprocessed = set.Parse(new[] { "--name=value", "--", "--other", "-x" });

            Assert.That(captured, Is.EqualTo("value"));
            Assert.That(unprocessed, Is.EqualTo(new[] { "--other", "-x" }));
        }

        [Test]
        public void Parse_DoubleDashItselfIsNotIncludedInUnprocessed()
        {
            var set = new OptionSet();

            var unprocessed = set.Parse(new[] { "--", "after" });

            Assert.That(unprocessed, Is.EqualTo(new[] { "after" }));
        }

        #endregion

        #region "<>" default handler

        [Test]
        public void Parse_DefaultHandlerCapturesEveryUnmatchedArgumentAndUnprocessedListIsEmpty()
        {
            var captured = new List<string>();
            var set = new OptionSet
            {
                { "name=", _ => { } },
                { "<>", v => captured.Add(v) },
            };

            var unprocessed = set.Parse(new[] { "first", "--name=v", "second", "third" });

            Assert.That(captured, Is.EqualTo(new[] { "first", "second", "third" }));
            Assert.That(unprocessed, Is.Empty);
        }

        [Test]
        public void Parse_WithoutDefaultHandler_UnmatchedArgumentsAreReturnedInUnprocessedList()
        {
            string? captured = null;
            var set = new OptionSet { { "name=", v => captured = v } };

            var unprocessed = set.Parse(new[] { "first", "--name=v", "second" });

            Assert.That(captured, Is.EqualTo("v"));
            Assert.That(unprocessed, Is.EqualTo(new[] { "first", "second" }));
        }

        [Test]
        public void Parse_DefaultHandlerAlsoReceivesArgumentsAfterDoubleDash()
        {
            // After "--", every remaining token routes through the same
            // Unprocessed() helper — so a registered "<>" still picks them up.
            var captured = new List<string>();
            var set = new OptionSet
            {
                { "<>", v => captured.Add(v) },
            };

            set.Parse(new[] { "before", "--", "--after" });

            Assert.That(captured, Is.EqualTo(new[] { "before", "--after" }));
        }

        #endregion

        #region End-of-argv handling

        [Test]
        public void Parse_RequiredOptionAtEndWithoutValue_ThrowsWhenActionReadsTheValue()
        {
            // The pending-Option-at-end branch invokes regardless. The action
            // (or the OptionValueCollection it accesses) is what throws when
            // it reads c.OptionValues[0] for a Required option with no value.
            var set = new OptionSet { { "name=", _ => { } } };

            var ex = Assert.Throws<OptionException>(() => set.Parse(new[] { "--name" }))!;
            Assert.That(ex.Message, Does.Contain("Missing required value for option"));
            Assert.That(ex.OptionName, Is.EqualTo("--name"));
        }

        [Test]
        public void Parse_OnEmptyArgs_ReturnsEmptyListAndInvokesNothing()
        {
            int invokeCount = 0;
            var set = new OptionSet { { "name=", _ => invokeCount++ } };

            var unprocessed = set.Parse(Array.Empty<string>());

            Assert.That(unprocessed, Is.Empty);
            Assert.That(invokeCount, Is.EqualTo(0));
        }

        #endregion

        #region Localizer integration in error paths

        [Test]
        public void Parse_RoutesErrorMessageThroughCustomLocalizer()
        {
            // The "too many values" path passes its message through the
            // configured localizer. Pin so the refactor doesn't accidentally
            // bypass it — a translation hook breaking is non-obvious.
            var set = new OptionSet(s => $"<<{s}>>");
            var option = new TwoValueOption("p=,", (_, _) => { });
            set.Add(option);

            var ex = Assert.Throws<OptionException>(() => set.Parse(new[] { "--p=a,b,c" }))!;
            Assert.That(ex.Message, Does.StartWith("<<"));
            Assert.That(ex.Message, Does.EndWith(">>"));
        }

        #endregion

        #region Realistic end-to-end smoke

        [Test]
        public void Parse_StampverStyleArgVector_FiresCorrectActionsAndCollectsPositionals()
        {
            // Approximates the prototype set wired up in Stampver.TryParseArguments.
            // This isn't a substitute for the existing StampverTests integration
            // suite — it pins that the full mix of option types coexists cleanly
            // when handed to one OptionSet.
            string? incrementPart = null;
            int verboseHits = 0;
            int dryRunHits = 0;
            var set = new OptionSet
            {
                { "i=", v => incrementPart = v },
                { "verbose", _ => verboseHits++ },
                { "dryrun", _ => dryRunHits++ },
            };

            var positionals = set.Parse(new[] { "-i", "minor", "--verbose", "--dryrun", "*.cs" });

            Assert.That(incrementPart, Is.EqualTo("minor"));
            Assert.That(verboseHits, Is.EqualTo(1));
            Assert.That(dryRunHits, Is.EqualTo(1));
            Assert.That(positionals, Is.EqualTo(new[] { "*.cs" }));
        }

        #endregion

        // Inline two-value Option subclass that lets us declare an explicit
        // separator (the higher-level Add overloads don't expose this knob).
        private sealed class TwoValueOption : Option
        {
            private readonly Action<string, string> _action;

            public TwoValueOption(string prototype, Action<string, string> action)
                : base(prototype, null!, 2)
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
