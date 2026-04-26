using System;

namespace stampver
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            IIOWrapper ioWrapper = new IoWrapper();
            try
            {
                var stampverProgram = new Stampver(ioWrapper, args);
                return stampverProgram.Run();
            }
            catch (Exception ex)
            {
                // Catch-all safety net for unhandled exceptions (permission denied
                // on file write, locked file, malformed input the parser didn't
                // anticipate, etc.). We deliberately do NOT print the stack trace
                // — it leaks internal paths and types to the user's terminal and
                // adds nothing actionable for end users. Set DOTNET_TraceFile or
                // run from a debugger if a stack trace is needed for diagnosis.
                ioWrapper.WriteToStdErr($"stampver: unexpected error: {ex.Message}");
                return ExitCodes.UnexpectedError;
            }
        }
    }
}
