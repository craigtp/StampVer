using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace stampver.Tests
{
    public static class TestHelpers
    {
        // Provides a simple Assert method that wraps an NUnit assertion and used the method above for finding a substring
        // within a list if strings.  Ensures that an appropriate message is output in the event of test failure.
        public static void AssertContains(IEnumerable<string> stringList, string stringToFind)
        {
            Assert.That(ListContainsSubstring(stringList, stringToFind), Is.True, $"Expected to find: {stringToFind} but was not found in list.");
        }
        
        // Simple method to allow checking if a substring exists within a list of strings, searched case-insensitively.
        private static bool ListContainsSubstring(IEnumerable<string> list, string stringSearched)
        {
            return list.Any(str => str.Contains(stringSearched, StringComparison.OrdinalIgnoreCase));
        }
    }
}
