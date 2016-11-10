using System;

namespace stampver
{
    public class AssemblyVersion
    {
        private readonly string _majorString;
        private readonly string _minorString;
        private readonly string _patchString;
        private readonly string _revisionString;

        private int? _majorInt;
        private int? _minorInt;
        private int? _patchInt;
        private int? _revisionInt;

        public AssemblyVersion(string versionString)
        {
            var versionElements = versionString.Split('.');
            if (versionElements.Length < 3)
            {
                throw new ArgumentException("versionString does not contain at least three parts.");
            }
            _majorString = versionElements[0];
            _minorString = versionElements[1];
            _patchString = versionElements[2];
            if (versionElements.Length > 3)
            {
                _revisionString = versionElements[3];
            }

            int majorInt;
            if (int.TryParse(_majorString, out majorInt))
            {
                _majorInt = majorInt;
            }

            int minorInt;
            if (int.TryParse(_minorString, out minorInt))
            {
                _minorInt = minorInt;
            }

            int patchInt;
            if (int.TryParse(_patchString, out patchInt))
            {
                _patchInt = patchInt;
            }

            if (_revisionString != null)
            {
                int revisionInt;
                if (int.TryParse(_revisionString, out revisionInt))
                {
                    _revisionInt = revisionInt;
                }
            }
        }

        private void IncrementMajor()
        {
            if (_majorInt != null && _majorInt < UInt16.MaxValue)
            {
                _majorInt++;

                // Reset the minor and patch numbers to zero when major is incremented.
                if (_minorInt != null)
                {
                    _minorInt = 0;
                }
                if (_patchInt != null)
                {
                    _patchInt = 0;
                }
            }
        }

        private void DecrementMajor()
        {
            if (_majorInt != null && _majorInt > 0)
            {
                _majorInt--;
            }
        }

        private void IncrementMinor()
        {
            if (_minorInt != null && _minorInt < UInt16.MaxValue)
            {
                _minorInt++;

                // Reset patch number to zero when minor is incremented.
                if (_patchInt != null)
                {
                    _patchInt = 0;
                }
            }
        }

        private void DecrementMinor()
        {
            if (_minorInt != null && _minorInt > 0)
            {
                _minorInt--;
            }
        }

        private void IncrementPatch()
        {
            if (_patchInt != null && _patchInt < UInt16.MaxValue)
            {
                _patchInt++;
            }
        }

        private void DecrementPatch()
        {
            if (_patchInt != null && _patchInt > 0)
            {
                _patchInt--;
            }
        }

        public string GetVersionString()
        {
            var versionString = $"{_majorInt?.ToString() ?? _majorString}.{_minorInt?.ToString() ?? _minorString}.{_patchInt?.ToString() ?? _patchString}";
            if (_revisionString != null)
            {
                versionString += $".{_revisionInt?.ToString() ?? _revisionString}";
            }
            return versionString;
        }

        public void Increment(VersionNumberPart versionNumberPart)
        {
            switch (versionNumberPart)
            {
                case VersionNumberPart.Major:
                    IncrementMajor();
                    break;
                case VersionNumberPart.Minor:
                    IncrementMinor();
                    break;
                case VersionNumberPart.Patch:
                    IncrementPatch();
                    break;
            }
        }

        public void Decrement(VersionNumberPart versionNumberPart)
        {
            switch (versionNumberPart)
            {
                case VersionNumberPart.Major:
                    DecrementMajor();
                    break;
                case VersionNumberPart.Minor:
                    DecrementMinor();
                    break;
                case VersionNumberPart.Patch:
                    DecrementPatch();
                    break;
            }
        }
    }
}
