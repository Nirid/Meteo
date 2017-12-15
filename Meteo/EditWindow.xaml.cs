using System;
using System.Collections.Generic;
using System.Device.Location;
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
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public EditWindow(Location location,IEnumerable<Location> allLocations, bool isEdit)
        {
            InitializeComponent();
            AllLocations = allLocations.ToList();
            AllLocations.Remove(location);
            if(isEdit)
            {
                Title = "Edycja Lokalizacji";
            }else
            {
                Title = "Tworzenie Lokalizacji";
            }
            InitialLocation = location;
            CurrentLocation = new Location(location.Name, location.X, location.Y, location.Update);
            UpdateTextBoxes(location);
        }

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }

        public Location InitialLocation;
        private Location CurrentLocation;
        private IList<Location> AllLocations;
        private GeoCoordinate CurrentCoordinate { get { return Location.LocationToGPS(CurrentLocation); } }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.ClearFocus();
            if (CurrentLocation.Name.Length < 1 || AllLocations.Any(x=>x.Name == CurrentLocation.Name))
            {
                MessageBox.Show("Nazwa musi być unikalna", "Błąd");
                return;
            }
            if(AllLocations.Contains(CurrentLocation))
            {
                MessageBox.Show("Lokalizacja o tych koordynatach już istnieje", "Błąd");
                return;
            }
            InitialLocation = new Location(CurrentLocation.Name, CurrentLocation.X, CurrentLocation.Y, CurrentLocation.Update);
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Keyboard.ClearFocus();
            DialogResult = false;
            this.Close();
        }

        private void UpdateTextBoxes(Location location)
        {
            NameTextBox.Text = location.Name;
            UpdateCheckBox.IsChecked = location.Update;
            LocationXTextBox.Text = location.X.ToString();
            LocationYTextBox.Text = location.Y.ToString();
            LocationETextBox.Text = Math.Round(CurrentCoordinate.Latitude, 2).ToString();
            LocationNTextBox.Text = Math.Round(CurrentCoordinate.Longitude, 2).ToString();
            Hyperlink link = new Hyperlink();
            var filename = MapGenerator.CreateAndSave(location);
            link.NavigateUri = new Uri(filename);
            link.RequestNavigate += Link_RequestNavigate;
            link.Inlines.Add("Sprawdź miejsce na mapie");
            ShowLocationTextBlock.Inlines.Add(link);
        }

        private Location ReadLocation()
        {
            if (!int.TryParse(LocationXTextBox.Text, out int X))
            {
                MessageBox.Show("X musi być liczbą całkowitą", "Błąd");
                UpdateTextBoxes(CurrentLocation);
                return null;
            }
            if(!int.TryParse(LocationYTextBox.Text,out int Y))
            {
                MessageBox.Show("Y musi być liczbą całkowitą", "Błąd");
                UpdateTextBoxes(CurrentLocation);
                return null;
            }
            return Location.SnapToGrid(NameTextBox.Text, X, Y);
        }

        private Location ReadGPS()
        {
            if (!double.TryParse(LocationETextBox.Text, out double E))
            {
                MessageBox.Show("E musi być liczbą", "Błąd");
                UpdateTextBoxes(CurrentLocation);
                return null;
            }
            if (!double.TryParse(LocationNTextBox.Text, out double N))
            {
                MessageBox.Show("N musi być liczbą", "Błąd");
                UpdateTextBoxes(CurrentLocation);
                return null;
            }
            if (N < 0)
                N = 0;
            else if (N > 90)
                N = 90;
            if (E < 0)
                E = 0;
            else if (E > 90)
                E = 90;

            return Location.GPSToLocation(NameTextBox.Text, new GeoCoordinate(N,E)); 
        }

        private void LocationChanged()
        {
            Location location = ReadLocation();
            if (location != null)
            {
                CurrentLocation = location;
                UpdateTextBoxes(CurrentLocation);
            }
        }

        private void GPSChanged()
        {
            Location location = ReadGPS();
            if (location != null)
            {
                CurrentLocation = location;
                UpdateTextBoxes(CurrentLocation);
            }
        }

        private void LocationETextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            GPSChanged();
        }

        private void LocationNTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            GPSChanged();
        }

        private void LocationXTextBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            LocationChanged();
        }

        private void LocationYTextBox_LostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            LocationChanged();
        }

        private void NameTextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (NameTextBox.Text.Length < 1 || AllLocations.Any(x => x.Name == NameTextBox.Text))
            {
                MessageBox.Show("Nazwa musi być unikalna", "Błąd");
            }
            else
            {
                CurrentLocation.Name = NameTextBox.Text;
            }
            UpdateTextBoxes(CurrentLocation);
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private void UpdateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CurrentLocation.Update = true;
        }

        private void UpdateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CurrentLocation.Update = false;
        }
    }
}
