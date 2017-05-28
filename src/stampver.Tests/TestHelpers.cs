using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace stampver.Tests
{
    public static class TestHelpers
    {
        // Simple method to allow checking if a substring exists within a list of strings, searched case-insensitively.
        public static bool ListContainsSubstring(IEnumerable<string> list, string stringSearched)
        {
            return list.Any(str => str.IndexOf(stringSearched, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // Provides a simple Assert method that wraps NUnit's Assert.True and used the method above for finding a substring
        // within a list if strings.  Ensures that an appropriate message is output in the event of test failure.
        public static void AssertContains(IEnumerable<string> stringList, string stringToFind)
        {
            Assert.True(ListContainsSubstring(stringList, stringToFind), $"Expected to find: {stringToFind} but was not found in list.");
        }
    }
}
