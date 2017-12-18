using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Meteo
{
    static class Geocoding
    {
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
                    if (status.Value != "OK")
                        return null;
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

                    var loc = Location.GPSToLocation(new System.Device.Location.GeoCoordinate(lon, lat));
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
