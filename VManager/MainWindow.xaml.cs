using System;
using System.ComponentModel;
using System.Diagnostics;
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
            StartV2rayInstance();

            Task.Run(TestNetwork);
            
            Task.Run(Download);
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

        private void StartV2rayInstance()
        {
            Process instance = V2RayHelper.GetInstance(settings.V2rayPath);

            instance.OutputDataReceived += (sender, args) =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    OutputTextBox.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (OutputTextBox.LineCount > 50)
                            OutputTextBox.Clear();
                        
                        OutputTextBox.Text += $"{args.Data} \r\n";
                    }));
                }));
            };
            instance.Start();
            instance.BeginOutputReadLine();
        }

        private void DownloadGeoFiles(string url, ProgressBar progressBar, Label label, Button button, bool isForced)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                label.Visibility = Visibility.Hidden;
                progressBar.Visibility = Visibility.Visible;
                progressBar.Value = 0;
                button.IsEnabled = false;
            }));

            var sourceFile = Path.GetFileName(url);
            var targetFile = $"{sourceFile}.temp";
            var backupFile = $"{sourceFile}.bak";
            if (File.Exists(targetFile) && !isForced)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(targetFile);
                if (lastWriteTime.Day == DateTime.Today.Day)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        label.Visibility = Visibility.Visible;
                        progressBar.Visibility = Visibility.Hidden;
                        button.IsEnabled = true;
                        label.Content = lastWriteTime.ToString("yyyy-MM-dd");
                    }));

                    return;
                }
            }

            using (WebClient client = new WebClient())
            {
                client.Proxy = new WebProxy(settings.ProxyHost, settings.ProxyPort);
                client.DownloadProgressChanged += (sender, args) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                        {
                            progressBar.Value = args.ProgressPercentage;
                        }
                    ));
                };
                client.DownloadFileCompleted += (sender, args) =>
                {
                    if (!File.Exists(targetFile))
                        return;

                    DateTime creationTime = File.GetLastWriteTime(targetFile);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        label.Visibility = Visibility.Visible;
                        progressBar.Visibility = Visibility.Hidden;
                        button.IsEnabled = true;
                        label.Content = creationTime.ToString("yyyy-MM-dd");
                    }));
                    
                    if (File.Exists(backupFile))
                        File.Delete(backupFile);
                    
                    File.Move(sourceFile, backupFile);
                    
                    File.Move(targetFile, sourceFile);
                };

                try
                {
                    client.DownloadFileAsync(new Uri(url), targetFile);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        label.Content = "download failed";
                        label.Visibility = Visibility.Visible;
                        progressBar.Visibility = Visibility.Hidden;
                        button.IsEnabled = true;
                    }));
                }
            }
        }

        private void DownloadV2ray(string url, ProgressBar progressBar, Label label, Button button, bool isForced)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                label.Visibility = Visibility.Hidden;
                progressBar.Visibility = Visibility.Visible;
                progressBar.Value = 0;
                button.IsEnabled = false;
            }));

            var fileName = Path.GetFileName(url);
            if (File.Exists(fileName) && !isForced)
            {
                DateTime lastWriteTime = File.GetLastWriteTime(fileName);
                if (lastWriteTime.Day == DateTime.Today.Day)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        label.Visibility = Visibility.Visible;
                        progressBar.Visibility = Visibility.Hidden;
                        button.IsEnabled = true;
                        label.Content = lastWriteTime.ToString("yyyy-MM-dd");
                    }));


                    return;
                }
            }

            using (WebClient client = new WebClient())
            {
                client.Proxy = new WebProxy(settings.ProxyHost, settings.ProxyPort);
                
                client.DownloadProgressChanged += (sender, args) =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        progressBar.Value = args.ProgressPercentage;
                    }));
                };
                
                client.DownloadFileCompleted += (sender, args) =>
                {
                    DateTime creationTime = File.GetLastWriteTime(fileName);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        label.Visibility = Visibility.Visible;
                        progressBar.Visibility = Visibility.Hidden;
                        button.IsEnabled = true;
                        label.Content = creationTime.ToString("yyyy-MM-dd");
                    }));
                };
                client.DownloadFileCompleted += V2rayDownloadCompleted;

                try
                {
                    client.DownloadFileAsync(new Uri(url), fileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                    
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        label.Content = "download failed";
                        label.Visibility = Visibility.Visible;
                        progressBar.Visibility = Visibility.Hidden;
                        button.IsEnabled = true;
                    }));
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

        private void Download()
        {
            try
            {
                signal.WaitOne();
                DownloadGeoFiles(settings.UrlOfGeoIp, GeoIpDownloadBar, GeoIpLabel, GeoIpDownloadButton, false);
                DownloadGeoFiles(settings.UrlOfGeoSite, GeoSiteDownloadBar, GeoSiteLabel, GeoSiteDownloadButton, false);
                DownloadV2ray(settings.UrlOfV2ray, V2rayDownloadBar, V2rayLabel, V2rayDownloadButton, false);
            }
            catch (Exception exception)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    GeoIpDownloadBar.Visibility = Visibility.Collapsed;
                    GeoSiteDownloadBar.Visibility = Visibility.Collapsed;
                    V2rayDownloadBar.Visibility = Visibility.Collapsed;

                    GeoIpLabel.Visibility = Visibility.Visible;
                    GeoSiteLabel.Visibility = Visibility.Visible;
                    V2rayLabel.Visibility = Visibility.Visible;

                    GeoIpLabel.Content = "Network is unavailable";
                    GeoSiteLabel.Content = "Network is unavailable";
                    V2rayLabel.Content = "Network is unavailable";
                }));
            }
        }
    }
}