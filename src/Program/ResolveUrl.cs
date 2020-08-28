using System;
using System.IO;
using System.Text.RegularExpressions;

namespace mcmli
{
    partial class Program
    {
        public static void ResolveUrl(string line, string dir,
            bool alwaysFetch = false, bool? clientMod = null)
        {
            /*
             * Resolves a url in a modlist such that client/server only mods
             * are only downloaded/present in their respective environments.
             * URLs will be skipped if already downloaded by default but this can be
             * overridden by the alwaysFetch argument.
             */
            if (String.IsNullOrWhiteSpace(line)) return; // Skip empty lines


            //  Ignore comments in URLs only if they are preceeded by whitespace,
            //  that is, not part of the URL.

            string url = Regex.Replace(line, @"[\s]+#.*", "");
            var uri = new Uri(url);
            string filename = Path.GetFileName(uri.LocalPath);

            // If mod is client only and the current instance is a server, remove it.
            if (clientMod == true & isServer)
            {
                if (File.Exists(Path.Combine(dir, filename)))
                {
                    Console.WriteLine($"Removing client only mod: {filename}");
                    File.Delete(Path.Combine(dir, filename));
                }
                return;
            }
            // If mod is server only and the current instance is a client, remove it.
            else if (clientMod == false & !isServer)
            {
                if (File.Exists(Path.Combine(dir, filename)))
                {
                    Console.WriteLine($"Removing server only mod: {filename}");
                    File.Delete(Path.Combine(dir, filename));
                }
                return;
            }

            if (useCache && !url.EndsWith(".cfg",true,null) && !url.EndsWith(".modlist",true,null))
            {
                GetFileFromCache(url, dir, alwaysFetch);
                return;
            }

            if (alwaysFetch)
            {
                Console.WriteLine("[{0}] Downloading {1} ...", dir, filename);
                DownloadFile(url, dir);
            }
            else if (File.Exists(Path.Combine(dir, filename)))
            {
                Console.WriteLine("[{0}] Skipping existing file: {1}", dir, filename);
            }
            else
            {
                Console.WriteLine("[{0}] Downloading {1} ...", dir, filename);
                DownloadFile(url, dir);
            }
        }
    }
}
