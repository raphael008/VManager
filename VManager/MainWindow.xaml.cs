using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;
using ProgressBar = System.Windows.Controls.ProgressBar;

namespace VManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static Settings settings;
        private static NotifyIcon icon;

        public MainWindow()
        {
            InitializeComponent();

            ReflectionHelper.SetWorkingDirectoy();

            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("settings.json"));
            if (settings == null)
            {
                MessageBox.Show("Please setup settings file first, app exits.");
                Environment.Exit(-1);
            }

            InitializeTrayIcon();

            CheckAutoStartStatus();

            StartV2rayInstance();

            DownloadGeoFiles(settings.UrlOfGeoIp, GeoIpDownloadBar, GeoIpLabel, GeoIpDownloadButton, false);
            DownloadGeoFiles(settings.UrlOfGeoSite, GeoSiteDownloadBar, GeoSiteLabel, GeoSiteDownloadButton, false);
            DownloadV2ray(settings.UrlOfV2ray, V2rayDownloadBar, V2rayLabel, V2rayDownloadButton, false);
        }

        private void InitializeTrayIcon()
        {
            MenuItem[] menuItems =
            {
                new MenuItem("AutoStart"),
                new MenuItem("Exit", (sender, args) => Application.Current.Shutdown())
            };
            ContextMenu menu = new ContextMenu(menuItems);

            icon = new NotifyIcon();
            icon.Icon = new Icon("icon.ico");
            icon.ContextMenu = menu;
            icon.Visible = true;
            icon.DoubleClick += (sender, args) =>
            {
                Show();
                Activate();
            }; 
        }

        private void StartV2rayInstance()
        {
            Process instance = V2RayHelper.GetInstance(settings.V2rayPath);

            instance.OutputDataReceived += (sender, args) =>
            {
                if (OutputTextBox.Dispatcher.CheckAccess())
                {
                    OutputTextBox.Text += $"{args.Data} \r\n";
                }
                else
                {
                    OutputTextBox.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        OutputTextBox.Text += $"{args.Data} \r\n";
                    }));
                }
            };
            instance.Start();
            instance.BeginOutputReadLine();
        }

        private void DownloadGeoFiles(string url, ProgressBar progressBar, Label label, Button button, bool isForced)
        {
            label.Visibility = Visibility.Hidden;
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0;
            button.IsEnabled = false;

            var fileName = Path.GetFileName(url);
            if (File.Exists(fileName) && !isForced)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(fileName);
                if (lastWriteTime.Day == DateTime.Today.Day)
                {
                    label.Visibility = Visibility.Visible;
                    progressBar.Visibility = Visibility.Hidden;
                    button.IsEnabled = true;
                    label.Content = lastWriteTime.ToString("yyyy-MM-dd");

                    return;
                }
            }

            using (WebClient client = new WebClient())
            {
                client.Proxy = new WebProxy(settings.ProxyHost, settings.ProxyPort);
                client.DownloadProgressChanged += (sender, args) => { progressBar.Value = args.ProgressPercentage; };
                client.DownloadFileCompleted += (sender, args) =>
                {
                    label.Visibility = Visibility.Visible;
                    progressBar.Visibility = Visibility.Hidden;
                    button.IsEnabled = true;

                    DateTime creationTime = File.GetLastWriteTime(fileName);
                    label.Content = creationTime.ToString("yyyy-MM-dd");
                };

                try
                {
                    client.DownloadFileAsync(new Uri(url), fileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    label.Content = "download failed";

                    label.Visibility = Visibility.Visible;
                    progressBar.Visibility = Visibility.Hidden;
                    button.IsEnabled = true;
                }
            }
        }
        
        private void DownloadV2ray(string url, ProgressBar progressBar, Label label, Button button, bool isForced)
        {
            label.Visibility = Visibility.Hidden;
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0;
            button.IsEnabled = false;

            var fileName = Path.GetFileName(url);
            if (File.Exists(fileName) && !isForced)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(fileName);
                if (lastWriteTime.Day == DateTime.Today.Day)
                {
                    label.Visibility = Visibility.Visible;
                    progressBar.Visibility = Visibility.Hidden;
                    button.IsEnabled = true;
                    label.Content = lastWriteTime.ToString("yyyy-MM-dd");

                    return;
                }
            }

            using (WebClient client = new WebClient())
            {
                client.Proxy = new WebProxy(settings.ProxyHost, settings.ProxyPort);
                client.DownloadProgressChanged += (sender, args) => { progressBar.Value = args.ProgressPercentage; };
                client.DownloadFileCompleted += (sender, args) =>
                {
                    label.Visibility = Visibility.Visible;
                    progressBar.Visibility = Visibility.Hidden;
                    button.IsEnabled = true;

                    DateTime creationTime = File.GetLastWriteTime(fileName);
                    label.Content = creationTime.ToString("yyyy-MM-dd");
                };
                client.DownloadFileCompleted += V2rayDownloadCompleted;

                try
                {
                    client.DownloadFileAsync(new Uri(url), fileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    label.Content = "download failed";

                    label.Visibility = Visibility.Visible;
                    progressBar.Visibility = Visibility.Hidden;
                    button.IsEnabled = true;
                }
            }
        }

        private void V2rayDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            V2RayHelper.ClearInstances();
            
            string fileName = "v2ray-windows-64.zip";
            var randomDirectory = Path.GetRandomFileName();
            while (Directory.Exists(randomDirectory))
            {
                randomDirectory = Path.GetRandomFileName();
            }
            
            Directory.CreateDirectory(randomDirectory);
            
            ZipFile.ExtractToDirectory(fileName, randomDirectory);

            string sourceBinary = Path.Combine(randomDirectory, "v2ray.exe");
            string targetBinary = "v2ray.exe";
            
            if (File.Exists(targetBinary))
            {
                File.Replace(sourceBinary, targetBinary, null);
            }
            else
            {
                File.Move(sourceBinary, targetBinary);
            }
            
            Directory.Delete(randomDirectory, true);
            
            StartV2rayInstance();
        }
        
        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            icon.Dispose();
            V2RayHelper.ClearInstances();
        }
    }
}