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

            System.IO.Directory.CreateDirectory(System.IO.Path.GetTempPath() + "Weather");
            FolderPath = System.IO.Path.GetTempPath() + "Weather";

            Handler = new FileHandler(FolderPath);
            XManager = new XMLManager(FolderPath);
            LegendHandler = new LegendHandler(FolderPath);

            Files = FileHandler.FileList;
            NewestWeatherDate = XManager.ReadLastUpdateDate();
            FileHandler.CheckNewestWeather(XManager.LastLocation, NewestWeatherDate);

            Locations = new ObservableCollection<Location>(XManager.AllLocations.ToList());
            CityList.ItemsSource = Locations;
            SelectedLocation = XManager.LastLocation;
            CityList.SelectedIndex = Locations.IndexOf(SelectedLocation);
            Locations.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnUpdateLocations);
            SetWeatherSource(Files.Where(x=>x.Location == SelectedLocation && x.Status == FileSet.DownloadStatus.Downloaded).OrderByDescending(x=>x.Date).FirstOrDefault());

            FileHandler.WeatherFileDownloaded += OnWeatherFileDownloaded;

            LegendHandler.LegendDownloaded += OnLegendDownloaded;
            if (!LegendHandler.CheckForLegendInFolder(FolderPath))
                LegendHandler.DownloadAndSaveLegend();
            else
                SetLegendSource();

            UpdateTimer = new System.Windows.Threading.DispatcherTimer();
            UpdateTimer.Tick += UpdateDispatcherTimer_Tick;
            UpdateTimer.Interval = new TimeSpan(1, 0, 0);
            UpdateTimer.Start();

            CleanupTimer = new System.Windows.Threading.DispatcherTimer();
            CleanupTimer.Tick += CleanupDispatcherTimer_Tick;
            CleanupTimer.Interval = new TimeSpan(0, 17, 1);
            CleanupTimer.Start();
        }

        public readonly string FolderPath;
        private XMLManager XManager;
        private FileHandler Handler;
        private LegendHandler LegendHandler;
        private ObservableCollection<Location> Locations;
        private System.Windows.Threading.DispatcherTimer UpdateTimer;
        private System.Windows.Threading.DispatcherTimer CleanupTimer;
        private DateTime NewestWeatherDate;
        private Location SelectedLocation;
        private FileSet Displayed;
        private ObservableCollection<FileSet> Files;
        private object SyncObject = new object();
        private object SyncObject2 = new object();

        private void UpdateDispatcherTimer_Tick(object sender, EventArgs e)
        {
            FileHandler.CheckNewestWeather(SelectedLocation, NewestWeatherDate);
        }

        private void CleanupDispatcherTimer_Tick(object sender, EventArgs e)
        {
            FileHandler.RemoveOutdatedFiles(FolderPath);
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
                    WeatherImage.Source = Bitmap;
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
            UpdateDispatcherTimer_Tick(this, null);
            UpdateTimer.Stop();
            UpdateTimer.Start();
            UpdateTextbox();
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
                    }
                    if (set.Location == SelectedLocation && (Displayed != null && ((Displayed.Location != SelectedLocation) || (Displayed.Location == SelectedLocation && set.Date > Displayed.Date)) || Displayed == null))
                    {
                        SetWeatherSource(set);
                    }
                }
            }
        }

        private void OnLegendDownloaded(object sender, EventArgs e)
        {
            SetLegendSource();
        }
    }
}
