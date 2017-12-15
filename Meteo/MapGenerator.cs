using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Meteo
{
    class MapGenerator
    {
        public MapGenerator(string path)
        {
            FolderPath = path;
        }
        private static string FolderPath;

        public static void DisplayLocation(Location location)
        {
            OpenBrowser(
                SaveHtml(
                    GenerateHtml(Location.LocationToGPS(location))));
        }

        public static string CreateAndSave(Location location)
        {
            return SaveHtml(GenerateHtml(Location.LocationToGPS(location)));
        }

        static void OpenBrowser(string fileName)
        {
            System.Diagnostics.Process.Start(fileName);
        }

        static string SaveHtml(string html)
        {
            string fileName = FolderPath + "\\T" + DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture) + ".html";
            File.WriteAllText(fileName,html);
            return fileName;
        }

        public static void Cleanup(string folderPath)
        {
            Regex regex = new Regex(@"^T(\d+)\.html$");
            DateTime dayAgo = DateTime.Now.AddDays(-1);

            var fileList = from file in Directory.GetFiles(folderPath)
                           let filename = Path.GetFileName(file)
                           let rex = regex.Match(filename)
                           where rex.Success
                           where new DateTime(Convert.ToInt64(rex.Groups[1].Value)) < dayAgo
                           select file;

            foreach(var filename in fileList)
            {
                try
                {
                    File.Delete(filename);
                }
                catch (IOException)
                {
                    continue;
                }
            }
        }

        static string GenerateHtml(GeoCoordinate coordinate)
        {
            string latitude = coordinate.Latitude.ToString(CultureInfo.InvariantCulture);
            string longitude = coordinate.Longitude.ToString(CultureInfo.InvariantCulture);
            return @"<!DOCTYPE html>
<html>
  <head>
    <meta name=""viewport"" content=""initial-scale=1.0, user-scalable=no"">
            <meta charset=""utf-8"">
         
             <title> Przybliżona lokalizacja miejsca </title>
         
             <style>
        #map {
        height: 100%;
        }
        html, body {
        height: 100%;
        margin: 0;
        padding: 0;
      }
    </style>
    <script>
      function initMap()
    {
        var map = new google.maps.Map(document.getElementById('map'), {
          zoom: 10,
		  scaleControl: true,
          center: {"
        + $" lat: {latitude}, lng: {longitude}"
        + @"},
          mapTypeId: 'roadmap'
        });

        var circle = new google.maps.Circle({
      strokeColor: '#FF0000',
      strokeOpacity: 0.8,
      strokeWeight: 2,
      fillColor: '#FF0000',
      fillOpacity: 0.35,
      map: map,
      center: {"
        + $"lat: {latitude}, lng: {longitude}"
        + @"},
      radius: 32000
        });
      }
    </script>
  </head>
  <body>
    <div id = ""map"" ></div>
    <script async defer
     src = ""https://maps.googleapis.com/maps/api/js?key=AIzaSyBDhAwZB_s9vQ472brW7b3c58EZT2LyOXU&callback=initMap"">
 
     </script>
 
   </body>
 </html> ";
        }
    }
}
