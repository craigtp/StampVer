namespace stampver
{
    internal class ProcessedLineResult
    {
        public ProcessedLineResult(string line, bool lineWasModified, string newVersionNumber)
        {
            Line = line;
            LineWasModified = lineWasModified;
            NewVersionNumber = newVersionNumber;
        }

        public string Line { get; }
        public bool LineWasModified { get; }
        public string NewVersionNumber { get; }
    }
}
