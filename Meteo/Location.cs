using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Location;

namespace Meteo
{
    class Location
    {
        public string Name;
        public int Y;
        public int X;

        public Location(string Name, int X, int Y) : this(X, Y)
        {
            this.Name = Name;
        }

        public Location(int X, int Y )
        {
            double test = (Y - 17) / 7.0;
            if (((test % 1 == 0) && test > -1 && test < 84 )|| AllowedY.Contains(Y))
                this.Y = Y;
            else
                throw new ArgumentOutOfRangeException("Y must have allowed value");
            test = (X - 17) / 7.0;
            if (((test % 1 == 0) && test > -1 && test < 60) || AllowedX.Contains(X))
                this.X = X;
            else
                throw new ArgumentOutOfRangeException("X must have allowed value");
            Name = "";
        }

        public static (int X,int Y) SnapToGrid(int X,int Y)
        {
                if ((((Y - 17) / 7.0) % 1) != 0.0)
                {
                    Y = ((int)Math.Round((Y - 17) / 7.0) * 7) + 17;
                }
                if (Y < 17)
                    Y = 17;
                if (Y > 598)
                    Y = 598;
                if ((((X - 17) / 7.0) % 1) != 0.0)
                {
                    X = ((int)Math.Round((X - 17) / 7.0) * 7) + 17;
                }
                if (X < 17)
                    X = 17;
                if (X > 430)
                    X = 430;

            return (X, Y);
            
        }

        public static Location GPSToLocation(GeoCoordinate position)
        {
            double lat = position.Latitude; //E-W
            double lon = position.Longitude; //S-N
            //Equations from multiple polynomial regression of GPS data to location data
            double X = -5.717114996 * 0.00001 * lon*lon*lon - 4.666954368 * 0.000001 * lon*lon*lat - 4.915961914 * 0.00001 * lon*lat*lat - 1.010349445 * 0.00001 * lat*lat*lat 
                + 3.578943599 * 0.001 *lon*lon - 4.964606815 * 0.01 * lon*lat + 2.635647828 * 0.001 * lat*lat + 5.091380838 * lon + 0.866611182 * lat - 68.26117676;
            double Y = -8.79278964 * 0.000001 * lon*lon*lon + 3.932780099 * 0.0001 * lon*lon*lat - 3.981124124 * 0.0000001 * lon*lat*lat + 1.188301147 * 0.00001 * lat*lat*lat
            - 3.682281501 * 0.01 * lon*lon - 1.506658693 * 0.01 * lon*lat - 2.016991693 * 0.001 * lat*lat + 1.426927733 * lon - 3.710854995 * lat + 246.7869373;
            if (X < 0)
                X = 0;
            else if (X > 59)
                X = 59;
            if (Y < 0)
                Y = 0;
            else if (Y > 83)
                Y = 83;
            return new Location( 17 + ((int)Math.Round(X))* 7, 17 + ((int)Math.Round(Y)) * 7);
        }

        public static bool operator ==(Location f1, Location f2)
        {
            if (object.ReferenceEquals(f1, f2))
            {
                return true;
            }
            if (object.ReferenceEquals(f1, null) ||
                object.ReferenceEquals(f2, null))
            {
                return false;
            }

            return f1.X == f2.X && f1.Y == f2.Y;
        }

        public override bool Equals(object other)
        {
            return this == (other as Location);
        }

        public bool Equals(Location other)
        {
            return this == other;
        }

        public static bool operator !=(Location f1, Location f2)
        {
            return !(f1 == f2);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 17;
                hash = hash * 23 + X.GetHashCode();
                hash = hash * 23 + Y.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public static class Cities 
        {
            public static Location Bialystok { get { return new Location("Białystok", 285, 379); } }
            public static Location Bydgoszcz { get { return new Location("Bydgoszcz", 199,381); } }
            public static Location Gdansk { get { return new Location("Gdańsk", 210,346); } }
            public static Location GorzowWielkoposki { get { return new Location("Gorzów Wielkopolski", 152, 390); } }
            public static Location Katowice { get { return new Location("Katowice", 215, 461); } }
            public static Location Kielce { get { return new Location("Kielce", 244,443); } }
            public static Location Krakow { get { return new Location("Kraków", 232,466); } }
            public static Location Lublin { get { return new Location("Lublin", 277,432); } }
            public static Location Lodz { get { return new Location("Łódź", 223,418); } }
            public static Location Olsztyn { get { return new Location("Olsztyn", 240, 363); } }
            public static Location Opole { get { return new Location("Opole", 196,449); } }
            public static Location Poznan { get { return new Location("Poznań", 180,400); } }
            public static Location Rzeszow { get { return new Location("Rzeszów", 269, 465); } }
            public static Location Szczecin { get { return new Location("Szczecin", 142, 370); } }
            public static Location Torun { get { return new Location("Toruń", 209, 383); } }
            public static Location Warszawa { get { return new Location("Warszawa", 250, 406); } }
            public static Location Wroclaw { get { return new Location("Wrocław", 181, 436); } }
            public static Location ZielataGora { get { return new Location("Zielata Góra", 155, 412); } }
        }

        public static readonly int[] AllowedX = { 285, 199, 210, 152, 215, 244, 232, 277, 223, 240,196,180,269,142,209,250,181,155 };
        public static readonly int[] AllowedY = { 379, 381, 346, 390, 461, 443, 466, 432, 418, 363,449,400,465,370,383,406,436,412 };
    }
}
