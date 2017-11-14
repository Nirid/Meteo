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

            Downloader = new Downloader();

            Timer = new System.Windows.Threading.DispatcherTimer();
            Timer.Tick += dispatcherTimer_Tick;
            Timer.Interval = new TimeSpan(1, 0, 0);
            Timer.Start();

            Locations = new List<Location>() { Location.Cities.Bialystok, Location.Cities.Bydgoszcz, Location.Cities.Gdansk, Location.Cities.GorzowWielkoposki, Location.Cities.Katowice, Location.Cities.Kielce, Location.Cities.Krakow, Location.Cities.Lodz, Location.Cities.Lublin, Location.Cities.Olsztyn, Location.Cities.Opole, Location.Cities.Poznan, Location.Cities.Rzeszow, Location.Cities.Szczecin, Location.Cities.Torun, Location.Cities.Warszawa, Location.Cities.Wroclaw, Location.Cities.ZielonaGora };
            CityList.ItemsSource = Locations;
            CityList.SelectedIndex = 11;
            Downloader.Location = (Location)CityList.SelectedItem;

            SetLegendaSource();
            SetWeatherSource();

            
        }

        private List<Location> Locations;
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
            Downloader.Location = (Location)CityList.SelectedItem;
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
            if(ManualMode.IsChecked.Value)
            {
                if(int.TryParse(YTextbox.Text, out var Y) && int.TryParse(XTextbox.Text, out var X))
                {
                    if((((Y-17)/7.0) % 1) != 0.0)
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
                    Downloader.Location = new Location(X,Y);
                    ForceUpdate();
                }
            }
        }
      
    }
}
