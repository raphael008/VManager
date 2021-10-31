using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace VManager
{
    public class ReflectionHelper
    {
        public static string GetFileUnderApplicationFolder(string filename)
        {
            string location = Assembly.GetExecutingAssembly().Location;
            
            if (string.IsNullOrWhiteSpace(location))
                throw new Exception();

            var directory = Path.GetDirectoryName(location);
            if (string.IsNullOrWhiteSpace(directory))
                throw new Exception();
            
            return Path.Combine(directory, filename);
        }

        public static void SetWorkingDirectoy()
        {
            string location = Assembly.GetExecutingAssembly().Location;
            
            if (string.IsNullOrWhiteSpace(location))
                throw new Exception();

            var directory = Path.GetDirectoryName(location);
            if (string.IsNullOrWhiteSpace(directory))
                throw new Exception();
            
            Directory.SetCurrentDirectory(directory);
        }
    }
}