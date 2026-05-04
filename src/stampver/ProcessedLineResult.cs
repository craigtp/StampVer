namespace stampver
{
    internal sealed record ProcessedLineResult(string Line, bool LineWasModified, string NewVersionNumber);
}
