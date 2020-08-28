using System;
using System.IO;

namespace mcmli
{
    partial class Program
    {
        public static void GetFileFromCache(string URL, string DestDir,
                bool alwaysFetch = false, bool? clientMod = null)
        {
            Uri uri = new Uri(URL);
            FileInfo File = new FileInfo(uri.LocalPath);

            FileInfo CachedFile = new FileInfo(Path.Combine(ModCache,File.Name));
            FileInfo LocalFile = new FileInfo(Path.Combine(exeDir,DestDir,File.Name));

            if (LocalFile.Exists && !CachedFile.Exists)
            {
                // Re-link if the file doesn't exist in the cache.
                LocalFile.Delete();
            }

            if (CachedFile.Exists && !alwaysFetch)
            {
                // If the file exists in cache and we don't want to refetch it
                // create a link from the cache to destination directory.
                if (!LocalFile.Exists)
                {
                    Console.WriteLine($"[{DestDir}] Linking {File.Name} ...");
                    int CreateLinkReturnValue = FileOp.CreateLink(CachedFile.FullName, Path.Combine(DestDir,File.Name),SymlinkToCache);
                    if (CreateLinkReturnValue == 1)
                    {
                        Console.Error.WriteLine("Error: Insufficient privilege to create symbolic links. Are you running as Admin?");
                        ExitHandler(sayThis: "Cannot continue. ");
                    }
                }
                else
                    Console.WriteLine($"[{DestDir}] Skipping existing file: {File.Name}");
            }
            else
            {
                Console.WriteLine("[{0}] Downloading {1} ...", DestDir, File.Name);
                DownloadFile(URL,ModCache);
                // Re-call GetFileFromCache but this time with alwaysFetch: false
                // since we have just downloaded the file.
                GetFileFromCache(URL, DestDir, false, clientMod);
            }
        }
    }
}
