using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Meteo
{
    class FileSet : INotifyPropertyChanged
    {
        public FileSet(Location location, DateTime date, DownloadStatus status)
        {
            Location = location;
            Date = date;
            Status = status;
        }
        public Location Location { get; }
        public DateTime Date { get; }
        private DownloadStatus status;
        public static string FolderPath;
        public DownloadStatus Status {
            get { return status; }
            set
            {
                var temp = status;
                status = value;
                if(PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs($"{temp.ToString()}"));
            } }

        public event PropertyChangedEventHandler PropertyChanged;

        public static bool operator ==(FileSet f1, FileSet f2)
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
            return f1.Status == f2.Status && f1.Location == f2.Location && f1.Date == f2.Date;
        }

        public override bool Equals(object other)
        {
            return this == (other as FileSet);
        }

        public bool Equals(FileSet other)
        {
            return this == other;
        }

        public static bool operator !=(FileSet f1, FileSet f2)
        {
            return !(f1 == f2);
        }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hash = 1777;
                hash = hash * 1231 + Location.GetHashCode();
                hash = hash * 1231 + Date.GetHashCode();
                hash = hash * 1231 + Status.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return $"{this.Location.Name} X:{this.Location.X} Y:{this.Location.Y} {this.Location.Update} | {this.Status.ToString()} | {this.Date.ToString()}";
        }

        public bool CompareLocationAndDate(FileSet other)
        {
            return this.Location == other.Location && this.Date == other.Date;
        }

        public enum DownloadStatus { ToBeDownloaded, Downloaded, IsDisplayed, DownloadFailed, NoWeatherFile, ToBeDeleted };

        public static string GetName(Location location, DateTime date) => $"Date{date.ToString("yyyy-MM-dd-HH", CultureInfo.InvariantCulture)} X{location.X.ToString(CultureInfo.InvariantCulture)} Y{location.Y.ToString(CultureInfo.InvariantCulture)} .png";
        public static string GetName(FileSet set) => GetName(set.Location, set.Date);
        public static string GetFilename(Location location, DateTime date) => FolderPath + "\\" + GetName(location, date);
        /// <summary>
        /// Returns filename corresponding to provided FileSet
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        public static string GetFilename(FileSet set) => GetFilename(set.Location, set.Date);
        /// <summary>
        /// Returns Location and DateTime form filename.
        /// </summary>
        public static (Location location, DateTime date)? GetLocationAndDate(string str)
        {
            Regex regex = new Regex(@"Date(\d{4})-(\d{1,2})-(\d{1,2})-(\d{1,2}) X(\d{2,3}) Y(\d{2,3}) .png");
            Match match = regex.Match(str);
            if (!match.Success)
                return null;
            Location location = new Location(Convert.ToInt32(match.Groups[5].Value, CultureInfo.InvariantCulture), Convert.ToInt32(match.Groups[6].Value, CultureInfo.InvariantCulture));
            DateTime date = new DateTime(Convert.ToInt32(match.Groups[1].Value, CultureInfo.InvariantCulture), Convert.ToInt32(match.Groups[2].Value, CultureInfo.InvariantCulture), Convert.ToInt32(match.Groups[3].Value, CultureInfo.InvariantCulture), Convert.ToInt32(match.Groups[4].Value, CultureInfo.InvariantCulture), 0, 0);
            return (location, date);
        }
    }
}
