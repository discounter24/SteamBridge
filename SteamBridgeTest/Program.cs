using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using steambridge;

namespace SteamBridgeTest
{
    class Program
    {

        static void Main(string[] args)
        {
            SteamInstaller installer = new SteamInstaller("C:\\SteamTest\\");
            if (!installer.Installed)
            {
                installer.installSteam();
            }

            SteamInstance instance = new SteamInstance(new System.IO.FileInfo(installer.Folder.FullName + "\\steamcmd.exe"));
            instance.SteamOutput += Instance_SteamOutput;

            Console.WriteLine("Started!");
            instance.close();

            Console.ReadKey();
        }

        private static void Instance_SteamOutput(object sender, string text)
        {
            Console.WriteLine("steam: " + text);
        }
    }
}
