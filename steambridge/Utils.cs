using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace steambridge
{
    public static class Utils
    {
        public static DirectoryInfo getLocalUnturnedInstallation()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam");
            try
            {
                string value = (string)key.GetValue("UninstallString", null);
                return new FileInfo(value).Directory;
            }
            catch (Exception)
            {
                return null;
            }
        }



    }
}
