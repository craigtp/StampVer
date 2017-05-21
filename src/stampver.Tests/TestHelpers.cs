using System;
using System.Collections.Generic;
using System.Linq;

namespace stampver.Tests
{
    public static class TestHelpers
    {
        // Simple method to allow checking if a substring exists within a list of strings, searched case-insensitively.
        public static bool ListContainsSubstring(IEnumerable<string> list, string stringSearched)
        {
            return list.Any(str => str.IndexOf(stringSearched, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
