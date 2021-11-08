using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Newtonsoft.Json;
using VManager.Constants;
using Application = System.Windows.Application;
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

        private ManualResetEvent signal = new ManualResetEvent(false);

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

            V2RayHelper.StartInstance(settings.V2rayPath, OutputTextBox);

            Task.Run(TestNetwork);
            Task.Run(DownloadUpdate);
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
            icon.Icon = System.Drawing.Icon.ExtractAssociatedIcon("icon.ico");
            icon.Icon = new Icon("icon.ico");
            icon.ContextMenu = menu;
            icon.Visible = true;
            icon.DoubleClick += (sender, args) =>
            {
                Show();
                Activate();
            };
        }

        private void DownloadFile(
            string url,
            string fileName,
            DownloadProgressChangedEventHandler downloadProgressChanged,
            AsyncCompletedEventHandler downloadFileCompleted)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Proxy = new WebProxy("192.168.8.3", 1082);
                webClient.DownloadProgressChanged += downloadProgressChanged;
                webClient.DownloadFileCompleted += downloadFileCompleted;
                webClient.DownloadFileAsync(new Uri(url), fileName);
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            icon.Dispose();
            V2RayHelper.ClearInstances();
        }

        private void TestNetwork()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.Proxy = new WebProxy(settings.ProxyHost, settings.ProxyPort);

            var httpClient = new HttpClient(httpClientHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            int testCount = 0;
            while (true)
            {
                if (testCount >= 5)
                {
                    throw new Exception("Network is unavailable.");
                }

                Task<HttpResponseMessage> result = httpClient.GetAsync("https://www.google.com/");
                result.Wait();

                if (result.Result.IsSuccessStatusCode)
                {
                    break;
                }

                testCount++;
                Thread.Sleep(3000);
            }

            signal.Set();
        }

        private void SetLabelContent(Label label, ProgressBar progressBar, string content)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                label.Content = content;
                label.Visibility = Visibility.Visible;
                progressBar.Visibility = Visibility.Collapsed;
            }));
        }

        private void DownloadUpdate()
        {
            try
            {
                TestNetwork();
            }
            catch (Exception e)
            {
                SetLabelContent(GeoIpLabel, GeoIpDownloadBar, "Network unavailable");
                SetLabelContent(GeoSiteLabel, GeoSiteDownloadBar, "Network unavailable");
                SetLabelContent(V2rayLabel, V2rayDownloadBar, "Network unavailable");
            }
            
            signal.WaitOne();

            if (FileHelper.IsToday(FileConstants.GeoIp))
            {
                SetLabelContent(GeoIpLabel, GeoIpDownloadBar, DateTime.Today.ToString("yyyy-MM-dd"));
            }
            else
            {
                try
                {
                    DownloadFile(settings.UrlOfGeoIp, FileConstants.GeoIpTemp, GeoIpProgressChanged, GeoIpDownloadCompleted);
                }
                catch (Exception e)
                {
                    ControlHelper.EnableButton(GeoIpDownloadButton);
                    SetLabelContent(GeoIpLabel, GeoIpDownloadBar, "Error occurs when downloading GeoIP");
                }
            }

            if (FileHelper.IsToday(FileConstants.GeoSite))
            {
                SetLabelContent(GeoSiteLabel, GeoSiteDownloadBar, DateTime.Today.ToString("yyyy-MM-dd"));
            }
            else
            {
                try
                {
                    DownloadFile(settings.UrlOfGeoSite, FileConstants.GeoSiteTemp, GeoSiteProgressChanged, GeoSiteDownloadCompleted);
                }
                catch (Exception e)
                {
                    ControlHelper.EnableButton(GeoSiteDownloadButton);
                    SetLabelContent(GeoSiteLabel, GeoSiteDownloadBar, "Error occurs when downloading GeoSite");
                }
            }

            if (FileHelper.IsToday(FileConstants.V2RayZip))
            {
                SetLabelContent(V2rayLabel, V2rayDownloadBar, DateTime.Today.ToString("yyyy-MM-dd"));
            }
            else
            {
                try
                {
                    DownloadFile(settings.UrlOfV2ray, FileConstants.V2RayZip, V2RayProgressChanged, V2RayDownloadCompleted);
                }
                catch (Exception e)
                {
                    ControlHelper.EnableButton(V2rayDownloadButton);
                    SetLabelContent(V2rayLabel, V2rayDownloadBar, "Error occurs when downloading V2Ray");
                }
            }
        }
    }
}