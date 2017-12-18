using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace Meteo
{
    class WeatherDownloader
    {
        private static CultureInfo IC = CultureInfo.InvariantCulture;
        private async static Task<bool> DownloadWeather(FileSet set, string path)
        {
            using (var client = new WebClient())
            {
                try
                {
                    await client.DownloadFileTaskAsync(new Uri($"http://www.meteo.pl/um/metco/mgram_pict.php?ntype=0u&fdate={set.Date.ToString("yyyyMMddHH", IC)}&row={set.Location.Y.ToString(IC)}&col={set.Location.X.ToString(IC)}&lang=pl"), path);
                    return true;
                }
                catch (System.Net.WebException Ex)
                {
                    Logging.Log(Ex.ToString());
                    return false;
                }
            }
        }

        public static async Task<bool> DownloadAndVerify(FileSet set)
        {
            string filename = FileSet.GetFilename(set.Location, set.Date);
            if (await DownloadWeather(set, filename))
            {
                if (new FileInfo(filename).Length < 20000)
                {
                    set.Status = FileSet.DownloadStatus.NoWeatherFile;
                    return false;
                }
                else
                {
                    set.Status = FileSet.DownloadStatus.Downloaded;
                    return true;
                }
            }
            else
            {
                if (set.Status == FileSet.DownloadStatus.ToBeDownloaded)
                {
                    set.Status = FileSet.DownloadStatus.DownloadFailed;
                    return false;
                }
                else
                    throw new InvalidOperationException();
            }
        }     
    }
}
