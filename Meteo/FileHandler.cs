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
       /// <param name="path">Path to folder with program data.</param>
        public FileHandler(string path)
        {
            Path = path;
            FileList.CollectionChanged += OnFileListCollectionchanged;
            FileList.ItemPropertyChanged += OnItemPropertyChanged;
            InternetConnection += OnInternetConnection;
            NoInternetConnection += OnNoInternetConnection;
            foreach(var set in FolderManager.CheckFolder(Path))
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
                                var task = new Task<bool>(() => { return WeatherDownloader.DownloadAndVerify(set).Result; });
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
                                File.Delete(FileSet.GetFilename(set));
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
                        var task = new Task<bool>(() => { return WeatherDownloader.DownloadAndVerify(set).Result; });
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
