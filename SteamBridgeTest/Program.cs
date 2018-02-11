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
                _.SteamOutput += (sender, e) => { Console.WriteLine(e); };
                _.tryGetSteamLogin();
                _.close().Wait();


            }

            SteamInstance.killAll();
            SteamInstance instance = new SteamInstance(new System.IO.FileInfo(installer.Folder.FullName + "\\steamcmd.exe"));
            instance.SteamOutput += Instance_SteamOutput;

            SteamGameInstaller gameInstaller = new SteamGameInstaller(instance);

            Console.WriteLine("Please enter your steam-username:");
            string username = Console.ReadLine();
            gameInstaller.Output += GameInstaller_Output;
            gameInstaller.login(username);
            gameInstaller.WaitingForPassword += GameInstaller_WaitingForPassword;
            gameInstaller.WaitingForTwoFactor += GameInstaller_WaitingForTwoFactor;


            while(true)
            {
                Task.Delay(100);
            }

            /*instance.LoginCallback += Instance_LoginCallback;


         
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
            */

        }

        private static void GameInstaller_WaitingForTwoFactor(object sender, TwoFactorRequiredArgs e)
        {
            Console.WriteLine("Steam Guard Code:");
            e.TwoFactorCode = Console.ReadLine();
        }

        private static void GameInstaller_WaitingForPassword(object sender, PasswordRequiredArgs e)
        {
            Console.WriteLine("Password:");
            e.password = Console.ReadLine();
        }

        private static void GameInstaller_Output(object sender, string e)
        {
            
        }

        private static void Instance_LoginCallback(object sender, LoginResult reason)
        {
           // if (reason == LoginResult.OK) ((SteamInstance)sender).updateApp(304930, "unturned");
        }


        private static void Instance_SteamOutput(object sender, string text)
        {
            Console.WriteLine("[steamcmd] " + text);
        }
    }
}
