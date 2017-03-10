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

                new SteamInstance(new System.IO.FileInfo(installer.Folder.FullName + "\\steamcmd.exe")).close().Wait();

            }

            SteamInstance.killAll();
            SteamInstance instance = new SteamInstance(new System.IO.FileInfo(installer.Folder.FullName + "\\steamcmd.exe"));
            instance.SteamOutput += Instance_SteamOutput;

            Console.WriteLine(instance.tryInstallOrUpdateLoginCredentialsFromSteamClient());
            Console.WriteLine("Login..");
            Console.WriteLine(instance.login("discounter24"));

            //Console.WriteLine(instance.login("deventuretech9", "$openPassword$").ToString());

            Console.WriteLine("Done!");
            Console.ReadKey();
            instance.close();

         
        }

        private static void Instance_SteamOutput(object sender, string text)
        {
            Console.WriteLine("steam: " + text);
        }
    }
}
