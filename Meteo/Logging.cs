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
        private static readonly string LogFolder = "Logs";
        private static readonly string LogPath = $"{LogFolder}\\Log {DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.log";
        private static int WriteNumber = 1;

        [Conditional("DEBUG")]
        public static void Log(string message)
        {
            if (!Directory.Exists(LogFolder))
                Directory.CreateDirectory(LogFolder);
            using (StreamWriter streamWriter = new StreamWriter(LogPath,append:true))
            {
                streamWriter.WriteLine($"{WriteNumber.ToString("D6")} {DateTime.Now} | {message}");
                streamWriter.Close();
            }
            WriteNumber++;
        }


    }
}
