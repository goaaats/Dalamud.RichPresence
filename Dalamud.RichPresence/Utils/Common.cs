using System;

namespace RichPresencePlugin.Utils
{
    internal static class CommonUtil
    {
        internal static bool IsOnLinuxOrWine()
        {
            var wineOnLinux = Environment.GetEnvironmentVariable("XL_WINEONLINUX");
            var wineOnMac = Environment.GetEnvironmentVariable("XL_WINEONMAC");
            return wineOnLinux?.ToLower() == "true" || wineOnLinux?.ToLower() == "1" || wineOnMac?.ToLower() == "true" || wineOnMac?.ToLower() == "1";
        }
    }
}