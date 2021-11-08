using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace VManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static Mutex mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "VManager";
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Current.Shutdown();
            }

            AppDomain.CurrentDomain.UnhandledException += DispatcherUnhandledExceptionHandler;
            
            base.OnStartup(e);
        }
        
        private void DispatcherUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.ExceptionObject.ToString());
            
            Current.Shutdown();
        }
    }
}