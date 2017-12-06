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

            XManager = new XMLManager(FolderPath);
            Handler = new FileHandler(FolderPath);
            LegendHandler = new LegendHandler(FolderPath);

            Files = FileHandler.FileList;
            //TODO: sprawdizić które się pokrywają z lokacjami z XManager AllLocations i ustawić name i Update odpowiednio
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
            lock (SyncObject2)
            {
                CheckForNewestWeatherFiles();
                FileHandler.CheckNewestWeather(SelectedLocation, NewestWeatherDate);
                SetTimeline(Files.Where(x => x.Status == FileSet.DownloadStatus.IsDisplayed).Single().Date);
            }
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
        /// <summary>
        /// Sets image backing WeatherImage to image backed by provided FileSet
        /// </summary>
        /// <param name="set">Image source</param>
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
                    SetTimeline(set.Date);
                }
            }
        }
        /// <summary>
        /// Sets image backing LegendImage
        /// </summary>
        private void SetLegendSource()
        {
            var uriLegend = new Uri(LegendHandler.LegendPath);
            var uriLegendLocal = new Uri(uriLegend.LocalPath);
            LegendImage.Source = new BitmapImage(uriLegendLocal);
        }
        /// <summary>
        /// Sets displayed timeline to provided DateTime
        /// </summary>
        /// <param name="date">DateTime to which timeline will be set</param>
        private void SetTimeline(DateTime date)
        {
            TimeLine.Dispatcher.BeginInvoke(new Action(() => { TimeLine.Visibility = Visibility.Visible; }));
            double LeftDistance = WeatherImage.Dispatcher.Invoke(new Func<double>(() => { return WeatherImage.Margin.Left; }));
            if (date.TimeOfDay == new TimeSpan(0, 0, 0) || date.TimeOfDay == new TimeSpan(12, 0, 0))
            {
                LeftDistance += 69.0 + 6.40278 * (DateTime.Now - date).TotalHours;
                TimeLine.Dispatcher.BeginInvoke(new Action(() => { TimeLine.X1 = TimeLine.X2 = LeftDistance; }));
            }
            else if (date.TimeOfDay == new TimeSpan(6, 0, 0) || date.TimeOfDay == new TimeSpan(18, 0, 0))
            {
                LeftDistance += 63 + 7.04167 * (DateTime.Now - date).TotalHours; ;
                TimeLine.Dispatcher.BeginInvoke(new Action(() => { TimeLine.X1 = TimeLine.X2 = LeftDistance; }));
            }
            else
                throw new ArgumentOutOfRangeException();
        }
        /// <summary>
        /// Forces UpdateTimer_Tick and resets UpdateTimer. If there is noInternetConnection also triggers NoInternetTimerTick
        /// </summary>
        private void ForceUpdate()
        {
            if (!IsInternetConnection)
            {
                NoInternetTimer.Stop();
                NoInternetTimer_Tick(this, null);
                NoInternetTimer.Start();
            }
            UpdateTimer.Stop();
            UpdateDispatcherTimer_Tick(this, null);
            UpdateTimer.Start();
        }
        /// <summary>
        /// To be used when SelectedLocation is changed
        /// </summary>
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
        /// <summary>
        /// Adds Location to Locations list
        /// </summary>
        /// <param name="location">Location to be added to list</param>
        /// <returns>Returns true if location has been added succesfully</returns>
        private bool AddLocation(Location location)
        {
            var all = XManager.AllLocations;
            if (!all.Contains(location) && !all.Any(x=>x.Name.Trim(' ').ToLower() == location.Name.Trim(' ').ToLower()))
            {
                XManager.AddLocation(location);
                var choosenLocation = (Location)CityList.SelectedItem;
                Locations = new ObservableCollection<Location>(XManager.AllLocations.ToList());
                CityList.ItemsSource = Locations;
                CityList.SelectedItem = choosenLocation;
                return true;
            }else
            {
                return false;
            }
        }
        /// <summary>
        /// Removes Location form Locations list
        /// </summary>
        /// <param name="location">Location to be removed</param>
        /// <returns>Returns true if removal is succesful</returns>
        private bool RemoveLocation(Location location)
        {
            var all = XManager.AllLocations.ToList();
            if (all.Contains(location) && location != XManager.LastLocation)
            {
                all.Remove(location);
                XManager.UpdateLocations(all);
                Locations = new ObservableCollection<Location>(all);
                CityList.ItemsSource = Locations;
                CityList.SelectedIndex = Locations.IndexOf(XManager.LastLocation);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Replaces Location in Location list
        /// </summary>
        /// <param name="oldLocation">Location to be replaced</param>
        /// <param name="newLocation">Location replacing oldLocation</param>
        /// <returns>Returns true if replacment is succesful</returns>
        private bool ReplaceLocation(Location oldLocation,Location newLocation)
        {
            var all = XManager.AllLocations.ToList();
            if (all.Contains(oldLocation) && oldLocation != XManager.LastLocation)
            {
                var tempAll = all.ToList();
                tempAll.Remove(oldLocation);
                if (!tempAll.Contains(newLocation) && !tempAll.Any(x => x.Name.Trim(' ').ToLower() == newLocation.Name.Trim(' ').ToLower()))
                {
                    var index = all.IndexOf(oldLocation);
                    all.Remove(oldLocation);
                    all.Insert(index, newLocation);
                    XManager.UpdateLocations(all);
                    Locations = new ObservableCollection<Location>(all);
                    CityList.ItemsSource = null;
                    CityList.ItemsSource = Locations;
                    CityList.SelectedIndex = index;
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if there are any Locations with Update == true which can be updated
        /// </summary>
        private void CheckForNewestWeatherFiles()
        {
            //For every location in AllLocations find Filesets in Files that have the same location then set Update property to be the same as AllLoctions Update property
            XManager.AllLocations.Select(location => (location, Files.Where(y => y.Location == location))).ToList()
                .ForEach(x => x.Item2.ToList().ForEach(y => { y.Location.Update = x.location.Update; y.Location.Name = x.location.Name; }));

            //Take newest file for every location, if it isn't newest file download it
            var updateableFiles = from file in Files
                             where (file.Location.Update == true) || (file.Location == SelectedLocation)
                             group file by file.Location into set
                             let first = set.OrderByDescending(x => x.Date).First()
                             where first.Date < NewestWeatherDate
                             select set.Where(x => x == first).Single();

            foreach (var set in updateableFiles)
                Files.Add(new FileSet(set.Location, NewestWeatherDate, FileSet.DownloadStatus.ToBeDownloaded));

            //If Locations with Update = true aren't in Files add them
            var fromXml = from location in XManager.AllLocations
                          where location.Update == true
                          where !Files.Any(x => x.Location == location)
                          select location;

            foreach (var location in fromXml)
                Files.Add(new FileSet(location, NewestWeatherDate, FileSet.DownloadStatus.ToBeDownloaded));
        }
        /// <summary>
        /// Fires when list of Locations is modified to update XML backing this list
        /// </summary>
        private void OnUpdateLocations(object Sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            XManager.UpdateLocations(Locations);
        }
        /// <summary>
        /// Fires when Weather file is downloaded
        /// </summary>
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
        /// <summary>
        /// Fires when Legend is downloaded
        /// </summary>
        private void OnLegendDownloaded(object sender, EventArgs e)
        {
            SetLegendSource();
        }
        /// <summary>
        /// Fires when interent connection is lost
        /// </summary>
        private void OnNoInternetConnection(object sender, EventArgs e)
        {
            IsInternetConnection = false;
            RefreshButton.Dispatcher.BeginInvoke(new Action(() => { RefreshButton.Background = Brushes.Red; }));
            NoInternetTimer.Start();
        }
        /// <summary>
        /// Fires when interent connetion is restored
        /// </summary>
        private void OnInternetConnection(object sender, EventArgs e)
        {
            IsInternetConnection = true;
            RefreshButton.Dispatcher.BeginInvoke(new Action(() => { RefreshButton.Background = Brushes.LimeGreen; }));
            NoInternetTimer.Stop();
        }

        
    }
}
