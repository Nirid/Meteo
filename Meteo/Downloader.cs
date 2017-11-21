using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Meteo
{
    class Downloader
    {
        public Downloader(string path)
        {
            Path = path;

            DateTime date = DateTime.Now;
            if (date.Hour < 6)
                WeatherDate = DateTime.Today.AddHours(-12);
            else if (date.Hour < 12)
                WeatherDate = DateTime.Today.AddHours(-6);
            else if (date.Hour < 18)
                WeatherDate = DateTime.Today;
            else
                WeatherDate = DateTime.Today.AddHours(6);

            CurrentWeatherDate = WeatherDate;
            DownloadLegend();

            RefreshImage();
        }

        public bool OfflineMode { get; private set; }
        public DateTime WeatherDate { get; private set; }
        private DateTime CurrentWeatherDate;
        public string DateString { get { return WeatherDate.ToString("yyyyMMddHH"); } }
        public string FullNameWeather { get { return $"{Path}\\Date{DateString}X{Location.X}Y{Location.Y}.png"; } }
        public string FullNameLegenda { get { return $"{Path}\\Legenda.png"; } }
        public Location Location = Location.Cities.Poznan;
        public readonly string Path;
      
        private void RefreshImage()
        {
            if (!File.Exists(FullNameWeather))
            {
                using (var client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile($"http://www.meteo.pl/um/metco/mgram_pict.php?ntype=0u&fdate={DateString}&row={Location.Y}&col={Location.X}&lang=pl", FullNameWeather);
                    }catch(System.Net.WebException Ex)
                    {
                        //TODO: Enter offline mode
                        Logging.Log(Ex.ToString());
                    }
                }
            }
        }

        private void DownloadLegend()
        {
            if (!File.Exists(FullNameLegenda))
            {
                using (var client = new WebClient())
                {
                    try
                    { 
                        client.DownloadFile("http://www.meteo.pl/um/metco/leg_um_pl_cbase_256.png", FullNameLegenda);
                    }catch (System.Net.WebException Ex)
                    {
                        //TODO: Enter offline mode
                        Logging.Log(Ex.ToString());
                    }
            }
            }
            var info = new FileInfo($"{Path}\\Legenda.png");
            if (info.Length < (5 * 10 ^ 3))
            {
                File.Delete(FullNameLegenda);
                DownloadLegend();
            }
        }

        internal void FindNewestWeather()
        {
            while(true)
            {
                if(!File.Exists(FullNameWeather))
                {
                    RefreshImage();
                }
                var info = new FileInfo(FullNameWeather);
                if (info.Length < (20 * 10 ^ 3))
                {
                    File.Delete(FullNameWeather);
                    WeatherDate = CurrentWeatherDate;
                    break;
                }
                else
                {
                    CurrentWeatherDate = WeatherDate;
                    WeatherDate = WeatherDate.AddHours(6);
                    RefreshImage();
                }
            }
            ClearTemp();
        }

        private DateTime? DateFromName(string name)
        {
            string[] sklad = name.Split('\\');
            string nazwa = "";
            foreach(string s in sklad)
            {
                if(s.EndsWith(".png"))
                {
                    nazwa = s;
                    break;
                }
            }
            Regex regex = new Regex(@"(20\d{2})(\d{2})(\d{2})(\d{2})");
            Match match = regex.Match(nazwa);
            if (!match.Success)
                return null;
            return new DateTime(Convert.ToInt32(match.Groups[1].Value), Convert.ToInt32(match.Groups[2].Value), Convert.ToInt32(match.Groups[3].Value),Convert.ToInt32(match.Groups[4].Value),0,0);
        }

        private Location LocationFromName(string name)
        {
            string[] sklad = name.Split('\\');
            string nazwa = "";
            foreach (string s in sklad)
            {
                if (s.EndsWith(".png"))
                {
                    nazwa = s;
                    break;
                }
            }
            Regex regex = new Regex(@"X(\d+)Y(\d+)");
            Match match = regex.Match(nazwa);
            if (!match.Success)
                return null;
            return new Location(Convert.ToInt32(match.Groups[1].Value), Convert.ToInt32(match.Groups[2].Value));
        }

        private void ClearTemp()
        {
            var NewestFiles = from file in Directory.GetFiles(Path)
                              let loc = LocationFromName(file)
                              let date = DateFromName(file)
                              where loc != null && date != null
                              where date.Value.AddDays(2) > DateTime.Now
                              group date.Value by loc into data
                              let date = data.OrderByDescending(x => x).First()
                              select $"{Path}\\Date{date.ToString("yyyyMMddHH")}X{data.Key.X}Y{data.Key.Y}.png";

            var list = NewestFiles.ToList();
            //Remove only meteograms, leave everything else alate
            foreach (var file in Directory.GetFiles(Path))
            {
                if (list.Contains(file) || file == FullNameLegenda || file == Path+"\\settings.xml")
                {
                    Logging.Log($"Ommited: {file}");
                    continue;
                }
                else
                {
                    Logging.Log($"Deleting: {file}");
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException Ex)
                    {
                        Logging.Log(Ex.ToString());
                        continue;
                    }
                }
            }    
        }
    }
}
