using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace mcmli
{
    partial class FileOp
    {
        public static int CreateLink(string source, string target, bool symbolic = false)
        {
            /* Returns:  0 on success
             *          -1 if process could not be started
             *          -2 if Command or Args are empty
             *           1 if not admin on windows.
             */
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                 isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
                 isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            string Command, Args, errMsg; Command = Args = errMsg = String.Empty;
            if (isLinux || isOSX)
            {
                Command = "/bin/ln";
                if (symbolic) Args = $"-s \"{source}\" \"{target}\"";
                else Args = $"\"{source}\" \"{target}\"";
            }
            else if (isWindows)
            {
                Command = "cmd.exe";
                if (symbolic) Args = $"/C mklink \"{target}\" \"{source}\"";
                else Args = $"/C mklink /H \"{target}\" \"{source}\"";
            }

            if (Command == String.Empty || Args == String.Empty) return -2;

            int retVal = linker(Command, Args);
            if (retVal != 0)
            {
                Console.WriteLine(
                    $"Could not create link with command '{Command} {Args}'.");
                Console.WriteLine(errMsg);
            }
            return retVal;

            int linker(string command, string arguments)
            {
                var Proc = new Process()
                {
                    StartInfo = new ProcessStartInfo(command, arguments)
                    {
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        UseShellExecute = false
                    }
                };

                Proc.ErrorDataReceived += (sender, args) =>
                {
                    if (!String.IsNullOrWhiteSpace(args.Data))
                    {
                        errMsg = args.Data;
                    }
                };

                if (!Proc.Start())
                {
                    Console.WriteLine("Could not start process.");
                    return -1;
                }
                Proc.BeginErrorReadLine();

                Proc.WaitForExit(2000);

                if (errMsg.ToLower().Contains("sufficient privilege"))
                    return 1;

                return 0;
            }
        }
    }
}
