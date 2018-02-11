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
                Console.WriteLine("Preparing steamcmd..");
                installer.installSteam();
                SteamInstance _ = new SteamInstance(new System.IO.FileInfo(installer.Folder.FullName + "\\steamcmd.exe"));
                _.tryGetSteamLogin();
                _.close().Wait();


            }

            SteamInstance.killAll();
            SteamInstance instance = new SteamInstance(new System.IO.FileInfo(installer.Folder.FullName + "\\steamcmd.exe"));
            instance.SteamOutput += Instance_SteamOutput;
            instance.LoginCallback += Instance_LoginCallback;


         
            Console.WriteLine("Please enter your steam-username:");
            string username = Console.ReadLine();

            if (instance.login(username,"") != LoginResult.OK)
            {
                instance.close().Wait();
                instance.SteamOutput -= Instance_SteamOutput;
                instance = new SteamInstance(new System.IO.FileInfo(installer.Folder.FullName + "\\steamcmd.exe"));
                instance.SteamOutput += Instance_SteamOutput;

                Console.WriteLine("Please enter your password:");
                string password = Console.ReadLine();

                LoginResult r = instance.login(username, password);


                if (r == LoginResult.WaitingForSteamGuard)
                {
                    Console.WriteLine("Please enter your steam guard code:");
                    string code = Console.ReadLine();
                    Console.WriteLine(instance.login(username, password, code));
                }
                else
                {
                    Console.WriteLine("Result: " + r.ToString());
                }
            }



          
            Console.WriteLine("Press return to exit..");
            Console.ReadLine();

            instance.SteamOutput -= Instance_SteamOutput;

            instance.close();


        }

        private static void Instance_LoginCallback(object sender, LoginResult reason)
        {
            if (reason == LoginResult.OK) ((SteamInstance)sender).updateApp(304930, "unturned");
        }


        private static void Instance_SteamOutput(object sender, string text)
        {
            Console.WriteLine("[steamcmd] " + text);
        }
    }
}
