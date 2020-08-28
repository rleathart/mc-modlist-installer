using System.IO;
using System.Linq;

namespace mcmli
{
    partial class Program
    {
        public static void GetModlists()
        {
            modlists = Directory.EnumerateFiles(exeDir,"*.modlist").OrderBy(f => f).ToList();
        }
    }
}
