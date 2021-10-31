using System.Threading;
using System.Windows;

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
            
            base.OnStartup(e);
        }
    }
}