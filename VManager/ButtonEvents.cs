using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace VManager
{
    public partial class MainWindow
    {
        private void GeoIpDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadGeoFiles(settings.UrlOfGeoIp, GeoIpDownloadBar, GeoIpLabel, GeoIpDownloadButton);
        }

        private void GeoSiteDownloadButton_OnClick(object sender, RoutedEventArgs e)
        {
            DownloadGeoFiles(settings.UrlOfGeoSite, GeoSiteDownloadBar, GeoSiteLabel, GeoSiteDownloadButton);
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
    }
}