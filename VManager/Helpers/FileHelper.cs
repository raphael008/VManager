using System;
using System.IO;

namespace VManager
{
    public class FileHelper
    {
        public static bool IsToday(string fileName)
        {
            if (!File.Exists(fileName))
                return false;

            DateTime lastWriteTime = File.GetLastWriteTime(fileName);
            if (lastWriteTime.Day == DateTime.Today.Day)
            {
                return true;
            }
            
            return false;
        }
    }
}