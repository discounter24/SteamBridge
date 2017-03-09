using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace steambridge
{


    public partial class SteamInstance
    {


        public void sendCommand(string command)
        {
            if (Steam != null)
            {

                Steam.StandardInput.WriteLine(command);
                Steam.StandardInput.Flush();
                Steam.StandardInput.WriteLine(Steam.StandardInput.NewLine);
                Steam.StandardInput.Flush();
            }
        }


        public void updateApp(int appid, string installdir, bool validate = false)
        {
            sendCommand(String.Format("force_install_dir \"{0}\"", installdir));
            sendCommand(String.Format("app_update {0} {1}", appid.ToString(), validate ? "validate" : ""));
        }

        public void requestAppLicense(int appid)
        {
            sendCommand(String.Format("app_license_request {0}", appid.ToString()));
        }

        public void getWorkshopMod(string appid, string modid)
        {
            sendCommand(String.Format("workshop_download_item {0} {1}", appid, modid));
        }

        public void getWorkshopStatus(int appid)
        {
            sendCommand("workshop_status " + appid);
        }

        public void loginAnonymousAsync()
        {
            sendCommand("login anonymous");
        }

        public bool loginAnonymous()
        {
            bool _loggedin = false;
            bool canceled = true;

            ManualResetEvent waitForResult = null;

            LoggedIn success = (sender) => 
            {
                canceled = false;
                LoginState = true;
                waitForResult.Set();
            };
            LoginFailed failed = (sender, reason) => 
            {
                canceled = false;
                waitForResult.Set();
            };


            LoggedIn += success;
            LoginFailed += failed;
            
            do
            {
                canceled = true;
                waitForResult = new ManualResetEvent(false);

                loginAnonymous();
                waitForResult.WaitOne();
            }
            while (!_loggedin & !canceled);

            LoggedIn -= success;
            LoginFailed -= failed;

            return LoginState;
        }





    }

}
