using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace VManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static Settings settings;
        public MainWindow()
        {
            InitializeComponent();

            string settingsPath = ReflectionHelper.GetFileUnderApplicationFolder("settings.json");
            settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsPath));
            if (settings == null)
            {
                MessageBox.Show("Please setup settings file first, app exits.");
                Environment.Exit(-1);
            }
            
            CheckAutoStartStatus();
            StartV2rayInstance();
            
            DownloadGeoFiles(settings.UrlOfGeoIp, GeoIpDownloadBar, GeoIpLabel, GeoIpDownloadButton);
            DownloadGeoFiles(settings.UrlOfGeoSite, GeoSiteDownloadBar, GeoSiteLabel, GeoSiteDownloadButton);
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

        private void DownloadGeoFiles(string url, ProgressBar progressBar, Label label, Button button)
        {
            label.Visibility = Visibility.Hidden;
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0;
            button.IsEnabled = false;

            var fileName = Path.GetFileName(url);
            
            DateTime lastWriteTime = File.GetLastWriteTime(fileName);
            if (lastWriteTime.Day == DateTime.Today.Day)
            {
                label.Visibility = Visibility.Visible;
                progressBar.Visibility = Visibility.Hidden;
                button.IsEnabled = true;
                label.Content = lastWriteTime.ToString("yyyy-MM-dd");
                
                return;
            }
            
            using (WebClient client = new WebClient())
            {
                client.Proxy = new WebProxy("192.168.8.4", 1082);
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

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            V2RayHelper.ClearInstances();
        }
    }
}