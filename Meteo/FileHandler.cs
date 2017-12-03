using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Xml.Linq;

namespace Meteo
{
    partial class FileHandler
    {
        public FileHandler(string path,XMLManager document)
        {
            Path = path;
            FileList.CollectionChanged += OnFileListCollectionchanged;
            FileList.ItemPropertyChanged += OnItemPropertyChanged;
            InternetConnection += OnInternetConnection;
            NoInternetConnection += OnNoInternetConnection;
            XManager = document;
            foreach(var set in CheckFolder(Path))
            {
                FileList.Add(set);
            }

        }

        public static string Path { get; private set; }
        private static readonly CultureInfo IC = CultureInfo.InvariantCulture;
        public static CustomObservableCollection<FileSet> FileList = new CustomObservableCollection<FileSet>();
        private static readonly object SyncObject = new object();
        public static event EventHandler NoInternetConnection;
        public static event EventHandler InternetConnection;
        public static bool IsInternetConnection = true;
        private static XMLManager XManager;

        public static string GetName(Location location, DateTime date) => $"Date{date.ToString("yyyy-MM-dd-HH", IC)} X{location.X.ToString(IC)} Y{location.Y.ToString(IC)} .png";
        public static string GetName(FileSet set) => GetName(set.Location, set.Date);
        public static string GetFilename(Location location, DateTime date) => Path + "\\" + GetName(location, date);
        public static string GetFilename(FileSet set) => GetFilename(set.Location, set.Date);

        public static (Location location, DateTime date)? GetLocationAndDate(string str)
        {
            Regex regex = new Regex(@"Date(\d{4})-(\d{1,2})-(\d{1,2})-(\d{1,2}) X(\d{2,3}) Y(\d{2,3}) .png");
            Match match = regex.Match(str);
            if (!match.Success)
                return null;
            Location location = new Location(Convert.ToInt32(match.Groups[5].Value, IC), Convert.ToInt32(match.Groups[6].Value, IC));
            DateTime date = new DateTime(Convert.ToInt32(match.Groups[1].Value, IC), Convert.ToInt32(match.Groups[2].Value, IC), Convert.ToInt32(match.Groups[3].Value, IC), Convert.ToInt32(match.Groups[4].Value, IC), 0, 0);
            return (location, date);
        }

        protected class ListChangedEventArgs
        {
            public ListChangedEventArgs(NotifyCollectionChangedEventArgs e)
            {
                CollectionChangedEventArgs = e;
            }
            public ListChangedEventArgs(PropertyChangedEventArgs e)
            {
                PropertyChangedEventArgs = e;
            }
            public NotifyCollectionChangedEventArgs CollectionChangedEventArgs;
            public PropertyChangedEventArgs PropertyChangedEventArgs;
        }

        private void OnFileListCollectionchanged(object sender, NotifyCollectionChangedEventArgs e) => OnChange(sender, new ListChangedEventArgs(e));

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) => OnChange(sender, new ListChangedEventArgs(e));

        private void OnChange(object sender, ListChangedEventArgs e)
        {
            lock(SyncObject)
            {
                if(e.CollectionChangedEventArgs != null && e.PropertyChangedEventArgs == null)
                {
                    NotifyCollectionChangedEventArgs args = e.CollectionChangedEventArgs;
                    if (args.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (FileSet set in args.NewItems)
                        {
                            if (set.Status == FileSet.DownloadStatus.ToBeDownloaded)
                            {
                                var task = new Task<bool>(() => { return DownloadAndVerify(set).Result; });
                                task.Start();
                                return;
                            }
                            else if (set.Status == FileSet.DownloadStatus.Downloaded)
                            {
                                return;
                            }
                            else
                                throw new ArgumentException();
                        }
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Replace)
                    {
                        throw new NotImplementedException();
                    }
                    else if (args.Action == NotifyCollectionChangedAction.Remove)
                    {
                        foreach (FileSet set in args.OldItems)
                        {
                            try
                            {
                                File.Delete(GetFilename(set));
                            }catch(IOException Ex)
                            {
                                Logging.Log(Ex.ToString());
                            }
                        }
                    }
                    else
                        throw new ArgumentException();
                }
                else if(e.PropertyChangedEventArgs != null && e.CollectionChangedEventArgs == null)
                {
                    if (!Enum.TryParse(e.PropertyChangedEventArgs.PropertyName, out FileSet.DownloadStatus previousStatus))
                        throw new InvalidOperationException();
                    FileSet set = (FileSet)sender;
                    if (set.Status == FileSet.DownloadStatus.Downloaded && previousStatus == FileSet.DownloadStatus.ToBeDownloaded)
                    {
                        WeatherFileDownloaded(set, EventArgs.Empty);
                        InternetConnection(set, EventArgs.Empty);
                        return;
                    }
                    else if (set.Status == FileSet.DownloadStatus.ToBeDeleted)
                    {
                        FileList.Remove(set);
                        return;
                    }
                    else if (set.Status == FileSet.DownloadStatus.IsDisplayed && previousStatus == FileSet.DownloadStatus.Downloaded)
                    {
                        return;
                    }
                    else if (set.Status == FileSet.DownloadStatus.Downloaded && previousStatus == FileSet.DownloadStatus.IsDisplayed)
                    {
                        return;
                    }
                    else if (set.Status == FileSet.DownloadStatus.DownloadFailed && previousStatus == FileSet.DownloadStatus.ToBeDownloaded)
                    {
                        WeatherFileDownloaded(set, EventArgs.Empty);
                        NoInternetConnection(set, EventArgs.Empty);
                        return;
                    }
                    else if (set.Status == FileSet.DownloadStatus.NoWeatherFile && previousStatus == FileSet.DownloadStatus.ToBeDownloaded)
                    {
                        WeatherFileDownloaded(set, EventArgs.Empty);
                        InternetConnection(set, EventArgs.Empty);
                        set.Status = FileSet.DownloadStatus.ToBeDeleted;
                        return;
                    }else if (set.Status == FileSet.DownloadStatus.ToBeDownloaded && previousStatus == FileSet.DownloadStatus.DownloadFailed)
                    {
                        var task = new Task<bool>(() => { return DownloadAndVerify(set).Result; });
                        task.Start();
                        return;
                    }
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        public static event EventHandler<EventArgs> WeatherFileDownloaded;

        public static void CheckNewestWeather(Location location, DateTime lastUpdateDate)
        {
            var now = DateTime.Now;
            DateTime startDate;
            if(now.Hour > 18)
            {
                startDate = now.Date;
            }else if(now.Hour > 12)
            {
                startDate = now.Date.AddHours(-6);
            }else if(now.Hour > 6)
            {
                startDate = now.Date.AddHours(-12);
            }else
            {
                startDate = now.Date.AddHours(-18);
            }

            while(startDate < now)
            {
                var set = new FileSet(location, startDate, FileSet.DownloadStatus.ToBeDownloaded);
                if (startDate > lastUpdateDate && (FileList.Where(x => x.Location == set.Location && x.Date == set.Date).Count() == 0))
                    FileList.Add(set);
                startDate = startDate.AddHours(6);
            }
        }

        private static async Task<bool> DownloadAndVerify(FileSet set)
        {
            string filename = GetFilename(set.Location, set.Date);
            if (await Downloader.DownloadWeather(set, filename))
            {
                if (new FileInfo(filename).Length < 20000)
                {
                    set.Status = FileSet.DownloadStatus.NoWeatherFile;
                    return false;
                }
                else
                {
                    set.Status = FileSet.DownloadStatus.Downloaded;
                    return true;
                }
            }
            else
            {
                if (set.Status == FileSet.DownloadStatus.ToBeDownloaded)
                {
                    set.Status = FileSet.DownloadStatus.DownloadFailed;
                    return false;
                }
                else
                    throw new InvalidOperationException();
            }
        }

        private static void OnInternetConnection(object sendet, EventArgs e)
        {
            IsInternetConnection = true;
            foreach(var file in FileList.Where(x=>x.Status == FileSet.DownloadStatus.DownloadFailed))
            {
                file.Status = FileSet.DownloadStatus.ToBeDownloaded;
            }
        }

        private static void OnNoInternetConnection(object sender, EventArgs e)
        {
            IsInternetConnection = false;
        }
    }
}
