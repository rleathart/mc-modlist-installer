using System.Runtime.InteropServices;

namespace mcmli
{
  partial class Program
  {
    public static bool IsOS(OSPlatform osPlatform)
    {
      return RuntimeInformation.IsOSPlatform(osPlatform);
    }
  }
}
