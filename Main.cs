using System;
using System.IO;

namespace mcmli
{
    partial class Program
    {
        public static void Main(string[] args)
        {
            if (exeDir.Length == 0) exeDir = ".";

            // Directory to execute in can be passed as first argument.
            if (args.Length != 0 && !String.IsNullOrWhiteSpace(args[0]))
                exeDir = args[0];
            // If we can, we want to run this code in the same directory as the
            // executable.
            try
                { Directory.SetCurrentDirectory(exeDir); }
            catch
                { Console.Error.WriteLine($"Warning: Could not cd to directory '{exeDir}', the installer may not run correctly."); }

            CurrentFS = GetFS(exeDir);

            // Set and create config directory.
            if (isWindows)
                ConfigDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"mcmli","config"));
            if (isOSX || isLinux)
                ConfigDir = new DirectoryInfo(Path.Combine(HomeDir.FullName,".config","mcmli"));

            try { Directory.CreateDirectory(ConfigDir.FullName); }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Error: Unable to create config directory '{ConfigDir.FullName}'\n"
                        + $"Exception: {e.Message}");
            }

            if (SetCacheLocation() != 0)
            {
                Console.Error.WriteLine("Failed to set a cache location.");
                ExitHandler(retVal: 3);
            }

            Console.WriteLine($"Current cache is: {ModCache}");

            GetModlists(); // List of all files matching *.modlist in base directory, sorted by name
            FetchRemoteList();
            GetModlists(); // Regenerate modlists list, files might have changed since fetching remotesList

            foreach (string list in modlists)
            {
                Console.WriteLine("Resolving {0} ...", Path.GetFileName(list));
                ResolveModlist(list);
            }

            ExitHandler(sayThis: "Done! ");
        }
    }
}
