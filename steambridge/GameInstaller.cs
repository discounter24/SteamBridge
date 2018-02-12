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

        public event AppUpdateStateChanged AppUpdateStateChanged;
        public event AppUpdated AppUpdated;

        public event EventHandler<PasswordRequiredArgs> WaitingForPassword;
        public event EventHandler<TwoFactorRequiredArgs> WaitingForTwoFactor;
        public event EventHandler<string> Output;

        private SteamInstance instance = null;

        public SteamGameInstaller(SteamInstance instance)
        {
            this.instance = instance;
        }



        public Task<LoginResult> login(string username, string password = "", string code = "")
        {
            return Task<LoginResult>.Run(() =>
            {
                if (instance.LoginState) return LoginResult.AlreadyLoggedIn;

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


        public void installGame(int id, string folder, bool validate = false)
        {
            if (instance.LoginState)
            {
                instance.requestAppLicense(id);

                instance.AppUpdateStateChanged += (sender, updateArgs) =>
                {
                    AppUpdateStateChanged?.Invoke(sender, updateArgs);
                };
                instance.AppUpdated += (sender, e) =>
                {
                    AppUpdated?.Invoke(sender, e);
                };

                instance.updateApp(id, folder, validate);
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
