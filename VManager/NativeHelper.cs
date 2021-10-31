using System;
using System.Runtime.InteropServices;

namespace VManager
{
    public enum ShowWindowOption
    {
        SW_HIDE = 0,
        SW_SHOW = 5
    }
    
    public class NativeHelper
    {
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowOption option);
    }
}