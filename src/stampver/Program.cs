namespace stampver
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            IIOWrapper ioWrapper = new IoWrapper();
            var stampverProgram = new Stampver(ioWrapper, args);
            return stampverProgram.Run();
        }
    }
}
