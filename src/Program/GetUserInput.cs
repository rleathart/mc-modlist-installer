using System;
using System.Linq;

namespace mcmli
{
    partial class Program
    {
        public static string GetUserInput(string prompt = "", string[] validResps = null)
        {
            string resp;
            do
            {
                Console.Write(prompt);
                resp = Console.ReadLine();
                if (validResps == null) break;
            }
            while (!validResps.Any(x => x.Equals(resp)));

            return resp;
        }
    }
}
