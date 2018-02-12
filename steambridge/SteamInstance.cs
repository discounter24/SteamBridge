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

    public delegate void LoginCallback(object sender, LoginResult reason);

    public delegate void AppUpdated(object sender, bool error = false);
    public delegate void AppUpdateStateChanged(object sender, SteamAppUpdateState state);

    public delegate void ModDownloaded(object sender, string folder);


    public partial class SteamInstance
    {

        public event AppUpdated AppUpdated;
        public event SteamExited SteamExited;
        public event LoginCallback LoginCallback;
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

        public void reset(bool asAdmin = false)
        {
            close(SteamExitReason.NothingSpecial,1000).Wait();
            start(asAdmin);
        }



        private void start(bool asAdmin = false)
        {
            Steam = new Process();
            Steam.StartInfo.FileName = SteamExeFile.FullName;

            Steam.StartInfo.RedirectStandardError = true;
            Steam.StartInfo.RedirectStandardInput = true;
            Steam.StartInfo.RedirectStandardOutput = true;
            Steam.StartInfo.UseShellExecute = false;

            Steam.StartInfo.CreateNoWindow = true;

            Steam.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            if (asAdmin) Steam.StartInfo.Verb = "runas";


            Steam.Start();

            Steam.EnableRaisingEvents = true;


            Task.Run(async () =>
            {
                try
                {
                    while (!Steam.HasExited)
                    {
                        try
                        {
                            string line = await Steam.StandardOutput.ReadLineAsync();
                            Steam_DataReceived(Steam, line);
                        }
                        catch (Exception)
                        {
                            await Task.Delay(1000);
                            continue;
                        }
                    }
                }
                catch (Exception) {  }



            });

            //Steam.ErrorDataReceived += Steam_DataReceived;
            //Steam.OutputDataReceived += Steam_DataReceived;
            Steam.Exited += Steam_Exited;

            //Steam.BeginErrorReadLine();
            //Steam.BeginOutputReadLine();

            waitStartAsync.WaitOne();
        }

        private void Steam_Exited(object sender, EventArgs e)
        {
            SteamExited?.Invoke(this,SteamExitReason.NothingSpecial);
        }

        private void Steam_DataReceived(object sender, string e)
        {
            if (string.IsNullOrEmpty(e)) return;
            string line = e;

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
            else if (line.Equals("FAILED with result code 5") | line.Equals("Login with cached credentials FAILED with result code 5"))
            {
                LoginCallback?.Invoke(this, LoginResult.WrongInformation);
            }
            else if (line.Equals("FAILED with result code 88"))
            {
                LoginCallback?.Invoke(this, LoginResult.SteamGuardCodeWrong);
            }
            else if (line.Equals("FAILED with result code 65"))
            {
                LoginCallback?.Invoke(this, LoginResult.SteamGuardCodeWrong);
            }
            else if (line.Equals("FAILED with result code 71"))
            {
                LoginCallback?.Invoke(this, LoginResult.ExpiredCode);
            }
            else if (line.Equals("FAILED with result code 84"))
            {
                LoginCallback?.Invoke(this, LoginResult.RateLimitedExceeded);
            }
            else if (line.Contains("using 'set_steam_guard_code'"))
            {
                LoginCallback?.Invoke(this, LoginResult.SteamGuardNotSupported);
            }
            else if (line.Contains("Enter the current code from your Steam Guard Mobile Authenticator app"))
            {
                LoginCallback?.Invoke(this, LoginResult.WaitingForSteamGuard);
            }
            else if (line.Contains("FAILED with result code 50"))
            {
                LoginCallback?.Invoke(this, LoginResult.AlreadyLoggedIn);
            }
            else if (LoginState == false & (line.Contains("Waiting for license info...OK") | line.Contains("Logged in OK")))
            {
                LoginState = true;
                LoginCallback?.Invoke(this, LoginResult.OK);
            }
            else if (Regex.IsMatch(line, "ERROR! Download item [0-9]+ failed (Access Denied)."))
            {
                ModDownloaded?.Invoke(this, null);
            }
            else if (Regex.IsMatch(line, "Error! App '[0-9]+' state is 0x[0-9]+ after update job."))
            {
                AppUpdated?.Invoke(true);
            }
            else if (Regex.IsMatch(line, @"Update state \(0x5\) validating, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)"))
            {
                Regex pattern = new Regex(@"Update state \(0x5\) validating, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)");
                Match match = pattern.Match(line);


                SteamAppUpdateState state = new SteamAppUpdateState();
                state.percentage = Convert.ToInt32(match.Groups[1].Value);
                state.receivedBytes = Convert.ToInt64(match.Groups[3].Value);
                state.totalBytes = Convert.ToInt64(match.Groups[4].Value);
                state.stage = UpdateStateStage.Validating;

                AppUpdateStateChanged?.Invoke(this, state);
            }
            else if (Regex.IsMatch(line, @"Update state \(0x61\) downloading, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)"))
            {
                Regex pattern = new Regex(@"Update state \(0x61\) downloading, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)");
                Match match = pattern.Match(line);


                SteamAppUpdateState state = new SteamAppUpdateState();
                state.percentage = Convert.ToInt32(match.Groups[1].Value);
                state.receivedBytes = Convert.ToInt64(match.Groups[3].Value);
                state.totalBytes = Convert.ToInt64(match.Groups[4].Value);
                state.stage = UpdateStateStage.Downloading;

                AppUpdateStateChanged?.Invoke(this, state);
            }
            else if (Regex.IsMatch(line, @"Update state \(0x81\) commiting, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)"))
            {
                Regex pattern = new Regex(@"Update state \(0x81\) commiting, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)");
                Match match = pattern.Match(line);


                SteamAppUpdateState state = new SteamAppUpdateState();
                state.percentage = Convert.ToInt32(match.Groups[1].Value);
                state.receivedBytes = Convert.ToInt64(match.Groups[3].Value);
                state.totalBytes = Convert.ToInt64(match.Groups[4].Value);
                state.stage = UpdateStateStage.Commiting;

                AppUpdateStateChanged?.Invoke(this, state);
            }
            else if (Regex.IsMatch(line, @"Update state \(0x11\) preallocating, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)"))
            {
                Regex pattern = new Regex(@"Update state \(0x11\) preallocating, progress: ([0-9]+)\.([0-9]+) \(([0-9]+) / ([0-9]+)\)");
                Match match = pattern.Match(line);


                SteamAppUpdateState state = new SteamAppUpdateState();
                state.percentage = Convert.ToInt32(match.Groups[1].Value);
                state.receivedBytes = Convert.ToInt64(match.Groups[3].Value);
                state.totalBytes = Convert.ToInt64(match.Groups[4].Value);
                state.stage = UpdateStateStage.Preallocating;
                AppUpdateStateChanged?.Invoke(this, state);

            }
            else if (line.Contains("Success! App '") & line.Contains("' fully installed."))
            {
                AppUpdated?.Invoke(this);
            }
            else if (line.Contains("Success! App '") & line.Contains("' already up to date."))
            {
                AppUpdated?.Invoke(this);
            }
            else if (line.Contains("Success. Downloaded item") & line.Contains("bytes"))
            {
                ModDownloaded?.Invoke(this, line.Split('"')[1]);
            }

        }


        public bool tryGetSteamLogin()
        {
           DirectoryInfo localUnturned = Utils.getLocalUnturnedInstallation();
            if (localUnturned == null)
            {
                return false;
            }
            else
            {
                try
                {

                    List<string> files = new List<string>();
                    files.Add("\\config\\config.vdf");
                    files.Add("\\config\\loginusers.vdf");


                    foreach(string cfile in files)
                    {

                        FileInfo src = new FileInfo(localUnturned.FullName + cfile);
                        FileInfo dest = new FileInfo(SteamExeFile.Directory.FullName +  cfile);

                        if (!dest.Directory.Exists) dest.Directory.Create();
                        

                        if (dest.Exists)
                        {
                            dest.Delete();
                        }

                        File.WriteAllBytes(dest.FullName, File.ReadAllBytes(src.FullName));
                    }


                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }


        public Task close(SteamExitReason reason = SteamExitReason.NothingSpecial, int timeout = 15000)
        {
            var task = Task.Run(() =>
            {
                sendCommand("logout");
                sendCommand("exit");

                if (!Steam.WaitForExit(timeout))
                {
                    Steam.Kill();
                }
                SteamExited?.Invoke(this, reason);
            });

            return task;
        }

        public static void killAll()
        {
            foreach(Process p in Process.GetProcessesByName("steamcmd"))
            {
                try
                {
                    p.Kill();
                } catch (Exception) { }
            }
        }

    }



    public class SteamAppUpdateState
    {
        public UpdateStateStage stage;
        public int percentage = 0;
        public long receivedBytes = 0;
        public long totalBytes = 0;
    }



}
