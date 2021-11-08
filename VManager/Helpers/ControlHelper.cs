using System;
using System.Windows.Controls;

namespace VManager
{
    public class ControlHelper
    {
        public static void EnableButton(Button button)
        {
            if (button.Dispatcher.CheckAccess())
            {
                button.IsEnabled = true;
            }
            else
            {
                button.Dispatcher.BeginInvoke(new Action(() =>
                {
                    button.IsEnabled = true;
                }));
            }
        }
    }
}