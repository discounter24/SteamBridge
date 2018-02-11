using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace steambridge
{

    

    public class SteamGameInstaller
    {
        public event EventHandler<PasswordRequiredArgs> WaitingForPassword;
        public event EventHandler<TwoFactorRequiredArgs> WaitingForTwoFactor;
        public event EventHandler<string> Output;

        private SteamInstance instance = null;

        public SteamGameInstaller(SteamInstance instance)
        {
            this.instance = instance;
            instance.AppUpdateStateChanged += Instance_AppUpdateStateChanged;
            instance.AppUpdated += Instance_AppUpdated;
        }

        private void Instance_AppUpdated(object sender, bool error = false)
        {
            throw new NotImplementedException();
        }

        private void Instance_AppUpdateStateChanged(object sender, SteamAppUpdateState state)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResult> login(string username, string password = "", string code = "")
        {
            return Task<LoginResult>.Run(() =>
            {
                bool cancel = false;
                LoginResult result = instance.login(username,password,code);
                if (result == LoginResult.WaitingForSteamGuard | result == LoginResult.SteamGuardCodeWrong)
                {
                    ManualResetEvent wait = new ManualResetEvent(false);
                    WaitingForTwoFactor += (sender, e) =>
                    {
                        cancel = e.cancel;
                        code = e.TwoFactorCode;
                        wait.Set();
                    };
                    WaitingForTwoFactor?.Invoke(this, new TwoFactorRequiredArgs());
                    wait.WaitOne();
                    if (!cancel)
                    {
                        //instance.reset();
                        return login(username, password, code).Result;
                    }
                    else
                    {
                        return LoginResult.CanceledByUser;
                    }

                }
                else if (result == LoginResult.Timeout | result == LoginResult.WrongInformation)
                {
                    ManualResetEvent wait = new ManualResetEvent(false);
                    WaitingForPassword += (sender, e) =>
                    {
                        cancel = e.cancel;
                        password = e.password;
                        wait.Set();
                    };
                    WaitingForPassword?.Invoke(this, new PasswordRequiredArgs());
                    wait.WaitOne();
                    if (!cancel)
                    {
                        //instance.reset();
                        return login(username, password).Result;
                    }
                    else
                    {
                        return LoginResult.CanceledByUser;
                    }
                }


                return result;

            });
        }


        public void installGame(int id)
        {
            if (instance.LoginState)
            {

            }
            else
            {
                throw new LoginRequiredException();
            }
        }
        
    }

    public class LoginRequiredException : Exception
    {
        public LoginRequiredException() : base("Please login before starting to install or update a game."){ }
    }

    public class PasswordRequiredArgs
    {
        public bool cancel = false;
        public string password = "";
    }

    public class TwoFactorRequiredArgs
    {
        public bool cancel = false;
        public string TwoFactorCode = "";
    }



}
