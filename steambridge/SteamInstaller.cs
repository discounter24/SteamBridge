using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;

namespace steambridge
{

    public delegate void SteamInstalled(object sender, FileInfo SteamCMDExe);
    public delegate void SteamInstallationError(object sender, Exception ex);

    

    public class SteamInstaller
    {
        public SteamInstalled SteamInstalled;
        public SteamInstallationError SteamInstallationError;

        private readonly SteamBridgeWebClient Client;

        ManualResetEvent waitSync;

        public DirectoryInfo Folder
        {
            get;
            set;
        }

        private FileInfo SteamCMDExe
        {
            get
            {
                return new FileInfo(Folder.FullName + "\\steamcmd.exe");
            }
        }

        private FileInfo SteamCMDZip
        {
            get
            {
                return new FileInfo(Folder.FullName + "\\steamcmd.zip");
            }
        }

        public Uri DownloadLink
        {
            get
            {
                return new Uri("https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip");
            }
        }

        public bool Installed
        {
            get
            {
                return SteamCMDExe.Exists;
            }
        }



        public SteamInstaller(string folder)
        {
            try
            {
                Folder = new DirectoryInfo(folder);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            Client = new SteamBridgeWebClient();
            Client.DownloadFileCompleted += (sender, e) => { unzipPackage(); };
        }

        private void prepareForInstallation()
        {
            if (!Folder.Exists)
            {
                Folder.Create();
            }

            clearCacheFile();

            if (SteamCMDExe.Exists)
            {
                SteamCMDExe.Delete();
            }
        }

        private void clearCacheFile()
        {
            if (SteamCMDZip.Exists)
            {
                SteamCMDZip.Delete();
            }
        }

        public void installSteamAsync()
        {
            _installSteamTask();
        }

        public void installSteam()
        {
            waitSync = new ManualResetEvent(false);
            _installSteamTask();

            waitSync.WaitOne();
        }

        private Task _installSteamTask()
        {
            return Task.Run(() =>
            {
                try
                {
                    prepareForInstallation();
                    downloadPackage();
                }
                catch (Exception ex)
                {
                    SteamInstallationError?.Invoke(this, ex);
                }
            });
        }


        private void downloadPackage()
        {
            Client.DownloadFileAsync(DownloadLink, SteamCMDZip.FullName);
        }

        private void unzipPackage()
        {
            FastZip archive = new FastZip();
            archive.CreateEmptyDirectories = true;
            archive.ExtractZip(SteamCMDZip.FullName, Folder.FullName, "");
            clearCacheFile();

            waitSync.Set();
            SteamInstalled?.Invoke(this, SteamCMDExe);
        }
    }



}
