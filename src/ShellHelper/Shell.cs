using System.Diagnostics;
using System.Runtime.InteropServices;

namespace mcmli
{
    public partial class ShellHelper
    {
        public string StdOut;
        public string StdErr;
    }
    public static partial class ShellHelperExtension
    {
        public static ShellHelper Shell(this string Args, string Command = "")
        {

            ShellHelper shell = new ShellHelper();

            if (Command == "")
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Command = "cmd.exe";
                    Args = $"/C {Args}";
                }
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                        || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Command = "/bin/bash";
                    Args = Args.Replace("\"","\\\"");
                    Args = $"-c \"{Args}\"";
                }
            }

            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Command,
                    Arguments = Args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            shell.StdOut = process.StandardOutput.ReadToEnd().Trim();
            shell.StdErr = process.StandardError.ReadToEnd().Trim();
            process.WaitForExit();

            return shell;

        }
    }
}
