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

            Downloader = new Downloader(FolderPath);

            Timer = new System.Windows.Threading.DispatcherTimer();
            Timer.Tick += dispatcherTimer_Tick;
            Timer.Interval = new TimeSpan(1, 0, 0);
            Timer.Start();

            Manager = new XMLManager(FolderPath);
            Locations = new ObservableCollection<Location>(Manager.AllLocations.ToList());
            CityList.ItemsSource = Locations;
            int a = Locations.IndexOf(Manager.LastLocation);
            CityList.SelectedIndex = a;

            SetLegendaSource();
            SetWeatherSource();

            Locations.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(UpdateLocations);

        }

        public readonly string FolderPath;
        private XMLManager Manager;
        private ObservableCollection<Location> Locations;
        private Downloader Downloader;
        private System.Windows.Threading.DispatcherTimer Timer;

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Downloader.FindNewestWeather();
            SetWeatherSource();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ForceUpdate();
        }

        private void SetWeatherSource()
        {
            var uriWeather = new Uri(Downloader.FullNameWeather);
            var uriWeatherLocal = new Uri(uriWeather.LocalPath);
            WeatherImage.Source = new BitmapImage(uriWeatherLocal);
        }

        private void SetLegendaSource()
        {
            var uriLegenda = new Uri(Downloader.FullNameLegenda);
            var uriLegendaLocal = new Uri(uriLegenda.LocalPath);
            LegendaImage.Source = new BitmapImage(uriLegendaLocal);
        }

        private void CityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CityList.SelectedIndex < 0 || CityList.SelectedIndex >= CityList.Items.Count)
            {
                return;
            }
            Downloader.Location = (Location)CityList.SelectedItem;
            if (Manager.LastLocation == Downloader.Location)
                SetDefaultButton.IsEnabled = false;
            else
                SetDefaultButton.IsEnabled = true;
             AutoUpdateCheckbox.IsChecked = Downloader.Location.Update;
            ForceUpdate();
        }

        private void ForceUpdate()
        {
            dispatcherTimer_Tick(this, null);
            Timer.Stop();
            Timer.Start();
            UpdateTextbox();
        }

        private void ManualMode_Checked(object sender, RoutedEventArgs e)
        {
            YTextbox.IsEnabled = true;
            XTextbox.IsEnabled = true;
            SaveNewLocalizationButton.IsEnabled = true;
            SetLocalizationButton.IsEnabled = true;
            NewLocalizationNameTextbox.IsEnabled = true;
        }

        private void ManualMode_Unchecked(object sender, RoutedEventArgs e)
        {
            YTextbox.IsEnabled = false;
            XTextbox.IsEnabled = false;
            SaveNewLocalizationButton.IsEnabled = false;
            SetLocalizationButton.IsEnabled = false;
            NewLocalizationNameTextbox.IsEnabled = false;
        }

        private void UpdateTextbox()
        {
            XTextbox.Text = Downloader.Location.X.ToString();
            YTextbox.Text = Downloader.Location.Y.ToString();
        }

        private void SetLocalizationButton_Click(object sender, RoutedEventArgs e)
        {
            if (ManualMode.IsChecked.Value)
            {
                if (int.TryParse(YTextbox.Text, out var Y) && int.TryParse(XTextbox.Text, out var X))
                {
                    var loc = Location.SnapToGrid(X, Y);
                    Downloader.Location = new Location(loc.X,loc.Y);
                    ForceUpdate();
                }
            }
        }

        private void SetDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            Manager.SetLastLocation((Location)CityList.SelectedItem);
            SetDefaultButton.IsEnabled = false;
        }

        private void SaveNewLocalizationButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NewLocalizationNameTextbox.Text;
            if(int.TryParse(YTextbox.Text, out var Y) && int.TryParse(XTextbox.Text, out var X))
            {
                var loc = Location.SnapToGrid(X, Y);
                var location = new Location(name, loc.X, loc.Y);
                if (!Manager.AllLocations.Contains(location))
                {
                    Manager.AddLocation(location);
                    Locations = new ObservableCollection<Location>( Manager.AllLocations.ToList());
                    CityList.ItemsSource = Locations;
                    CityList.SelectedItem = location;
                }
            }
        }

        private void AutoUpdateCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            var loc = (Location)CityList.SelectedItem;
            if (loc.Update == true)
                return;
            int index = Locations.IndexOf(loc);
            var replaced = Locations[index];
            Locations[index] = new Location(replaced.Name, replaced.X, replaced.Y, true);
            CityList.SelectedIndex = Locations.IndexOf(loc);
        }

        private void AutoUpdateCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            var loc = (Location)CityList.SelectedItem;
            if (loc.Update == false)
                return;
            int index = Locations.IndexOf(loc);
            var replaced = Locations[index];
            Locations[index] = new Location(replaced.Name, replaced.X, replaced.Y, false);
            CityList.SelectedIndex = Locations.IndexOf(loc);
        }

        public void UpdateLocations(object Sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Logging.Log(Sender.GetType().ToString() + " | " + e.Action.ToString());
            Manager.UpdateLocations(Locations);
        }
    }
}
