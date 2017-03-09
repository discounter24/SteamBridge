using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace steambridge
{
    public delegate void SteamOutput(object sender, string text);

    public delegate void SteamStarted(object sender);
    public delegate void SteamExited(object sender, SteamExitReason reason);

    public delegate void LoggedIn(object sender);

    public delegate void LoginFailed(object sender, LoginFailReason reason);

    public delegate void AppUpdated(object sender, bool error = false);
    public delegate void AppUpdateStateChanged(object sender, SteamAppUpdateState state);

    public delegate void ModDownloaded(object sender, string folder);


    public partial class SteamInstance
    {

        public event AppUpdated AppUpdated;
        public event SteamExited SteamExited;
        public event LoggedIn LoggedIn;
        public event LoginFailed LoginFailed;
        public event SteamOutput SteamOutput;
        public event ModDownloaded ModDownloaded;
        public event AppUpdateStateChanged AppUpdateStateChanged;

        private FileInfo SteamExeFile;
        private Process Steam;

        private ManualResetEvent waitStartAsync = new ManualResetEvent(false);


        public bool LoginState
        {
            get;
            private set;
        }


        public SteamInstance(FileInfo pSteamExeFile)
        {
            this.SteamExeFile = pSteamExeFile;
            start();
        }



        private void start()
        {
            Steam = new Process();
            Steam.StartInfo.FileName = SteamExeFile.FullName;

            Steam.StartInfo.RedirectStandardError = true;
            Steam.StartInfo.RedirectStandardInput = true;
            Steam.StartInfo.RedirectStandardOutput = true;
            Steam.StartInfo.UseShellExecute = false;

            Steam.StartInfo.CreateNoWindow = true;

            Steam.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;


            Steam.StartInfo.Verb = "runas";

            Steam.Start();

            Steam.ErrorDataReceived += Steam_DataReceived;
            Steam.OutputDataReceived += Steam_DataReceived;
            Steam.Exited += Steam_Exited;

            Steam.BeginErrorReadLine();
            Steam.BeginOutputReadLine();

            waitStartAsync.WaitOne();
        }

        private void Steam_Exited(object sender, EventArgs e)
        {
            SteamExited?.Invoke(this,SteamExitReason.NothingSpecial);
        }

        private void Steam_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;
            string line = e.Data;

            SteamOutput?.Invoke(this, line);

            if (line.Equals("Loading Steam API...OK."))
            {
                waitStartAsync.Set();
                SteamExited?.Invoke(this, SteamExitReason.NonEnglishCharachers);
            }
            else if (line.Contains("cannot run from a folder path that includes non-English characters"))
            {
                close(SteamExitReason.NonEnglishCharachers);
            }
            else if (line.Equals("FAILED with result code 5"))
            {
                LoginFailed?.Invoke(this, LoginFailReason.WrongInformation);
            }
            else if (line.Equals("FAILED with result code 88"))
            {
                LoginFailed?.Invoke(this, LoginFailReason.TwoFactorWrong);
            }
            else if (line.Equals("FAILED with result code 65"))
            {
                LoginFailed?.Invoke(this, LoginFailReason.SteamGuardCodeWrong);
            }
            else if (line.Equals("FAILED with result code 71"))
            {
                LoginFailed?.Invoke(this, LoginFailReason.ExpiredCode);
            }
            else if (line.Contains("using 'set_steam_guard_code'") | line.Contains("Enter the current code from your Steam Guard Mobile Authenticator app"))
            {
                LoginFailed?.Invoke(this, LoginFailReason.SteamGuardNotSupported);

            }
            else if (line.Contains("FAILED with result code 50"))
            {
                LoginFailed?.Invoke(this, LoginFailReason.AlreadyLoggedIn);
            }
            else if (line.Contains("Waiting for license info...OK"))
            {
                LoginState = true;
                LoggedIn(this);
            }
            else if (Regex.IsMatch(line, "ERROR! Download item [0-9]+ failed (Access Denied)."))
            {
                ModDownloaded?.Invoke(this,null);
            }
            else if (Regex.IsMatch(line, "Error! App '[0-9]+' state is 0x[0-9]+ after update job."))
            {
                AppUpdated?.Invoke(true);
            }
            else if (Regex.IsMatch(line, @"Update state \(0x61\) downloading, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)"))
            {
                Regex pattern = new Regex(@"Update state \(0x61\) downloading, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)");
                Match match = pattern.Match(line);


                SteamAppUpdateState state = new SteamAppUpdateState();
                state.percentage = Convert.ToInt32(match.Groups[1].Value);
                state.receivedBytes = Convert.ToInt64(match.Groups[3].Value);
                state.totalBytes = Convert.ToInt64(match.Groups[4].Value);

                AppUpdateStateChanged?.Invoke(this, state);
            }
            else if (line.Contains("Success! App '") & line.Contains("' fully installed."))
            {
                AppUpdated?.Invoke(this, true);
            }
            else if (line.Contains("Success! App '") & line.Contains("' already up to date."))
            {
                AppUpdated?.Invoke(this, false);
            }
            else if (line.Contains("Success. Downloaded item") & line.Contains("bytes"))
            {
                ModDownloaded?.Invoke(this, line.Split('"')[1]);
            }

        }






        public Task close(SteamExitReason reason = SteamExitReason.NothingSpecial)
        {
            var task = Task.Run(() =>
            {
                sendCommand("logout");
                sendCommand("exit");

                if (!Steam.WaitForExit(15000))
                {
                    Steam.Kill();
                }
                SteamExited?.Invoke(this, reason);
            });

            return task;
        }



    }



    public class SteamAppUpdateState
    {
        public int percentage = 0;
        public long receivedBytes = 0;
        public long totalBytes = 0;
    }

}
