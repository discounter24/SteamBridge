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

        public void loginUseCache(string username)
        {
            sendCommand(string.Format("login {0}"));
        }

        public LoginResult loginAnonymous(int timeout = 2000)
        {
            return login(null, null, null, timeout);
        }

        public void loginAsync(string username, string password)
        {
            sendCommand(string.Format("login {0} {1}",username,password));
        }

        public LoginResult login(string username = null, string password = "", string steamguard = "", int timeout = 20000)
        {
            var source = new CancellationTokenSource();
            source.CancelAfter(timeout);

            return Task<LoginResult>.Run(() =>
            {
                bool _loggedin = false;
                bool canceled = true;

                ManualResetEvent waitForResult = null;
                LoginResult result = LoginResult.Timeout;

                LoggedIn success = (sender) => { canceled = false; result = LoginResult.OK; _loggedin = true; waitForResult.Set(); };

                LoginFailed failed = (sender, r) => {
                    result = r;
                    waitForResult.Set();
                };

                LoggedIn += success;
                LoginFailed += failed;

              
                int i = 0;
                do
                {
                    canceled = true;
                    waitForResult = new ManualResetEvent(false);

                    sendCommand(username == null ? "login anonymous" : string.Format("login {0}{1}{2}", username, string.IsNullOrEmpty(" " + password) ? "" : " " + password, string.IsNullOrEmpty(" " + steamguard) ? "" : " " + steamguard));
                    waitForResult.WaitOne(timeout);
                    i++;
                }
                while (!_loggedin & !canceled & !source.Token.IsCancellationRequested);
                source.Dispose();

                LoggedIn -= success;
                LoginFailed -= failed;


                return result;
            }, source.Token).Result;
        }

    }



}
