using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

class Program {
  public static void Main(string[] args) {

    string exeFileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
    string exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

    if (Directory.Exists(exeDir))
    {
      if (Regex.Match(exeFileName, @"install").Success)
      {
        Directory.SetCurrentDirectory(exeDir);
      }
      else if (!Regex.Match(exeFileName, @"mono").Success)
      {
        Console.WriteLine("Warning: {0} does not look like 'install', the installer is not guarenteed to run in the correct directory. Did you rename the installer?",
            exeFileName);
      }
    }
    else
    {
      Console.Write("Directory {0} does not exist.", exeDir);
      Console.ReadKey();
      return;
    }

    if (!File.Exists("modlist.txt") || !File.Exists("configlist.txt")) {
      Console.WriteLine("Missing one or both of modlist.txt and configlist.txt");
      Console.Write("Press any key to exit...");
      Console.ReadKey();
      return;
    }

    // Create required directories
    string[] reqdirs = {"mods", "config"};
    foreach (string i in reqdirs) {
      Directory.CreateDirectory(i);
    }

    string[] modlists = new string[2];
    modlists[0] = "modlist.txt";

    if (File.Exists(@"modlist_personal.txt")) {
      modlists[1] = @"modlist_personal.txt";
    }

    foreach (string i in modlists) {
      if ( i != null ) {
        ResolveModlist(i,"mods");
      }
    }

    Console.WriteLine("Fetching configs ...");
    ResolveModlist("configlist.txt","config",alwaysFetch:true);

    List<string>mods = new List<string>();

    mods = Directory.GetFiles(
        "mods","*",SearchOption.TopDirectoryOnly).ToList();

    foreach (string i in mods) {
      string filename;
      string local_filename = Path.GetFileName(i);
      string local_dir = Path.GetDirectoryName(i);
      // We want to re-encode filenames
      filename = Regex.Replace(local_filename, @" ", @"%20");
      filename = Regex.Replace(filename, @"\+", @"%2B");

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

  private static void ResolveModlist(string list,string dir, bool alwaysFetch = false) {
    // Download files in modlist
    System.IO.StreamReader modlist = new System.IO.StreamReader(list);
    string line;

    // Do any files match *server*?
    bool isServer = Directory.EnumerateFiles(".","*server*").Any();

    while((line = modlist.ReadLine()) != null) {

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
        continue;
      }
      else if (clientMod == false & !isServer) {
        if (File.Exists(Path.Combine(dir,filename))) {
          Console.WriteLine($"Removing server only mod: {filename}");
          File.Delete(Path.Combine(dir,filename));
        }
        continue;
      }

      if (alwaysFetch) {
        Console.WriteLine("Downloading {0} ...", filename);
        DownloadFile(url, dir);
      }
      else if (File.Exists(Path.Combine(dir,filename))) {
        Console.WriteLine("Skipping existing file: {0}",filename);
      }
      else if (File.Exists(Path.Combine("Flan",filename))) {
        Console.WriteLine("Skipping existing file: {0}",filename);
      }
      else {
        Console.WriteLine("Downloading {0} ...", filename);
        DownloadFile(url, dir);
      }
    }
    modlist.Close();
  }
}
