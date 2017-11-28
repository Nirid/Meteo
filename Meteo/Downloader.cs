using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;

namespace Meteo
{
    class Downloader
    {
        private static CultureInfo IC = CultureInfo.InvariantCulture;
        public async static Task<bool> DownloadWeather(FileSet set, string path)
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

        public async static Task<bool> DownloadLegend(string path)
        {
            using (var client = new WebClient())
            {
                try
                {
                    await Task.Run(() => { client.DownloadFileTaskAsync("http://www.meteo.pl/um/metco/leg_um_pl_cbase_256.png", path); });
                    return true;
                }
                catch (System.Net.WebException Ex)
                {
                    //TODO: Enter offline mode
                    Logging.Log(Ex.ToString());
                    return false;
                }
            }
        }
    }
}
