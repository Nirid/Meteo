using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            public static Location ZielonaGora { get { return new Location("Zielona Góra", 155, 412); } }
        }

        public static readonly int[] AllowedX = { 285, 199, 210, 152, 215, 244, 232, 277, 223, 240,196,180,269,142,209,250,181,155 };
        public static readonly int[] AllowedY = { 379, 381, 346, 390, 461, 443, 466, 432, 418, 363,449,400,465,370,383,406,436,412 };
    }
}
