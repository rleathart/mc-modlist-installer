using System;
using System.IO;

namespace mcmli
{
    partial class Program
    {
        public static string GetFS(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) return String.Empty;
            string PathToCheck = input;
            FileInfo f = new FileInfo(input);
            DirectoryInfo d = new DirectoryInfo(input);

            if (isWindows)
            {
                return Path.GetPathRoot(f.FullName);
            }
            if (isLinux || isOSX)
            {
                // We want to walk up the file path of input until we find a file
                // or directory that exists.

                // If a file with path 'input' exists, then we can just use df
                // on that path.
                if (f.Exists) PathToCheck = f.FullName;
                // Similarly if a directory with path 'input' exists
                else if (d.Exists) PathToCheck = d.FullName;
                // Otherwise we walk up the path of 'input' until we find a
                // directory that exists and then we use df on that directory.
                else
                {
                    while (!d.Exists)
                    {
                        d = new DirectoryInfo(Path.GetDirectoryName(d.FullName));
                    }
                    PathToCheck = d.FullName;
                }

                return $"df {PathToCheck} | tail -n 1 | cut -d ' ' -f 1".Shell().StdOut;;

            }

            return String.Empty;
        }
    }
}
