using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace mcmli
{
    partial class Program
    {
        // Directory of executing binary
        public static string exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        public static string CurrentFS = GetFS(exeDir);
        // List of files to be fetched (say, a modlist) stored in '.install.remote'
        public static string remotesList = ".install.remote";
        public static List<string> modlists;
        // Do any files match *server*?
        public static bool isServer = Directory.EnumerateFiles(".", "*server*").Any();
        // What operating system are we running on?
        public static bool isWindows = IsOS(OSPlatform.Windows),
                           isOSX     = IsOS(OSPlatform.OSX),
                           isLinux   = IsOS(OSPlatform.Linux);
        // Directory where we store mods to later link them to modpack directory
        public static string ModCache;
        // Do we want to use a cache?
        public static bool useCache = true;
        // Do we want to use symlinks to the cache?
        public static bool SymlinkToCache = false;

        // Directory to store persistent config.
        // We set this to C:\ProgramData\mcmli\config on windows and
        // ~/.config/mcmli on *nix
        public static DirectoryInfo ConfigDir;
        // User's home directory
        public static DirectoryInfo HomeDir = new DirectoryInfo(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        public static FileInfo CacheList;
    }
}
