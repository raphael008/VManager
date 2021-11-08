using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows.Controls;
using VManager.Constants;

namespace VManager
{
    public partial class MainWindow
    {
        private void GeoIpProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            SafeChangeProgressBarValue(GeoIpDownloadBar, e.ProgressPercentage);
        }
        
        private void GeoSiteProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            SafeChangeProgressBarValue(GeoSiteDownloadBar, e.ProgressPercentage);
        }
        
        private void V2RayProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            SafeChangeProgressBarValue(V2rayDownloadBar, e.ProgressPercentage);
        }

        private void GeoIpDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ControlHelper.EnableButton(GeoIpDownloadButton);
            
            if (!File.Exists(FileConstants.GeoIpTemp))
                return;
            
            if (!File.Exists(FileConstants.GeoIp))
                File.Move(FileConstants.GeoIpTemp, FileConstants.GeoIp);
            else
                File.Replace(FileConstants.GeoIpTemp, FileConstants.GeoIp, FileConstants.GeoIpBackup);
            
            SetLabelContent(GeoIpLabel, GeoIpDownloadBar, DateTime.Today.ToString("yyyy-MM-dd"));
        }
        
        private void GeoSiteDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ControlHelper.EnableButton(GeoSiteDownloadButton);
            
            if (!File.Exists(FileConstants.GeoSiteTemp))
                return;
            
            if (!File.Exists(FileConstants.GeoSite))
                File.Move(FileConstants.GeoSiteTemp, FileConstants.GeoSite);
            else
                File.Replace(FileConstants.GeoSiteTemp, FileConstants.GeoSite, FileConstants.GeoSiteBackup);
            
            SetLabelContent(GeoSiteLabel, GeoSiteDownloadBar, DateTime.Today.ToString("yyyy-MM-dd"));
        }
        
        private void V2RayDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ControlHelper.EnableButton(V2rayDownloadButton);
            
            V2RayHelper.ClearInstances();
            
            var randomDirectory = Path.GetRandomFileName();
            while (Directory.Exists(randomDirectory))
            {
                randomDirectory = Path.GetRandomFileName();
            }

            Directory.CreateDirectory(randomDirectory);

            ZipFile.ExtractToDirectory(FileConstants.V2RayZip, randomDirectory);

            string sourceBinary = Path.Combine(randomDirectory, FileConstants.V2Ray);
            string targetBinary = FileConstants.V2Ray;

            if (File.Exists(targetBinary))
                File.Replace(sourceBinary, targetBinary, null);
            else
                File.Move(sourceBinary, targetBinary);
            Directory.Delete(randomDirectory, true);
            
            V2RayHelper.StartInstance(settings.V2rayPath, OutputTextBox);
            
            SetLabelContent(V2rayLabel, V2rayDownloadBar, DateTime.Today.ToString("yyyy-MM-dd"));
        }

        private void SafeChangeProgressBarValue(ProgressBar progressBar, int value)
        {
            if (progressBar.Dispatcher.CheckAccess())
            {
                progressBar.Value = value;
            }
            else
            {
                progressBar.Dispatcher.BeginInvoke(new Action(() =>
                {
                    progressBar.Value = value;
                }));
            }
        }
    }
}