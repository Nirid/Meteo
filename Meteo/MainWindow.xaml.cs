using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections.ObjectModel;
using System.Threading;

namespace Meteo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Meteo App");
            FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Meteo App";

            Handler = new FileHandler(FolderPath);
            XManager = new XMLManager(FolderPath);
            LegendHandler = new LegendHandler(FolderPath);

            Files = FileHandler.FileList;
            FileHandler.WeatherFileDownloaded += OnWeatherFileDownloaded;
            FileHandler.InternetConnection += OnInternetConnection;
            FileHandler.NoInternetConnection += OnNoInternetConnection;
            NewestWeatherDate = XManager.ReadLastUpdateDate();
            FileHandler.CheckNewestWeather(XManager.LastLocation, NewestWeatherDate);

            Locations = new ObservableCollection<Location>(XManager.AllLocations.ToList());
            CityList.ItemsSource = Locations;
            SelectedLocation = XManager.LastLocation;
            CityList.SelectedIndex = Locations.IndexOf(SelectedLocation);
            Locations.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnUpdateLocations);

            if (Files.Where(x => x.Location == SelectedLocation && x.Date == NewestWeatherDate).Count() == 0)
                Files.Add(new FileSet(SelectedLocation, NewestWeatherDate, FileSet.DownloadStatus.ToBeDownloaded));
            SetWeatherSource(Files.Where(x => x.Location == SelectedLocation && x.Status == FileSet.DownloadStatus.Downloaded).OrderByDescending(x => x.Date).FirstOrDefault());

            LegendHandler.LegendDownloaded += OnLegendDownloaded;
            if (!LegendHandler.CheckForLegendInFolder(FolderPath))
                LegendHandler.DownloadAndSaveLegend();
            else
                SetLegendSource();

            UpdateTimer = new System.Windows.Threading.DispatcherTimer();
            UpdateTimer.Tick += UpdateDispatcherTimer_Tick;
            UpdateTimer.Interval = new TimeSpan(0, 10, 0);
            UpdateTimer.Start();

            CleanupTimer = new System.Windows.Threading.DispatcherTimer();
            CleanupTimer.Tick += CleanupDispatcherTimer_Tick;
            CleanupTimer.Interval = new TimeSpan(0, 0, 5);
            CleanupTimer.Start();

            NoInternetTimer = new System.Windows.Threading.DispatcherTimer();
            NoInternetTimer.Tick += NoInternetTimer_Tick;
            NoInternetTimer.Interval = new TimeSpan(0, 1, 0);

            var poznan = Downloader.GetLocation("poznan");
            var warszawa = Downloader.GetLocation("warszawa");

        }

        public readonly string FolderPath;
        private XMLManager XManager;
        private FileHandler Handler;
        private LegendHandler LegendHandler;
        private ObservableCollection<Location> Locations;
        private System.Windows.Threading.DispatcherTimer UpdateTimer;
        private System.Windows.Threading.DispatcherTimer CleanupTimer;
        private System.Windows.Threading.DispatcherTimer NoInternetTimer;
        private DateTime NewestWeatherDate;
        private Location SelectedLocation;
        private FileSet Displayed;
        private ObservableCollection<FileSet> Files;
        private object SyncObject = new object();
        private object SyncObject2 = new object();
        private bool IsInternetConnection = true;

        private void UpdateDispatcherTimer_Tick(object sender, EventArgs e)
        {
            CheckForNewestWeatherFiles();
            FileHandler.CheckNewestWeather(SelectedLocation, NewestWeatherDate);
        }

        private void CleanupDispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!Files.Any(x => x.Status == FileSet.DownloadStatus.ToBeDownloaded || x.Status == FileSet.DownloadStatus.ToBeDeleted))
                FileHandler.RemoveOutdatedFiles(FolderPath);
        }

        private void NoInternetTimer_Tick(object sender, EventArgs e)
        {
            var selected = Files.Where(x => x.Location == SelectedLocation).OrderByDescending(x => x.Date).ToList();
            if (selected.Count > 0)
            {
                foreach (var set in selected)
                {
                    if (set.Status != FileSet.DownloadStatus.IsDisplayed)
                    {
                        var newSet = new FileSet(set.Location, set.Date, FileSet.DownloadStatus.ToBeDownloaded);
                        Files.Remove(set);
                        Files.Add(newSet);
                        return;
                    }
                }
                Files.Add(new FileSet(SelectedLocation, NewestWeatherDate.AddHours(-6), FileSet.DownloadStatus.ToBeDownloaded));
                return;
            }else
            {
                Files.Add(new FileSet(SelectedLocation, NewestWeatherDate, FileSet.DownloadStatus.ToBeDownloaded));
                return;
            }
        }

        private void SetWeatherSource(FileSet set)
        {
            lock (SyncObject2)
            {
                if ((set != null && set.Status == FileSet.DownloadStatus.Downloaded) && ((Displayed == null) || (Displayed != null && ((Displayed.Location == SelectedLocation && set.Date > Displayed.Date) || (Displayed.Location != SelectedLocation)))))
                {
                    if (Displayed != null)
                        Displayed.Status = FileSet.DownloadStatus.Downloaded;
                    set.Status = FileSet.DownloadStatus.IsDisplayed;
                    Displayed = set;
                    var uriWeather = new Uri(FileHandler.GetFilename(set));
                    var uriWeatherLocal = new Uri(uriWeather.LocalPath);
                    var Bitmap = new BitmapImage(uriWeatherLocal);
                    Bitmap.Freeze();
                    WeatherImage.Dispatcher.BeginInvoke(new Action(() => WeatherImage.Source = Bitmap));
                }
            }
        }

        private void SetLegendSource()
        {
            var uriLegend = new Uri(LegendHandler.LegendPath);
            var uriLegendLocal = new Uri(uriLegend.LocalPath);
            LegendaImage.Source = new BitmapImage(uriLegendLocal);
        }

        private void ForceUpdate()
        {
            if (!IsInternetConnection)
                NoInternetTimer_Tick(this, null);
            UpdateDispatcherTimer_Tick(this, null);
            UpdateTimer.Stop();
            UpdateTimer.Start();
        }

        private void SelectedLocationChanged()
        {
            UpdateTextbox();
            var other = Files.Where(x => x.Location == SelectedLocation && (x.Status == FileSet.DownloadStatus.Downloaded || x.Status == FileSet.DownloadStatus.IsDisplayed)).OrderByDescending(x => x.Date).ToList();
            if (other.Count == 0)
            {
                Files.Add(new FileSet(SelectedLocation, NewestWeatherDate, FileSet.DownloadStatus.ToBeDownloaded));
                return;
            }
            else
            {
                var first = other.First();
                if (first.Date < NewestWeatherDate)
                {
                    Files.Add(new FileSet(SelectedLocation, NewestWeatherDate, FileSet.DownloadStatus.ToBeDownloaded));
                }
                SetWeatherSource(first);
            }
        }

        private void CheckForNewestWeatherFiles()
        {
            var Updateable = from file in Files
                             where (file.Location.Update == true) || (file.Location == SelectedLocation)
                             group file by file.Location into set
                             let first = set.OrderByDescending(x => x.Date).First()
                             where first.Date < NewestWeatherDate
                             select set.Where(x => x == first).Single();

            foreach (var set in Updateable)
                Files.Add(new FileSet(set.Location, NewestWeatherDate, FileSet.DownloadStatus.ToBeDownloaded));
        }

        private void OnUpdateLocations(object Sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Logging.Log(Sender.GetType().ToString() + " | " + e.Action.ToString());
            XManager.UpdateLocations(Locations);
        }

        private void OnWeatherFileDownloaded(object sender, EventArgs e)
        {
            lock (SyncObject)
            {
                var set = (FileSet)sender;
                if (set.Status == FileSet.DownloadStatus.Downloaded)
                {
                    if (set.Date > NewestWeatherDate)
                    {
                        NewestWeatherDate = set.Date;
                        XManager.UpdateLastWeatherUpdateDate(set.Date);
                        CheckForNewestWeatherFiles();
                    }
                    if (set.Location == SelectedLocation && (Displayed != null && ((Displayed.Location != SelectedLocation) || (Displayed.Location == SelectedLocation && set.Date > Displayed.Date)) || Displayed == null))
                    {
                        SetWeatherSource(set);
                    }
                }
                else if (set.Status == FileSet.DownloadStatus.DownloadFailed)
                {
                    
                }
                else if (set.Status == FileSet.DownloadStatus.NoWeatherFile)
                {
                    
                }
            }
        }

        private void OnLegendDownloaded(object sender, EventArgs e)
        {
            SetLegendSource();
        }

        private void OnNoInternetConnection(object sender, EventArgs e)
        {
            IsInternetConnection = false;
            RefreshButton.Dispatcher.BeginInvoke(new Action(() => { RefreshButton.Background = Brushes.Red; }));
            NoInternetTimer.Start();
        }

        private void OnInternetConnection(object sender, EventArgs e)
        {
            IsInternetConnection = true;
            RefreshButton.Dispatcher.BeginInvoke(new Action(() => { RefreshButton.Background = Brushes.LimeGreen; }));
            NoInternetTimer.Stop();
        }

    }
}
