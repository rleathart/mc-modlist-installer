using System;

namespace mcmli
{
    partial class Program
    {
        public static void ExitHandler(int retVal = 0, string sayThis = "")
        {
            if (sayThis.Length != 0) Console.Write(sayThis);
            Console.Write("Press any key to exit...");
            Console.ReadKey();
            Console.WriteLine();
            System.Environment.Exit(retVal);
        }
    }
}
