using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;
using VManager.Constants;

namespace VManager
{
    public partial class MainWindow
    {
        private void GeoIpDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            GeoIpDownloadButton.IsEnabled = false;
            DownloadFile(settings.UrlOfGeoIp, FileConstants.GeoIp, GeoIpProgressChanged, GeoIpDownloadCompleted);
        }

        private void GeoSiteDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            GeoSiteDownloadButton.IsEnabled = false;
            DownloadFile(settings.UrlOfGeoSite, FileConstants.GeoSite, GeoSiteProgressChanged, GeoSiteDownloadCompleted);
        }
        
        private void V2rayDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            V2rayDownloadButton.IsEnabled = false;
            DownloadFile(settings.UrlOfV2ray, FileConstants.V2Ray, V2RayProgressChanged, V2RayDownloadCompleted);
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
            V2RayHelper.StartInstance(settings.V2rayPath, OutputTextBox);
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