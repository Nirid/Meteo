using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool CompareLocationAndDate(FileSet other)
        {
            return this.Location == other.Location && this.Date == other.Date;
        }

        public enum DownloadStatus { ToBeDownloaded, Downloaded, IsDisplayed, DownloadFailed, NoWeatherFile, ToBeDeleted };
    }
}
