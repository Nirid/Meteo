using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Xml.Linq;
using System.Globalization;

namespace Meteo
{
    class XMLManager
    {
        public XMLManager(string path)
        {
            this.Path = path + "\\settings.xml";
            if(File.Exists(Path))
            {
                try
                {
                    Doc = XDocument.Load(Path);
                }
                catch(Exception Ex)
                {
                    Logging.Log(Ex.ToString());
                    InitializeDocument();
                }
            }else
            {
                InitializeDocument();
            }
        }
        private readonly string Path;
        private XDocument Doc;
        public Dictionary<Location, DateTime> SavedLocations;
        private static CultureInfo IC = CultureInfo.InvariantCulture;
        public IEnumerable<Location> AllLocations => Doc.Descendants("Location").Select(x => XElementToLocation(x));
        public Location DefaultLocation => Doc.Descendants("LastLocation").Select(x => XElementToLocation(x)).Single();
        
        private void InitializeDocument()
        {
            Doc = new XDocument(new XElement("Root",
                new XElement("Settings",
                new XElement("LastLocation",
                new XElement("X", "180"),
                new XElement("Y", "400")),
                new XElement("LastUpdateDate", DateTime.MinValue.ToString(IC))),
                new XElement("Locations",
                Location.Cities.AllCities.Select(x => LocationToXlement(x)))));
            Doc.Declaration = new XDeclaration("1.0", "UTF-8", null);
            Doc.Save(Path);
        }

        public bool AddLocation(Location location)
        {
            var all = AllLocations.ToList();
            if (all.Any(x => x == location))
                return false;
            all.Add(location);
            UpdateLocations(all);
            return true;
        }

        public bool ReplaceLocation(Location old, Location replacement)
        {
            if (AllLocations.Contains(old))
            {
                var all = AllLocations.ToList();
                var index = all.IndexOf(old);
                all.Remove(old);
                all.Insert(index, replacement);
                UpdateLocations(all);
                return true;
            }
            return false;
        }

        public void UpdateLocations(IEnumerable<Location> locations)
        {
            Logging.Log("Locations Updataed");
            Doc.Descendants("Locations").Single().ReplaceWith(IEnumerableToLoations(locations));
            Doc.Save(Path);
        }

        public void SetDefaultLocation(Location location)
        {
            var target = Doc.Descendants("LastLocation").Single();
            target.Element("X").Value = location.X.ToString(IC);
            target.Element("Y").Value = location.Y.ToString(IC);
            Doc.Save(Path);
        }

        private static XElement LocationToXlement(Location location)
        {
            return new XElement("Location",
                new XElement("Name",location.Name),
                new XElement("X", location.X.ToString(IC)),
                new XElement("Y", location.Y.ToString(IC)),
                new XElement("Update", location.Update.ToString(IC))
                );
        }

        private static Location XElementToLocation(XElement element)
        {
            string Name = element.Element("Name")?.Value ?? "";
            int X = Convert.ToInt32(element.Element("X").Value,IC);
            int Y = Convert.ToInt32(element.Element("Y").Value,IC);
            bool Update = Convert.ToBoolean(element.Element("Update")?.Value ?? "False",IC);
            return new Location(Name, X, Y,Update);
        }

        private static XElement IEnumerableToLoations(IEnumerable<Location> locations)
        {
            return new XElement("Locations",
                locations.Select(x => LocationToXlement(x)));
        }
        
        public void UpdateLastWeatherUpdateDate(DateTime date)
        {
            var target = Doc.Descendants("LastUpdateDate").Single();
            target.Value = date.ToString(CultureInfo.InvariantCulture);
            Doc.Save(Path);
        }

        public DateTime ReadLastUpdateDate()
        {
            var target = Doc.Descendants("LastUpdateDate").Single();
            return Convert.ToDateTime(target.Value, IC);
        }
        
    }
}
