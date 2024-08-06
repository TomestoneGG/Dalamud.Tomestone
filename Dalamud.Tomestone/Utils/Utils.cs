using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Dalamud.Tomestone
{
    internal static class Utils
    {
        /// <summary>
        /// Open a URL in the default browser.
        /// Supports Windows, Linux, and macOS.
        /// </summary>
        internal static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        internal static byte ReverseBits(byte b)
        {
            b = (byte)((b * 0x0202020202UL & 0x010884422010UL) % 1023);
            return b;
        }
    }
}
