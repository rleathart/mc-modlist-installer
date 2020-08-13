using System;
using System.IO;
using System.Net;
using System.Threading;

namespace mcmli
{
    partial class Program
    {
        public static void DownloadFile(string url, string dir = ".")
        {
            /*
             * Downloads a URL with an automatically determined filename
             * to a given directory ('.' by default).
             */
            var uri = new Uri(url);
            string filename = Path.GetFileName(uri.LocalPath);

            WebClient client = new WebClient();

            client.DownloadFileAsync(uri, Path.Combine(dir, filename));
            while (client.IsBusy) { Thread.Sleep(100); }
        }
    }
}
