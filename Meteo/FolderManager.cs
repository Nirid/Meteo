using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meteo
{
    class FolderManager
    {
        public FolderManager(string path)
        {
            Path = path;
            CurrentWeatherDate = WeatherDate;
        }
        public DateTime WeatherDate { get; private set; }
        private DateTime CurrentWeatherDate;
        public string DateString { get { return WeatherDate.ToString("yyyyMMddHH"); } }
        public string FullNameWeather { get { return $"{Path}\\Date{DateString}X{Location.X}Y{Location.Y}.png"; } }
        public string FullNameLegenda { get { return $"{Path}\\Legenda.png"; } }
        public Location Location = Location.Cities.Poznan;
        public readonly string Path;
    }
}
