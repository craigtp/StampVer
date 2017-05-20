namespace stampver
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            IIOWrapper ioWrapper = new IoWrapper();
            var stampverProgram = new Stampver(ioWrapper, args);
            stampverProgram.Run();
        }
    }
}
