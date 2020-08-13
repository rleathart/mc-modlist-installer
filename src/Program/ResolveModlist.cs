using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace mcmli
{
    partial class Program
    {
        public static void ResolveModlist(string list)
        {
            // Download files in modlist
            string dir = "mods"; // If no directory given, download to "mods".
            bool alwaysFetch = false; // Skip existing files by default
            bool? clientMod = null; // All files are common by default
            List<string> modlists = new List<string>(Directory.EnumerateFiles(".", "*.modlist"));
            List<string> mod_dirs = new List<String>();
            List<string> mods = new List<string>();

            foreach (string inputLine in File.ReadLines(list))
            {
                string line = inputLine;
                // Skip lines containing only whitespace
                if (Regex.Match(line, @"^[\s]*$").Success) continue;

                // Trim leading whitespace
                line = Regex.Replace(line, @"^[\s]*(?=[^\s])", "");

                bool isUrl = line.StartsWith("http");
                if (!isUrl)
                {
                    line = Regex.Replace(inputLine, @"#.*", @""); // ignore comments

                    // Skip this line if is is not a valid control directive. That is,
                    // it doesn't contain anything inside square or angle brackets.
                    if (!Regex.Match(line, @"[\s]*(\[[^]]+\]|<[^>]+>)").Success) continue;

                    if (ExtractFromDelims(line, "[]").Count > 0)
                    {
                        // Do this for every directory specification.
                        alwaysFetch = false;
                        clientMod = null;
                    }

                    var delimContent = ExtractFromDelims(line, "[]");
                    if (delimContent.Count > 0 && delimContent[0].ToString().Length != 0)
                        dir = delimContent[0].ToString();

                    if (ExtractFromDelims(line, "<>").Any(x =>
                          x.ToString().ToLower() == "always-fetch")) alwaysFetch = true;
                    if (ExtractFromDelims(line, "<>").Any(x =>
                          x.ToString().ToLower() == "smart-fetch")) alwaysFetch = false;

                    if (ExtractFromDelims(line, "<>").Any(x =>
                          x.ToString().ToLower() == "server-only")) clientMod = false;
                    if (ExtractFromDelims(line, "<>").Any(x =>
                          x.ToString().ToLower() == "client-only")) clientMod = true;
                    if (ExtractFromDelims(line, "<>").Any(x =>
                          x.ToString().ToLower() == "common")) clientMod = null;

                    continue;
                }

                if (!mod_dirs.Contains(dir)) mod_dirs.Add(dir);

                Directory.CreateDirectory(dir);
                ResolveUrl(line, dir, alwaysFetch, clientMod);
            }

            foreach (string mod_dir in mod_dirs)
            {
                mods.AddRange(Directory.GetFiles(mod_dir, "*", SearchOption.TopDirectoryOnly).ToList());
            }

            foreach (string i in mods) // Remove local mods not in a modlist.
            {
                string filename;
                string local_filename = Path.GetFileName(i);
                string local_dir = Path.GetDirectoryName(i);

                // We want to re-encode filenames
                // NOTE: This may need to change if some mod urls contain unconventional
                //       characters.
                filename = Regex.Replace(local_filename, @" ", @"%20");
                filename = Regex.Replace(filename, @"\+", @"%2B");
                filename = Regex.Replace(filename, @"#", @"%23");
                filename = Regex.Replace(filename, @":", @"%3A");
                filename = Regex.Replace(filename, @";", @"%3B");

                // Skip configs, old ones are harmless.
                if (filename.EndsWith("cfg", true, null)) continue;

                if (!modlists.Any(x => x != null &&
                      File.ReadAllText(x).Contains(filename)))
                {
                    Console.WriteLine("Found {0} in {1} directory but not in any modlist. It may be old.", local_filename, local_dir);

                    string resp;
                    string[] validResps = { "y", "n", "" };

                    do
                    {
                        Console.Write("Do you want to remove it? [Y/n]: ");
                        resp = Console.ReadLine().ToLower();
                    }
                    while (!validResps.Any(s => s.Equals(resp)));

                    if (resp == "y" || resp == "")
                    {
                        Console.WriteLine($"Removing {local_filename}");
                        File.Delete(i);
                    }
                }
            }
        }
    }
}
