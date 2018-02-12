using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using steambridge;
using System.Diagnostics;

namespace SteamBridgeTest
{
    class Program
    {
        static Stopwatch watch = new Stopwatch();

        static void Main(string[] args)
        {

            Console.WriteLine("This tool will create a new steam and a new unturned installation on your computer at C:\\SteamTest\\ .");
            Console.WriteLine("This will take about 4 gigabytes of your disk space.");
            Console.WriteLine("We will start a stopwatch once you have logged into your steam account. Please tell us the time it shows when the installation completed.");
            Console.WriteLine("Press any key to continue..");

            Console.ReadKey();
            watch.Start();
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

            Console.WriteLine("Connecting to steam..");
            SteamInstance.killAll();
            SteamInstance instance = new SteamInstance(new System.IO.FileInfo(installer.Folder.FullName + "\\steamcmd.exe"));
            instance.SteamOutput += Instance_SteamOutput;

            SteamGameInstaller gameInstaller = new SteamGameInstaller(instance);

            Console.WriteLine("Please enter your steam-username:");
            string username = Console.ReadLine();
            gameInstaller.Output += GameInstaller_Output;
            
            gameInstaller.WaitingForPassword += GameInstaller_WaitingForPassword;
            gameInstaller.WaitingForTwoFactor += GameInstaller_WaitingForTwoFactor;

            Console.WriteLine("Login..");
            if (gameInstaller.login(username).Result == LoginResult.OK)
            {
                gameInstaller.AppUpdateStateChanged += GameInstaller_AppUpdateStateChanged;
                gameInstaller.AppUpdated += GameInstaller_AppUpdated;
                gameInstaller.installGame(304930,"unturned",true);

                instance.sendCommand("quit");
            }


            string command = Console.ReadLine();
            
            while (!command.Equals("exit"))
            {
                instance.sendCommand(command);
                Console.ReadLine();
            }
            

        }

        private static void GameInstaller_AppUpdated(object sender, bool error = false)
        {
            watch.Stop();
            Console.WriteLine("Game ready!");
            Console.WriteLine("It took " + watch.Elapsed.ToString() + " to get everything ready.");
        }

        private static void GameInstaller_AppUpdateStateChanged(object sender, SteamAppUpdateState state)
        {
            switch (state.stage)
            {
                case UpdateStateStage.Validating:
                    Console.WriteLine(string.Format("Validating game files ({0}%)..", state.percentage));
                    break;
                case UpdateStateStage.Downloading:
                    Console.WriteLine(string.Format("Downloading game files ({0}%)..", state.percentage));
                    break;
                case UpdateStateStage.Commiting:
                    Console.WriteLine(string.Format("Commiting game files ({0}%)..", state.percentage));
                    break;
                case UpdateStateStage.Preallocating:
                    Console.WriteLine(string.Format("Preparing game files ({0}%)..", state.percentage));
                    break;
                default:
                    break;
            }

            
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
            //Console.WriteLine("[steamcmd] " + text);
        }
    }
}
