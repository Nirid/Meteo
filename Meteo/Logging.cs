using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Meteo
{
    public static class Logging
    {
        private static readonly string LogFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Meteo App\\Logs";
        private static readonly string LogPath = $"{LogFolder}\\Log {DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.log";
        private static int WriteNumber = 1;
        private static object SyncObject = new object();
        /// <summary>
        /// Writes to log.
        /// </summary>
        /// <param name="message">String to be written to log</param>
        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            lock (SyncObject)
            {
                if (!Directory.Exists(LogFolder))
                    Directory.CreateDirectory(LogFolder);
                using (StreamWriter streamWriter = new StreamWriter(LogPath, append: true))
                {
                    streamWriter.WriteLine($"{WriteNumber.ToString("D6")} {DateTime.Now} | {message}");
                    streamWriter.Close();
                }
                WriteNumber++;
            }
        }


    }
}
