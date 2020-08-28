using System;
using System.Net;
using System.IO;
using System.Linq;

namespace mcmli
{
    partial class Program
    {
        public static void FetchRemoteList()
        {
            if (!modlists.Any()) // No modlists, user probably wants a remote
            {
                Console.Error.WriteLine("No modlists found.");

                // Ask user for remote URL (holding the raw content for a .install.remote)
                string resp = GetUserInput("Please specify a remote: ");
                if (resp != "") // If the user gives a URL, download it, otherwise do nothing
                {
                    WebClient client = new WebClient();
                    try { client.DownloadFile(resp, remotesList); }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"Could not download URL {resp}");
                        Console.Error.WriteLine($"Exception: {e.Message}");
                        ExitHandler(2);
                    }
                }
            }

            if (!File.Exists(remotesList)) return;

            if (!File.ReadLines(remotesList).Any())
            {
                // Don't do anything if remotesList is empty
                Console.Error.WriteLine("{0} is empty!", remotesList);
                return;
            }

            Console.WriteLine("Fetching remote files ...");
            foreach (string line in File.ReadLines(remotesList))
            {
                ResolveUrl(line, ".", alwaysFetch: true);
            }
        }
    }
}
