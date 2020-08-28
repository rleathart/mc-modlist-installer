using System;
using System.IO;
using System.Linq;

namespace mcmli
{
    partial class Program
    {
        /*
         * NOTE: A potential pitfall here is that the user might decide to
         *       install a modpack on a different drive than the cache. This
         *       means we might end up storing more data on disk than necessary.
         *
         *       We can perhaps avoid this by checking if exeDir is on a
         *       different drive to the cache and then prompting the user with
         *       the following options:
         *       - Use symlinks instead
         *       - Create a new cache on the current drive
         *       - Create a new cache on the current drive and clear the default
         *         cache
         *       - Don't use a cache for this installation.
         *
         *       Checking what drive a file is on may be non trivial. On *nix
         *       we can parse the output of df: `df $file | tail -n 1 | cut -d ' ' -f 1`
         */

        public static int SetCacheLocation()
        {
            // Store mods in C:\ProgramData\mcmli\cache on windows
            if (isWindows)
                ModCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "mcmli", "cache");
            // Store mods in ~/.cache/mcmli on *nix
            if (isLinux || isOSX)
                ModCache = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "mcmli");

            CacheList = new FileInfo(Path.Combine(ConfigDir.FullName,"cache.list"));
            string altPath = String.Empty;

            /*
             * If CacheList is empty, ask user to set default cache location.
             * Then we want to loop through CacheList and see if there is
             * a cache on CurrentFS. If not, we ask the user what to do
             */

            if (!CacheList.Exists)
                CacheList.Create().Close();

            if (!File.ReadAllLines(CacheList.FullName).Any()) // CacheList is empty.
            {
                altPath = GetUserInput($"Mods will be cached in {ModCache}.\nPress enter to use the defult location, or specify a path: ");
                if (!String.IsNullOrWhiteSpace(altPath))
                    ModCache = altPath;
                try
                {
                    Directory.CreateDirectory(ModCache);
                    File.AppendAllText(CacheList.FullName, ModCache + Environment.NewLine);
                    /* return 0; */
                }
                catch (Exception e)
                { Console.Error.WriteLine($"Error: Could not create cache directory '{ModCache}'.\n"
                        + $"Exception: {e.Message}"); return 1; }
            }

            foreach (string Line in File.ReadAllLines(CacheList.FullName))
            {
                if (CurrentFS == GetFS(Line))
                {
                    ModCache = Line;
                    return 0;
                }
            }

            if (CurrentFS != GetFS(ModCache))
            {
                Console.WriteLine("You are currently installing to a directory on a different filesystem than the default cache.");
                Console.WriteLine("You have a few options here:\n" +
                        "1. Use symbolic links rather than hard links (Recommended. Requires Admin rights on Windows.)\n" +
                        "2. Create a new cache on the current filesystem\n" +
                        "3. Delete all previous caches and create a new cache on the current filesystem\n" +
                        "4. Don't use a cache for this installation");
                switch (GetUserInput("Please select an option (default 1): ",
                            new string[] {"1","2","3","4",""}))
                {
                    case "1":
                        SymlinkToCache = true;
                        break;
                    case "2":
                        if (isWindows)
                        {
                            FileInfo f = new FileInfo(ModCache);
                            string ModCacheWithoutDrive = f.FullName.Substring(GetFS(exeDir).Length);
                            ModCache = Path.Combine(GetFS(exeDir),ModCacheWithoutDrive);

                            SetNewCache();
                        }
                        if (isOSX || isLinux)
                        {
                            SetNewCache();
                        }
                        break;

                    case "3":
                        string userResp;
                        foreach (string Line in File.ReadAllLines(CacheList.FullName))
                        {
                            if (Directory.Exists(Line))
                            {
                                string[] validResps = {"Y","N","y","n",""};
                                userResp = GetUserInput($"OK to remove directory {Line} [Y/n]? ",validResps).ToLower();
                                if ( userResp == "y" || userResp == "" )
                                {
                                    Directory.Delete(Line, recursive: true);
                                    // Remove Line from CacheList
                                    File.WriteAllLines(CacheList.FullName,
                                            File.ReadLines(CacheList.FullName).Where(
                                                l => l != Line).ToList());
                                }
                            }
                            else
                            {
                                // Remove Line from CacheList
                                File.WriteAllLines(CacheList.FullName,
                                        File.ReadLines(CacheList.FullName).Where(
                                            l => l != Line).ToList());
                            }
                        }
                        goto case "2";
                    case "4":
                        useCache = false;
                        break;

                    default:
                        goto case "1";
                }

                if (!String.IsNullOrWhiteSpace(altPath)) ModCache = altPath;
                Directory.CreateDirectory(ModCache);

                string SetNewCache()
                {
                    bool canProceed;
                    string altPath;
                    do
                    {
                        canProceed = true;
                        if (isWindows) altPath = GetUserInput($"Specify cache directory [default: {ModCache}]: ");
                        else
                        {
                            do altPath = GetUserInput($"Specify cache directory: ");
                            while (String.IsNullOrWhiteSpace(altPath));
                        }
                        if (altPath != "")
                        {
                            if (GetFS(exeDir) != GetFS(altPath))
                            {
                                canProceed = false;
                                Console.Error.WriteLine($"Error: {altPath} is not on the same filesystem as {exeDir}.");
                            }
                            else
                            {
                                ModCache = altPath;
                            }
                        }
                    } while (!canProceed);

                    File.AppendAllText(CacheList.FullName,ModCache + Environment.NewLine);
                    return ModCache;
                }
            }

            return 0;
        }
    }
}
