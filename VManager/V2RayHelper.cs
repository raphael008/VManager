using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace VManager
{
    public class V2RayHelper
    {
        private static readonly string v2ray = "v2ray.exe";
        private static List<Process> processes = new List<Process>();
        
        public static Process GetInstance(string v2rayPath)
        {
            Process process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(v2rayPath, v2ray),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };
            
            processes.Add(process);
            
            return process;
        }

        public static void ClearInstances()
        {
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }
    }
}