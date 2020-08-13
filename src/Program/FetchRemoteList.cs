using System;
using System.IO;
using System.Linq;

namespace mcmli
{
    partial class Program
    {
        public static void FetchRemoteList(string remotesList)
        {
            if (!File.ReadLines(remotesList).Any())
            {
                // Don't do anything if remotesList is empty
                Console.Error.WriteLine("{0} is empty!", remotesList);
                return;
            }

            Console.WriteLine("Fetching remote files ...");
            foreach (string line in File.ReadLines(remotesList))
            {
                if (line.Length != 0) // Don't resolve empty lines
                    ResolveUrl(line, ".", alwaysFetch: true);
            }
        }
    }
}
