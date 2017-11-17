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
            if(File.Exists(this.Path))
            {
                try
                {
                    Doc = XDocument.Load(this.Path);
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
        List<Location> DefaultLocations = (new List<Location>() { Location.Cities.Bialystok, Location.Cities.Bydgoszcz, Location.Cities.Gdansk, Location.Cities.GorzowWielkoposki, Location.Cities.Katowice, Location.Cities.Kielce, Location.Cities.Krakow, Location.Cities.Lodz, Location.Cities.Lublin, Location.Cities.Olsztyn, Location.Cities.Opole, Location.Cities.Poznan, Location.Cities.Rzeszow, Location.Cities.Szczecin, Location.Cities.Torun, Location.Cities.Warszawa, Location.Cities.Wroclaw, Location.Cities.ZielonaGora }).OrderBy(x=>x.X).ThenBy(x=>x.Y).ToList();

        private void InitializeDocument()
        {
            Doc = new XDocument(new XElement("Root",
                new XElement("Settings",
                new XElement("LastLocation",
                new XElement("X", "180"),
                new XElement("Y", "400"))),
                new XElement("Locations",
                DefaultLocations.Select(x => LocationToXlement(x)))));
            Doc.Declaration = new XDeclaration("1.0", "UTF-8", null);
            Doc.Save(Path);
        }

        public bool AddLocation(Location location)
        {
            throw new NotImplementedException();
        }

        public void SetLastLocation(Location location)
        {
            
        }

        private XElement LocationToXlement(Location location)
        {
            return new XElement("Location",
                new XElement("Name",location.Name),
                new XElement("X", location.X.ToString()),
                new XElement("Y", location.Y.ToString()));
        }

        public IOrderedEnumerable<Location> ReturnAllLocations()
        {
            return Doc.Descendants("Location").Select(x=>new Location(x.Element("Name").Value,Convert.ToInt32(x.Element("X").Value),Convert.ToInt32(x.Element("Y").Value))).OrderBy(x=>x.X).ThenBy(x=>x.Y);
        }

        public Location ReturnLastLocation()
        {
            return Doc.Descendants("LastLocation").Select(x => new Location(Convert.ToInt32(x.Element("X").Value), Convert.ToInt32(x.Element("Y").Value))).Single();
        }
    }
}
