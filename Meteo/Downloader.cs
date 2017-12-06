using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Threading;
using System.Xml.Linq;
using System.Text.RegularExpressions;

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
                    await client.DownloadFileTaskAsync("http://www.meteo.pl/um/metco/leg_um_pl_cbase_256.png", path);
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

        public static Location GetLocation(string name)
        {
            try
            {
                name = name + " , Poland";
                string requestUri = $"https://maps.googleapis.com/maps/api/geocode/xml?address={name}&language=pl&sensor=false&key=AIzaSyAG_omb2mFPj-8lRFp9MebC3ZZiVauMLiE";
                WebRequest request = WebRequest.Create(requestUri);
                using (WebResponse response = request.GetResponse())
                {
                    XDocument xdoc = XDocument.Load(response.GetResponseStream());
                    XElement result = xdoc.Element("GeocodeResponse").Element("result");
                    XElement status = xdoc.Element("GeocodeResponse").Element("status");
                    XElement location = result.Element("geometry").Element("location");
                    double lat = Convert.ToDouble(location.Element("lat").Value, CultureInfo.InvariantCulture);
                    double lon = Convert.ToDouble(location.Element("lng").Value, CultureInfo.InvariantCulture);
                    string address = result.Element("formatted_address").Value;
                    string[] splitedByComma = address.Split(',');
                    Regex code = new Regex(@"^(\d{2})(-\d{3})?$");
                    Regex poland = new Regex("^ Polska$");
                    string finalName = "";
                    foreach (string split in splitedByComma)
                    {
                        if (code.IsMatch(split) || (poland.IsMatch(split)))
                            continue;
                        else
                            finalName = split + " ";
                    }
                    finalName = Regex.Replace(finalName, @"(\d{2}(-\d{3}))?", "");
                    finalName = finalName.Trim(' ');

                    var loc = Location.GPSToLocation(new System.Device.Location.GeoCoordinate(lat, lon));
                    loc.Name = finalName;
                    return loc;
                }
            }
            catch (WebException Ex)
            {
                Logging.Log(Ex.ToString());
            }
            return null;
        }
    }
}
