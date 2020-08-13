using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Collections.Generic;

namespace mcmli
{
    partial class Program
    {
        public static void Main(string[] args)
        {
            // If we can, we want to run this code in the same directory as the
            // executable.
            string exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            try
            {
                Directory.SetCurrentDirectory(exeDir);
            }
            catch
            {
                Console.Error.Write("Warning: Could not cd to directory '{0}', the installer may not run correctly.", exeDir);
            }

            // List of files to be fetched (say, a modlist) stored in '.install.remote'
            string remotesList = ".install.remote";
            if (File.Exists(remotesList)) FetchRemoteList(remotesList);

            // List of all files matching *.modlist in base directory, sorted by name
            List<string> modlists = new List<string>(Directory.EnumerateFiles(".", "*.modlist")
                .OrderBy(f => f));
            if (!modlists.Any())
            {
                Console.Error.WriteLine("No modlists found.");

                // Ask user for remote URL (holding the raw content for a .install.remote)
                // if there are no modlists
                Console.Write("Please specify a remote: ");
                string resp = Console.ReadLine();
                if (resp.StartsWith("http",true,null)) // If the user gives a URL, download it, otherwise do nothing
                {
                    WebClient client = new WebClient();
                    try { client.DownloadFile(resp, remotesList); }
                    catch
                    {
                      Console.Error.WriteLine($"{resp} is not a valid URL.");
                      ExitHandler(2);
                    }

                    FetchRemoteList(remotesList);
                }
            }
            // Regenerate modlists list, files might have changed since fetching
            // remotesList
            modlists = new List<string>(Directory.EnumerateFiles(".", "*.modlist")
                .OrderBy(f => f));

            foreach (string list in modlists)
            {
                Console.WriteLine("Resolving {0} ...", Path.GetFileName(list));
                ResolveModlist(list);
            }

            ExitHandler(sayThis: "Done! ");
        }
    }
}
