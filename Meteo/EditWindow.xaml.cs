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
using System.Windows.Shapes;

namespace Meteo
{
    /// <summary>
    /// Interaction logic for EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        public EditWindow(Location location, bool isEdit)
        {
            InitializeComponent();
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

        public Location InitialLocation;
        private Location CurrentLocation;
        private GeoCoordinate CurrentCoordinate { get { return Location.LocationToGPS(CurrentLocation); } }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            InitialLocation = new Location(CurrentLocation.Name, CurrentLocation.X, CurrentLocation.Y, InitialLocation.Update);
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void UpdateTextBoxes(Location location)
        {
            NameTextBox.Text = location.Name;
            LocationXTextBox.Text = location.X.ToString();
            LocationYTextBox.Text = location.Y.ToString();
            LocationETextBox.Text = Math.Round(CurrentCoordinate.Latitude, 2).ToString();
            LocationNTextBox.Text = Math.Round(CurrentCoordinate.Longitude, 2).ToString();
        }

        private Location ReadLocation()
        {
            if (!int.TryParse(LocationXTextBox.Text, out int X))
            {
                //TODO:Error Massage
                return null;
            }
            if(!int.TryParse(LocationYTextBox.Text,out int Y))
            {
                //TODO:Error Massage
                return null;
            }
            if (NameTextBox.Text.Length < 1)
            {
                //TODO:Error Massage
                return null;
            }
            return Location.SnapToGrid(NameTextBox.Text, X, Y);
        }

        private Location ReadGPS()
        {
            if (!double.TryParse(LocationETextBox.Text, out double E))
            {
                //TODO:Error Massage
                return null;
            }
            if (!double.TryParse(LocationNTextBox.Text, out double N))
            {
                //TODO:Error Massage
                return null;
            }

            return Location.GPSToLocation(new GeoCoordinate(N,E)); 
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
            if (NameTextBox.Text.Length < 1)
            {
                //TODO:Error Massage
            }
            else
            {
                CurrentLocation.Name = NameTextBox.Text;
            }
            
        }

        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

    }
}
