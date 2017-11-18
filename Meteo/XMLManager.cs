using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Xml.Linq;

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
                    InitializeDocument();
                }
            }else
            {
                InitializeDocument();
            }
        }
        private readonly string Path;
        private XDocument Doc;
        List<Location> DefaultLocations = new List<Location>() { Location.Cities.Bialystok, Location.Cities.Bydgoszcz, Location.Cities.Gdansk, Location.Cities.GorzowWielkoposki, Location.Cities.Katowice, Location.Cities.Kielce, Location.Cities.Krakow, Location.Cities.Lodz, Location.Cities.Lublin, Location.Cities.Olsztyn, Location.Cities.Opole, Location.Cities.Poznan, Location.Cities.Rzeszow, Location.Cities.Szczecin, Location.Cities.Torun, Location.Cities.Warszawa, Location.Cities.Wroclaw, Location.Cities.ZielonaGora };
        public IEnumerable<Location> AllLocations => Doc.Descendants("Location").Select(x => XElementToLocation(x));
        public Location LastLocation => Doc.Descendants("LastLocation").Select(x => XElementToLocation(x)).Single();
        
        private void InitializeDocument()
        {
            Doc = new XDocument(new XElement("Root",
                new XElement("Settings",
                new XElement("LastLocation",
                new XElement("Name","Poznań"),
                new XElement("X", "180"),
                new XElement("Y", "400"))),
                new XElement("Locations",
                DefaultLocations.Select(x => LocationToXlement(x)))));
            Doc.Declaration = new XDeclaration("1.0", "UTF-8", null);
            Doc.Save(Path);
        }

        public bool AddLocation(Location location)
        {
            if (Doc.Descendants("Location").Any(x => XElementToLocation(x) == location))
                return false;
            Doc.Descendants("Locations").Single().Add(LocationToXlement(location));
            Doc.Save(Path);
            return true;
        }

        public void SetLastLocation(Location location)
        {
            var target = Doc.Descendants("LastLocation").Single();
            target.Element("Name").Value = location.Name;
            target.Element("X").Value = location.X.ToString();
            target.Element("Y").Value = location.Y.ToString();
            Doc.Save(Path);
        }

        private XElement LocationToXlement(Location location)
        {
            return new XElement("Location",
                new XElement("Name",location.Name),
                new XElement("X", location.X.ToString()),
                new XElement("Y", location.Y.ToString()));
        }

        private Location XElementToLocation(XElement element)
        {
            string Name = element.Element("Name").Value;
            int X = Convert.ToInt32(element.Element("X").Value);
            int Y = Convert.ToInt32(element.Element("Y").Value);
            return new Location(Name, X, Y);
        }

        

        
    }
}
