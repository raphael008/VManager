using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace VManager
{
    public partial class MainWindow
    {
        private void GeoIpDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadGeoFiles(settings.UrlOfGeoIp, GeoIpDownloadBar, GeoIpLabel, GeoIpDownloadButton, true);
        }

        private void GeoSiteDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadGeoFiles(settings.UrlOfGeoSite, GeoSiteDownloadBar, GeoSiteLabel, GeoSiteDownloadButton, true);
        }
        
        private void V2rayDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadV2ray(settings.UrlOfV2ray, V2rayDownloadBar, V2rayLabel, V2rayDownloadButton, true);
        }
        
        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            key?.SetValue(assembly.GetName().Name, assembly.Location);
        }

        private void ToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            key?.DeleteValue(assembly.GetName().Name);
        }
        
        private void CheckAutoStartStatus()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (key?.GetValue(assembly.GetName().Name) != null)
            {
                AutoStartCheckBox.IsChecked = true;
            }
        }
        
        private void RestartServiceButton_OnClick(object sender, RoutedEventArgs e)
        {
            OutputTextBox.Clear();
            V2RayHelper.ClearInstances();
            StartV2rayInstance();
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
        
        private IntPtr GetMainWindowHandle()
        {
            var window = Application.Current.MainWindow;
            if (window != null)
            {
                return new WindowInteropHelper(window).Handle;
            }
            return IntPtr.Zero;
        }
        
        private new void Hide()
        {
            if (WindowState != WindowState.Minimized)
            {
                WindowState = WindowState.Minimized;
            }

            ShowInTaskbar = false;
            NativeHelper.ShowWindow(GetMainWindowHandle(), ShowWindowOption.SW_HIDE);
        }

        private new void Show()
        {
            if (WindowState != WindowState.Normal)
            {
                WindowState = WindowState.Normal;
            }

            ShowInTaskbar = true;
            NativeHelper.ShowWindow(GetMainWindowHandle(), ShowWindowOption.SW_SHOW);
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }
    }
}