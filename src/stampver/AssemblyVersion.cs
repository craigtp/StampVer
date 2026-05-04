using System;
using System.Globalization;

namespace stampver
{
    internal sealed class AssemblyVersion
    {
        // Each numeric part is bounded by ushort.MaxValue (65535) — the historical
        // limit for AssemblyVersion attribute parts. Increment is a no-op once a
        // part reaches this; decrement is a no-op at 0.
        private const int MaxVersionPart = ushort.MaxValue;

        // AssemblyVersion attributes carry at most four parts (major.minor.patch.revision).
        // Anything beyond that is silently truncated to preserve historical behaviour;
        // the upstream regex in Stampver also caps matches at four parts.
        private const int MaxParts = 4;

        private readonly string[] _originalParts;
        private readonly int?[] _parsed;

        public AssemblyVersion(string versionString)
        {
            var split = versionString.Split('.');
            if (split.Length < 3)
            {
                throw new ArgumentException("versionString does not contain at least three parts.");
            }

            var length = Math.Min(split.Length, MaxParts);
            _originalParts = new string[length];
            _parsed = new int?[length];
            for (var i = 0; i < length; i++)
            {
                _originalParts[i] = split[i];
                if (int.TryParse(split[i], out var parsed))
                {
                    _parsed[i] = parsed;
                }
            }
        }

        public void Increment(VersionNumberPart versionNumberPart) => Adjust(versionNumberPart, delta: +1, cascade: true);

        public void Decrement(VersionNumberPart versionNumberPart) => Adjust(versionNumberPart, delta: -1, cascade: false);

        // Single source of truth for both Increment and Decrement. The cascade flag
        // controls whether sibling parts after `part` are reset to zero — increments
        // cascade (e.g. major bump zeroes minor and patch); decrements never do.
        private void Adjust(VersionNumberPart part, int delta, bool cascade)
        {
            var index = part switch
            {
                VersionNumberPart.Major => 0,
                VersionNumberPart.Minor => 1,
                VersionNumberPart.Patch => 2,
                _ => -1
            };
            if (index < 0 || _parsed[index] is null)
            {
                return;
            }

            var next = _parsed[index]!.Value + delta;
            if (next < 0 || next > MaxVersionPart)
            {
                return;
            }
            _parsed[index] = next;

            if (cascade)
            {
                // Reset every sibling from index+1 through patch (index 2). Revision
                // (index 3) is intentionally NOT cascaded — historical AssemblyVersion
                // semantics. Skips parts whose original token wasn't an integer
                // (e.g. "*"), preserving the literal in GetVersionString.
                for (var i = index + 1; i <= 2 && i < _parsed.Length; i++)
                {
                    if (_parsed[i] is not null)
                    {
                        _parsed[i] = 0;
                    }
                }
            }
        }

        public string GetVersionString()
        {
            var parts = new string[_originalParts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                parts[i] = _parsed[i]?.ToString(CultureInfo.InvariantCulture) ?? _originalParts[i];
            }
            return string.Join('.', parts);
        }
    }
}
