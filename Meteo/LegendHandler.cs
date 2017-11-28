using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meteo
{
    class LegendHandler
    {
        public LegendHandler(string path)
        {
            LegendPath = path + "//Legend.png";
        }

        public static string LegendPath { get; private set; }
        public static event EventHandler<LegendDownloadedEventArgs> LegendDownloaded;

        public class LegendDownloadedEventArgs : EventArgs
        {
            public LegendDownloadedEventArgs(FileSet.DownloadStatus status)
            {
                this.status = status;
            }
            public FileSet.DownloadStatus status;
        }

        public bool CheckForLegendInFolder(string path)
        {
            var file = from f in Directory.GetFiles(path)
                       where f.EndsWith("Legend.png")
                       where new FileInfo(LegendPath).Length > 1 * (10 ^ 3)
                       select f;
            if (file.Count() == 0)
                return false;
            else
                return true;
        }

        public static async Task<bool> DownloadAndSaveLegend()
        {
            if (await DownloadAndVerifyLegend() == FileSet.DownloadStatus.Downloaded)
            {
                LegendDownloaded(null, new LegendDownloadedEventArgs(FileSet.DownloadStatus.Downloaded));
                return true;
            }
            else
            {
                FileSet.DownloadStatus status = await DownloadAndVerifyLegend();
                LegendDownloaded(null, new LegendDownloadedEventArgs(status));
                if (status == FileSet.DownloadStatus.Downloaded)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private static async Task<FileSet.DownloadStatus> DownloadAndVerifyLegend()
        {
            if (await Downloader.DownloadLegend(LegendPath))
            {
                FileInfo info = new FileInfo(LegendPath);
                if (info.Length < (1 * 1000))
                {
                    return FileSet.DownloadStatus.NoWeatherFile;
                }
                else
                {
                    return FileSet.DownloadStatus.Downloaded;
                }
            }
            else
            {
                return FileSet.DownloadStatus.DownloadFailed;
            }
        }
    }
}
