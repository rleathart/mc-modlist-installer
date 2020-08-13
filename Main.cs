using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace mcmli
{
    partial class Program
    {
        public static void Main(string[] args)
        {

            string exeFileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            string exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            // If we can, we want to run this code in the same directory as the
            // executable.
            if (Directory.Exists(exeDir))
            {
                if (!Regex.Match(exeFileName, @"mono").Success)
                {
                    Directory.SetCurrentDirectory(exeDir);
                }
            }
            else
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
                if (resp != "") // If the user gives a URL, download it, otherwise do nothing
                {
                    WebClient client = new WebClient();
                    client.DownloadFile(resp, remotesList);

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
