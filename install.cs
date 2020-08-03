using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;

class Program {
  private static void ExitHandler()
  {
    Console.Write("Press any key to exit...");
    Console.ReadKey();
    System.Environment.Exit(1);
  }
  private static void FetchRemoteList(string remotesList)
  {
    if (!File.ReadLines(remotesList).Any())
    {
      Console.WriteLine("{0} is empty!",remotesList);
      return;
    }

    Console.WriteLine("Fetching remote files ...");
    foreach (string line in File.ReadLines(remotesList))
    {
      if (line != "")
        ResolveUrl(line,".", alwaysFetch: true);
    }
  }
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
      Console.Write("Warning: Could not cd to directory '{0}', the installer may not run correctly.", exeDir);
    }

    string remotesList = ".install.remote";
    // List of files to be fetched (say, a modlist) stored in '.install.remote'
    if (File.Exists(remotesList)) FetchRemoteList(remotesList);

    List<string> modlists = new List<string>(Directory.EnumerateFiles(".","*.modlist"));
    if (!modlists.Any())
    {
      Console.WriteLine("No modlists found.");
      Console.Write("Please specify a remote: ");
      string resp = Console.ReadLine();
      if (resp != "")
      {
        WebClient client = new WebClient();
        client.DownloadFile(resp, remotesList);

        FetchRemoteList(remotesList);
      }
    }

    foreach (string list in modlists)
    {
      Console.WriteLine("Resolving {0} ...", Path.GetFileName(list));
      ResolveModlist(list);
    }

    Console.Write("Done, press any key to exit...");
    Console.ReadKey();
  }

  private static void DownloadFile(string url, string dir = ".") {
    var uri = new Uri(url);
    string filename = Path.GetFileName(uri.LocalPath);

    WebClient client = new WebClient();

    client.DownloadFileAsync(uri, Path.Combine(dir,filename));
    while (client.IsBusy) { Thread.Sleep(100); }
  }

  private static void ResolveUrl(string line, string dir, bool alwaysFetch = false)
  {
    bool isServer = Directory.EnumerateFiles(".","*server*").Any();

    string url = Regex.Replace(line," #.*","");
    var uri = new Uri(url);
    string filename = Path.GetFileName(uri.LocalPath);
    bool? clientMod = null;

    if (Regex.Match(line," #Client only").Success) {
      clientMod = true;
    }
    else if (Regex.Match(line, " #Server only").Success) {
      clientMod = false;
    }

    if (clientMod == true & isServer) {
      if (File.Exists(Path.Combine(dir,filename))) {
        Console.WriteLine($"Removing client only mod: {filename}");
        File.Delete(Path.Combine(dir,filename));
      }
      return;
    }
    else if (clientMod == false & !isServer) {
      if (File.Exists(Path.Combine(dir,filename))) {
        Console.WriteLine($"Removing server only mod: {filename}");
        File.Delete(Path.Combine(dir,filename));
      }
      return;
    }

    if (alwaysFetch) {
      Console.WriteLine("[{0}] Downloading {1} ...",dir, filename);
      DownloadFile(url, dir);
    }
    else if (File.Exists(Path.Combine(dir,filename))) {
      Console.WriteLine("[{0}] Skipping existing file: {1}",dir, filename);
    }
    else if (File.Exists(Path.Combine("Flan",filename))) {
      Console.WriteLine("[{0}] Skipping existing file: {1}",dir, filename);
    }
    else {
      Console.WriteLine("[{0}] Downloading {1} ...",dir, filename);
      DownloadFile(url, dir);
    }
  }

  private static void ResolveModlist(string list) {
    // Download files in modlist
    System.IO.StreamReader modlist = new System.IO.StreamReader(list);
    string line;
    string dir;
    bool alwaysFetch = false;
    dir = "mods";
    List<string> modlists = new List<string>(Directory.EnumerateFiles(".","*.modlist"));
    List<string> mod_dirs = new List<String>();
    List<string> mods = new List<string>();

    while((line = modlist.ReadLine()) != null)
    {
      if (Regex.Match(line,@"^#[^#]+").Success)
      {
        dir = Regex.Replace(line,@"^#", "");
        alwaysFetch = false; // Reset alwaysFetch on new directory
        continue;
      }
      if (Regex.Match(line,@"^##alwaysFetch").Success)
      {
        alwaysFetch = true;
        continue;
      }

      if (!mod_dirs.Contains(dir)) mod_dirs.Add(dir);

      Directory.CreateDirectory(dir);
      ResolveUrl(line,dir,alwaysFetch);

    }

    foreach (string mod_dir in mod_dirs)
    {
      mods.AddRange(Directory.GetFiles(mod_dir,"*",SearchOption.TopDirectoryOnly).ToList());
    }


    foreach (string i in mods) // Remove local mods not in a modlist.
    {
      string filename;
      string local_filename = Path.GetFileName(i);
      string local_dir = Path.GetDirectoryName(i);

      CultureInfo ci = new CultureInfo("en-GB");

      // We want to re-encode filenames
      filename = Regex.Replace(local_filename, @" ", @"%20");
      filename = Regex.Replace(filename, @"\+", @"%2B");

      // Skip configs, old ones are harmless.
      if (filename.EndsWith("cfg", true, ci)) continue;

      if (!modlists.Any(x => x != null &&
            File.ReadAllText(x).Contains(filename))) {
        Console.WriteLine("Found {0} in {1} directory but not in any modlist. It may be old.",local_filename,local_dir);

        string resp;
        string[] validResps = {"y","n",""};

        do {
          Console.Write("Do you want to remove it? [Y/n]: ");
          resp = Console.ReadLine().ToLower();
        }
        while (!validResps.Any(s => s.Equals(resp)));

        if (resp == "y" || resp == "") {
          Console.WriteLine($"Removing {local_filename}");
          File.Delete(i);
        }
      }
    }
    modlist.Close();
  }
}
