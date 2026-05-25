// Originally derived from NDesk.Options by Jonathan Pryor (Novell, MIT, 2008).
// Substantially rewritten 2026 to target modern C#, drop legacy interop, and
// fix several observable bugs and quirks pinned by the unit-test suite.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace stampver.Options;

internal enum OptionValueType
{
    None,
    Optional,
    Required,
}

internal sealed class OptionException : Exception
{
    public OptionException()
    {
    }

    public OptionException(string? message, string? optionName)
        : base(message)
    {
        OptionName = optionName;
    }

    public OptionException(string? message, string? optionName, Exception? innerException)
        : base(message, innerException)
    {
        OptionName = optionName;
    }

    public string? OptionName { get; }
}

// Backed by a List<string?> internally so the parser can store null sentinel
// values (the "-a-" minus-suffix path and the optional-with-no-value path both
// rely on this). The IList<string>/IList faces are kept because tests pin
// IsFixedSize and ICollection behaviour.
internal sealed class OptionValueCollection : IList<string>, IList
{
    private readonly List<string?> _values = [];
    private readonly OptionContext _context;

    internal OptionValueCollection(OptionContext context)
    {
        _context = context;
    }

    public int Count => _values.Count;

    public bool IsReadOnly => false;

    public void Add(string item) => _values.Add(item);

    public void Clear() => _values.Clear();

    public bool Contains(string item) => _values.Contains(item);

    public void CopyTo(string[] array, int arrayIndex) =>
        ((ICollection)_values).CopyTo(array, arrayIndex);

    public bool Remove(string item) => _values.Remove(item);

    public int IndexOf(string item) => _values.IndexOf(item);

    public void Insert(int index, string item) => _values.Insert(index, item);

    public void RemoveAt(int index) => _values.RemoveAt(index);

    // The indexer getter is the contract surface that the OptionSet parser and
    // user-action lambdas read through. AssertValid runs four distinct checks:
    //   1. Option not bound       → InvalidOperationException
    //   2. index ≥ MaxValueCount  → ArgumentOutOfRangeException
    //   3. Required, no value     → OptionException ("Missing required value")
    //   4. Optional, no value     → returns null (no throw)
    // The setter intentionally bypasses AssertValid — that asymmetry is pinned
    // by tests and is preserved here.
    public string this[int index]
    {
        get
        {
            AssertValid(index);
            return index >= _values.Count ? null! : _values[index]!;
        }
        set => _values[index] = value;
    }

    private void AssertValid(int index)
    {
        if (_context.Option is null)
        {
            throw new InvalidOperationException("OptionContext.Option is null.");
        }
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _context.Option.MaxValueCount);
        if (_context.Option.OptionValueType == OptionValueType.Required && index >= _values.Count)
        {
            throw new OptionException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    _context.OptionSet.MessageLocalizer("Missing required value for option '{0}'."),
                    _context.OptionName),
                _context.OptionName);
        }
    }

    public IEnumerator<string> GetEnumerator()
    {
        foreach (var value in _values)
        {
            yield return value!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

    public List<string> ToList() => new(_values!);

    public string[] ToArray() => _values.ToArray()!;

    public override string ToString() => string.Join(", ", _values);

    bool IList.IsFixedSize => false;

    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => ((ICollection)_values).SyncRoot;

    void ICollection.CopyTo(Array array, int index) => ((ICollection)_values).CopyTo(array, index);

    object? IList.this[int index]
    {
        get => this[index];
        set => _values[index] = (string?)value;
    }

    int IList.Add(object? value)
    {
        _values.Add((string?)value);
        return _values.Count - 1;
    }

    bool IList.Contains(object? value) => _values.Contains((string?)value);

    int IList.IndexOf(object? value) => _values.IndexOf((string?)value);

    void IList.Insert(int index, object? value) => _values.Insert(index, (string?)value);

    void IList.Remove(object? value) => _values.Remove((string?)value);

    void IList.RemoveAt(int index) => _values.RemoveAt(index);
}

internal sealed class OptionContext
{
    public OptionContext(OptionSet set)
    {
        ArgumentNullException.ThrowIfNull(set);
        OptionSet = set;
        OptionValues = new OptionValueCollection(this);
    }

    public Option? Option { get; set; }

    public string? OptionName { get; set; }

    public int OptionIndex { get; set; }

    public OptionSet OptionSet { get; }

    public OptionValueCollection OptionValues { get; }
}

internal abstract class Option
{
    private static readonly char[] NameTerminator = ['=', ':'];

    private readonly string[] _names;
    private string[]? _separators;

    protected Option(string prototype, string? description)
        : this(prototype, description, 1)
    {
    }

    protected Option(string prototype, string? description, int maxValueCount)
    {
        ArgumentNullException.ThrowIfNull(prototype);
        if (prototype.Length == 0)
        {
            throw new ArgumentException("Cannot be the empty string.", nameof(prototype));
        }
        ArgumentOutOfRangeException.ThrowIfNegative(maxValueCount);

        Prototype = prototype;
        _names = prototype.Split('|');
        Description = description;
        MaxValueCount = maxValueCount;
        OptionValueType = ParsePrototype();

        if (MaxValueCount == 0 && OptionValueType != OptionValueType.None)
        {
            throw new ArgumentException(
                "Cannot provide maxValueCount of 0 for OptionValueType.Required or OptionValueType.Optional.",
                nameof(maxValueCount));
        }
        if (OptionValueType == OptionValueType.None && maxValueCount > 1)
        {
            throw new ArgumentException(
                $"Cannot provide maxValueCount of {maxValueCount} for OptionValueType.None.",
                nameof(maxValueCount));
        }
        if (Array.IndexOf(_names, "<>") >= 0 &&
            ((_names.Length == 1 && OptionValueType != OptionValueType.None) ||
             (_names.Length > 1 && MaxValueCount > 1)))
        {
            throw new ArgumentException(
                "The default option handler '<>' cannot require values.",
                nameof(prototype));
        }
    }

    public string Prototype { get; }

    public string? Description { get; }

    public OptionValueType OptionValueType { get; }

    public int MaxValueCount { get; }

    public string[] GetNames() => (string[])_names.Clone();

    public string[] GetValueSeparators() =>
        _separators is null ? [] : (string[])_separators.Clone();

    internal string[] Names => _names;

    internal string[]? ValueSeparators => _separators;

    // Uses InvariantCulture (Phase 0 modernisation — the original NDesk code
    // called ConvertFromString without a culture argument, which silently
    // defaulted to CurrentCulture and made command-line option parsing depend
    // on the user's locale).
    protected static T Parse<T>(string? value, OptionContext c)
    {
        var converter = TypeDescriptor.GetConverter(typeof(T));
        try
        {
            if (value is not null)
            {
                return (T)converter.ConvertFromString(null!, CultureInfo.InvariantCulture, value)!;
            }
        }
        catch (Exception e)
        {
            throw new OptionException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    c.OptionSet.MessageLocalizer("Could not convert string `{0}' to type {1} for option `{2}'."),
                    value, typeof(T).Name, c.OptionName),
                c.OptionName,
                e);
        }
        return default!;
    }

    public void Invoke(OptionContext c)
    {
        OnParseComplete(c);
        c.OptionName = null;
        c.Option = null;
        c.OptionValues.Clear();
    }

    protected abstract void OnParseComplete(OptionContext c);

    public override string ToString() => Prototype;

    private OptionValueType ParsePrototype()
    {
        char type = '\0';
        var separators = new List<string>();
        for (int i = 0; i < _names.Length; i++)
        {
            var name = _names[i];
            if (name.Length == 0)
            {
                throw new ArgumentException("Empty option names are not supported.", nameof(Prototype));
            }

            int end = name.IndexOfAny(NameTerminator);
            if (end == -1)
            {
                continue;
            }
            _names[i] = name[..end];
            if (type == '\0' || type == name[end])
            {
                type = name[end];
            }
            else
            {
                throw new ArgumentException(
                    $"Conflicting option types: '{type}' vs. '{name[end]}'.",
                    nameof(Prototype));
            }
            AddSeparators(name, end, separators);
        }

        if (type == '\0')
        {
            return OptionValueType.None;
        }

        if (MaxValueCount <= 1 && separators.Count != 0)
        {
            throw new ArgumentException(
                $"Cannot provide key/value separators for Options taking {MaxValueCount} value(s).",
                nameof(Prototype));
        }
        if (MaxValueCount > 1)
        {
            _separators = separators switch
            {
                { Count: 0 } => [":", "="],
                [""] => null,
                _ => [.. separators],
            };
        }

        return type == '=' ? OptionValueType.Required : OptionValueType.Optional;
    }

    private static void AddSeparators(string name, int end, List<string> separators)
    {
        int start = -1;
        for (int i = end + 1; i < name.Length; i++)
        {
            switch (name[i])
            {
                case '{':
                    if (start != -1)
                    {
                        throw new ArgumentException(
                            $"Ill-formed name/value separator found in \"{name}\".",
                            nameof(name));
                    }
                    start = i + 1;
                    break;
                case '}':
                    if (start == -1)
                    {
                        throw new ArgumentException(
                            $"Ill-formed name/value separator found in \"{name}\".",
                            nameof(name));
                    }
                    separators.Add(name[start..i]);
                    start = -1;
                    break;
                default:
                    if (start == -1)
                    {
                        separators.Add(name[i].ToString());
                    }
                    break;
            }
        }
        if (start != -1)
        {
            throw new ArgumentException(
                $"Ill-formed name/value separator found in \"{name}\".",
                nameof(name));
        }
    }
}

internal sealed partial class OptionSet : IEnumerable<Option>
{
    // 80-column terminal target. OptionPrototypeColumnWidth is the column at
    // which descriptions begin; LineWidth-OptionPrototypeColumnWidth-2 is the
    // wrap point for description text.
    private const int LineWidth = 80;
    private const int OptionPrototypeColumnWidth = 29;
    private const int DescriptionLineWidth = LineWidth - OptionPrototypeColumnWidth - 2;
    private const string DefaultHandlerName = "<>";

    private readonly List<Option> _options = [];
    private readonly Dictionary<string, Option> _optionsByName = new(StringComparer.Ordinal);

    public OptionSet()
        : this(static s => s)
    {
    }

    public OptionSet(Func<string, string> localizer)
    {
        ArgumentNullException.ThrowIfNull(localizer);
        MessageLocalizer = localizer;
    }

    public Func<string, string> MessageLocalizer { get; }

    public int Count => _options.Count;

    public bool Contains(string name) => _optionsByName.ContainsKey(name);

    public Option this[string name]
    {
        get
        {
            if (!_optionsByName.TryGetValue(name, out var option))
            {
                throw new KeyNotFoundException();
            }
            return option;
        }
    }

    public OptionSet Add(Option option)
    {
        ArgumentNullException.ThrowIfNull(option);
        AddCore(option);
        return this;
    }

    public bool Remove(Option option)
    {
        ArgumentNullException.ThrowIfNull(option);

        // Capture aliases BEFORE removing — the original NDesk implementation
        // dereferenced the underlying KeyedCollection's storage AFTER the
        // base.RemoveItem call, which either threw or returned the wrong item
        // depending on whether the removed option was last in the list.
        var aliases = option.Names;
        if (!_options.Remove(option))
        {
            return false;
        }
        foreach (var alias in aliases)
        {
            _optionsByName.Remove(alias);
        }
        return true;
    }

    private void AddCore(Option option)
    {
        // Validate every alias before touching either collection so a partial
        // failure leaves the set unchanged.
        foreach (var name in option.Names)
        {
            if (_optionsByName.ContainsKey(name))
            {
                throw new ArgumentException(
                    $"Option with name '{name}' already added.",
                    nameof(option));
            }
        }

        _options.Add(option);
        foreach (var name in option.Names)
        {
            _optionsByName[name] = option;
        }
    }

    public OptionSet Add(string prototype, Action<string> action) =>
        Add(prototype, null, action);

    public OptionSet Add(string prototype, string? description, Action<string> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Add(new ActionOption(prototype, description, 1, values => action(values[0])));
    }

    public OptionSet Add(string prototype, Action<string, string> action) =>
        Add(prototype, null, action);

    public OptionSet Add(string prototype, string? description, Action<string, string> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Add(new ActionOption(prototype, description, 2, values => action(values[0], values[1])));
    }

    public OptionSet Add<T>(string prototype, Action<T> action) =>
        Add(prototype, null, action);

    public OptionSet Add<T>(string prototype, string? description, Action<T> action) =>
        Add(new ActionOption<T>(prototype, description, action));

    public OptionSet Add<TKey, TValue>(string prototype, Action<TKey, TValue> action) =>
        Add(prototype, null, action);

    public OptionSet Add<TKey, TValue>(string prototype, string? description, Action<TKey, TValue> action) =>
        Add(new ActionOption<TKey, TValue>(prototype, description, action));

    public IEnumerator<Option> GetEnumerator() => _options.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _options.GetEnumerator();

    // The three private Option subclasses below are the implementations behind
    // the user-facing Action-based Add overloads. Keeping them nested keeps the
    // Option public/internal surface limited to the abstract base.
    private sealed class ActionOption : Option
    {
        private readonly Action<OptionValueCollection> _action;

        public ActionOption(string prototype, string? description, int count, Action<OptionValueCollection> action)
            : base(prototype, description, count)
        {
            ArgumentNullException.ThrowIfNull(action);
            _action = action;
        }

        protected override void OnParseComplete(OptionContext c) => _action(c.OptionValues);
    }

    private sealed class ActionOption<T> : Option
    {
        private readonly Action<T> _action;

        public ActionOption(string prototype, string? description, Action<T> action)
            : base(prototype, description, 1)
        {
            ArgumentNullException.ThrowIfNull(action);
            _action = action;
        }

        protected override void OnParseComplete(OptionContext c) =>
            _action(Parse<T>(c.OptionValues[0], c));
    }

    private sealed class ActionOption<TKey, TValue> : Option
    {
        private readonly Action<TKey, TValue> _action;

        public ActionOption(string prototype, string? description, Action<TKey, TValue> action)
            : base(prototype, description, 2)
        {
            ArgumentNullException.ThrowIfNull(action);
            _action = action;
        }

        protected override void OnParseComplete(OptionContext c) =>
            _action(
                Parse<TKey>(c.OptionValues[0], c),
                Parse<TValue>(c.OptionValues[1], c));
    }

    private OptionContext CreateOptionContext() => new(this);

    [GeneratedRegex(
        @"^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$",
        RegexOptions.CultureInvariant)]
    private static partial Regex OptionTokenPattern();

    public List<string> Parse(IEnumerable<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var context = CreateOptionContext();
        context.OptionIndex = -1;
        bool processingOptions = true;
        var unprocessed = new List<string>();
        _optionsByName.TryGetValue(DefaultHandlerName, out var defaultHandler);

        foreach (var argument in arguments)
        {
            context.OptionIndex++;
            if (processingOptions && argument == "--")
            {
                processingOptions = false;
                continue;
            }
            if (!processingOptions)
            {
                RouteUnprocessed(unprocessed, defaultHandler, context, argument);
                continue;
            }
            if (!ParseArgument(argument, context))
            {
                RouteUnprocessed(unprocessed, defaultHandler, context, argument);
            }
        }

        // Flush any option whose value-collection wasn't satisfied during the
        // loop (e.g. a required option at the very end of argv with no value).
        // The OptionException is raised inside the user's action via the
        // OptionValueCollection indexer's AssertValid path.
        context.Option?.Invoke(context);
        return unprocessed;
    }

    private static void RouteUnprocessed(
        List<string> unprocessed,
        Option? defaultHandler,
        OptionContext context,
        string argument)
    {
        if (defaultHandler is null)
        {
            unprocessed.Add(argument);
            return;
        }
        context.OptionValues.Add(argument);
        context.Option = defaultHandler;
        defaultHandler.Invoke(context);
    }

    private bool ParseArgument(string argument, OptionContext context)
    {
        // If a previous argument bound a multi-value option that's still
        // collecting, the current argument is its next value.
        if (context.Option is not null)
        {
            ParseValue(argument, context);
            return true;
        }

        var match = OptionTokenPattern().Match(argument);
        if (!match.Success)
        {
            return false;
        }

        var flag = match.Groups["flag"].Value;
        var name = match.Groups["name"].Value;
        string? separator = match.Groups["sep"].Success ? match.Groups["sep"].Value : null;
        string? value = match.Groups["value"].Success ? match.Groups["value"].Value : null;

        if (_optionsByName.TryGetValue(name, out var option))
        {
            context.OptionName = flag + name;
            context.Option = option;
            switch (option.OptionValueType)
            {
                case OptionValueType.None:
                    context.OptionValues.Add(name);
                    option.Invoke(context);
                    break;
                case OptionValueType.Optional:
                case OptionValueType.Required:
                    ParseValue(value, context);
                    break;
            }
            return true;
        }

        return ParseBoolToggle(argument, name, context)
            || ParseBundle(flag, string.Concat(name, separator, value), context);
    }

    private void ParseValue(string? value, OptionContext context)
    {
        var option = context.Option!;
        if (value is not null)
        {
            var parts = option.ValueSeparators is { } seps
                ? value.Split(seps, StringSplitOptions.None)
                : [value];
            foreach (var part in parts)
            {
                context.OptionValues.Add(part);
            }
        }

        if (context.OptionValues.Count == option.MaxValueCount ||
            option.OptionValueType == OptionValueType.Optional)
        {
            option.Invoke(context);
        }
        else if (context.OptionValues.Count > option.MaxValueCount)
        {
            throw new OptionException(
                MessageLocalizer(string.Format(
                    CultureInfo.InvariantCulture,
                    "Error: Found {0} option values when expecting {1}.",
                    context.OptionValues.Count,
                    option.MaxValueCount)),
                context.OptionName);
        }
    }

    private bool ParseBoolToggle(string argument, string name, OptionContext context)
    {
        if (name.Length < 1)
        {
            return false;
        }
        char suffix = name[^1];
        if (suffix != '+' && suffix != '-')
        {
            return false;
        }

        var rootName = name[..^1];
        if (!_optionsByName.TryGetValue(rootName, out var option))
        {
            return false;
        }

        string? value = suffix == '+' ? argument : null;
        context.OptionName = argument;
        context.Option = option;
        context.OptionValues.Add(value!);
        option.Invoke(context);
        return true;
    }

    private bool ParseBundle(string flag, string bundle, OptionContext context)
    {
        if (flag != "-")
        {
            return false;
        }

        for (int i = 0; i < bundle.Length; i++)
        {
            var optionToken = flag + bundle[i];
            var character = bundle[i].ToString();

            if (!_optionsByName.TryGetValue(character, out var option))
            {
                if (i == 0)
                {
                    return false;
                }
                throw new OptionException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        MessageLocalizer("Cannot bundle unregistered option '{0}'."),
                        optionToken),
                    optionToken);
            }

            switch (option.OptionValueType)
            {
                case OptionValueType.None:
                    InvokeWithSingleValue(context, optionToken, bundle, option);
                    break;
                case OptionValueType.Optional:
                case OptionValueType.Required:
                {
                    var remainder = bundle[(i + 1)..];
                    context.Option = option;
                    context.OptionName = optionToken;
                    ParseValue(remainder.Length != 0 ? remainder : null, context);
                    return true;
                }
                default:
                    throw new InvalidOperationException(
                        $"Unknown OptionValueType: {option.OptionValueType}");
            }
        }
        return true;
    }

    private static void InvokeWithSingleValue(OptionContext context, string name, string value, Option option)
    {
        context.OptionName = name;
        context.Option = option;
        context.OptionValues.Add(value);
        option.Invoke(context);
    }

    public void WriteOptionDescriptions(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        foreach (var option in _options)
        {
            int columnsWritten = 0;
            if (!WriteOptionPrototype(writer, option, MessageLocalizer, ref columnsWritten))
            {
                continue;
            }

            if (columnsWritten < OptionPrototypeColumnWidth)
            {
                writer.Write(new string(' ', OptionPrototypeColumnWidth - columnsWritten));
            }
            else
            {
                writer.WriteLine();
                writer.Write(new string(' ', OptionPrototypeColumnWidth));
            }

            var description = MessageLocalizer(NormaliseDescription(option));
            var lines = WrapDescription(description);
            writer.WriteLine(lines[0]);

            var continuationIndent = new string(' ', OptionPrototypeColumnWidth + 2);
            for (int i = 1; i < lines.Count; i++)
            {
                writer.Write(continuationIndent);
                writer.WriteLine(lines[i]);
            }
        }
    }

    private static bool WriteOptionPrototype(
        TextWriter writer,
        Option option,
        Func<string, string> localizer,
        ref int columnsWritten)
    {
        var names = option.Names;
        int firstVisible = NextVisibleName(names, 0);
        if (firstVisible == names.Length)
        {
            // Option exposes only "<>" → don't render a row at all.
            return false;
        }

        var prototypeBuilder = new StringBuilder();
        AppendName(prototypeBuilder, names[firstVisible], leading: true);

        for (int i = NextVisibleName(names, firstVisible + 1); i < names.Length; i = NextVisibleName(names, i + 1))
        {
            prototypeBuilder.Append(", ");
            AppendName(prototypeBuilder, names[i], leading: false);
        }

        // The names themselves are NOT routed through the localizer (a translation
        // of "--verbose" would break CLI parsing). The value tail "[=VALUE]" or
        // "=VALUE1:VALUE2" IS localized — but as one piece, not fragment-by-fragment
        // the way the original NDesk code did it.
        var valueTail = BuildValueTail(option);
        if (valueTail.Length > 0)
        {
            prototypeBuilder.Append(localizer(valueTail));
        }

        var prototypeText = prototypeBuilder.ToString();
        writer.Write(prototypeText);
        columnsWritten += prototypeText.Length;
        return true;

        static void AppendName(StringBuilder sb, string name, bool leading)
        {
            if (leading)
            {
                sb.Append(name.Length == 1 ? "  -" : "      --");
            }
            else
            {
                sb.Append(name.Length == 1 ? "-" : "--");
            }
            sb.Append(name);
        }
    }

    private static string BuildValueTail(Option option)
    {
        if (option.OptionValueType == OptionValueType.None)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        if (option.OptionValueType == OptionValueType.Optional)
        {
            builder.Append('[');
        }
        builder.Append('=').Append(GetArgumentName(0, option.MaxValueCount, option.Description));

        var separator = option.ValueSeparators is { Length: > 0 } seps ? seps[0] : " ";
        for (int i = 1; i < option.MaxValueCount; i++)
        {
            builder.Append(separator).Append(GetArgumentName(i, option.MaxValueCount, option.Description));
        }

        if (option.OptionValueType == OptionValueType.Optional)
        {
            builder.Append(']');
        }

        return builder.ToString();
    }

    private static int NextVisibleName(string[] names, int start)
    {
        while (start < names.Length && names[start] == DefaultHandlerName)
        {
            start++;
        }
        return start;
    }

    private static string GetArgumentName(int index, int maxIndex, string? description)
    {
        if (description is null)
        {
            return maxIndex == 1 ? "VALUE" : $"VALUE{index + 1}";
        }

        // Indexed token "{0:NAME}" wins for multi-value options; bare "{NAME}"
        // covers the single-value case.
        string[] tokenStarts = maxIndex == 1
            ? ["{0:", "{"]
            : [$"{{{index}:"];

        foreach (var tokenStart in tokenStarts)
        {
            int start = -1, scan = 0;
            do
            {
                start = description.IndexOf(tokenStart, scan, StringComparison.Ordinal);
            }
            while (start >= 0 && scan != 0 && description[scan++ - 1] == '{');

            if (start == -1)
            {
                continue;
            }
            int end = description.IndexOf('}', start);
            if (end == -1)
            {
                continue;
            }
            return description.Substring(start + tokenStart.Length, end - start - tokenStart.Length);
        }

        return maxIndex == 1 ? "VALUE" : $"VALUE{index + 1}";
    }

    private static string NormaliseDescription(Option option)
    {
        var description = option.Description;
        if (string.IsNullOrEmpty(description))
        {
            return string.Empty;
        }

        var sb = new StringBuilder(description.Length);
        int start = -1;
        for (int i = 0; i < description.Length; i++)
        {
            switch (description[i])
            {
                case '{':
                    if (i == start)
                    {
                        sb.Append('{');
                        start = -1;
                    }
                    else if (start < 0)
                    {
                        start = i + 1;
                    }
                    break;
                case '}':
                    if (start < 0)
                    {
                        if (i + 1 == description.Length || description[i + 1] != '}')
                        {
                            // Phase 0 modernisation: was InvalidOperationException
                            // with a generic message — FormatException is the
                            // BCL-conventional choice for malformed input and
                            // the message identifies the offending option.
                            throw new FormatException(
                                $"Unbalanced '}}' in description for option '{option.Prototype}'. Use '}}}}' to escape.");
                        }
                        i++;
                        sb.Append('}');
                    }
                    else
                    {
                        sb.Append(description.AsSpan(start, i - start));
                        start = -1;
                    }
                    break;
                case ':':
                    if (start < 0)
                    {
                        goto default;
                    }
                    start = i + 1;
                    break;
                default:
                    if (start < 0)
                    {
                        sb.Append(description[i]);
                    }
                    break;
            }
        }
        return sb.ToString();
    }

    private static List<string> WrapDescription(string description)
    {
        var lines = new List<string>();
        if (string.IsNullOrEmpty(description))
        {
            lines.Add(string.Empty);
            return lines;
        }

        int start = 0;
        while (true)
        {
            int end = FindLineBreak(description, start, DescriptionLineWidth);
            bool needsContinuationDash = false;

            if (end < description.Length)
            {
                char terminator = description[end];
                if (terminator == '-' || (char.IsWhiteSpace(terminator) && terminator != '\n'))
                {
                    end++;
                }
                else if (terminator != '\n')
                {
                    needsContinuationDash = true;
                    end--;
                }
            }

            var line = description[start..end];
            if (needsContinuationDash)
            {
                line += "-";
            }
            lines.Add(line);

            start = end;
            if (start < description.Length && description[start] == '\n')
            {
                start++;
            }
            if (end >= description.Length)
            {
                break;
            }
        }
        return lines;
    }

    private static int FindLineBreak(string description, int start, int width)
    {
        int end = Math.Min(start + width, description.Length);
        int lastSeparator = -1;
        for (int i = start; i < end; i++)
        {
            switch (description[i])
            {
                case ' ':
                case '\t':
                case '\v':
                case '-':
                case ',':
                case '.':
                case ';':
                    lastSeparator = i;
                    break;
                case '\n':
                    return i;
            }
        }
        return lastSeparator == -1 || end == description.Length ? end : lastSeparator;
    }
}
