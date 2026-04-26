namespace stampver
{
    /// <summary>
    /// Process exit codes returned by stampver. Modelled loosely on BSD sysexits
    /// so that CI pipelines and shell scripts can distinguish between success and
    /// well-known failure modes without parsing stdout/stderr.
    /// </summary>
    public static class ExitCodes
    {
        /// <summary>The run completed successfully.</summary>
        public const int Success = 0;

        /// <summary>The user supplied invalid command-line arguments.</summary>
        public const int UsageError = 64;
    }
}
