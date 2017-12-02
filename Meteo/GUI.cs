using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Meteo
{
    partial class MainWindow
    {
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ForceUpdate();
        }

        private void ManualMode_Checked(object sender, RoutedEventArgs e)
        {
            YTextbox.IsEnabled = true;
            XTextbox.IsEnabled = true;
            SaveNewLocalizationButton.IsEnabled = true;
            SetLocalizationButton.IsEnabled = true;
            NewLocalizationNameTextbox.IsEnabled = true;
            UpdateTextbox();
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
            if (ManualMode.IsChecked == true)
            {
                XTextbox.Text = SelectedLocation.X.ToString();
                YTextbox.Text = SelectedLocation.Y.ToString();
            }
        }

        private void SetLocalizationButton_Click(object sender, RoutedEventArgs e)
        {
            if (ManualMode.IsChecked.Value)
            {
                if (int.TryParse(YTextbox.Text, out var Y) && int.TryParse(XTextbox.Text, out var X))
                {
                    var loc = Location.SnapToGrid(X, Y);
                    SelectedLocation = new Location(loc.X, loc.Y);
                    SelectedLocationChanged();
                }
            }
        }

        private void SetDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            XManager.SetLastLocation((Location)CityList.SelectedItem);
            SetDefaultButton.IsEnabled = false;
        }

        private void SaveNewLocalizationButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NewLocalizationNameTextbox.Text;
            if (int.TryParse(YTextbox.Text, out var Y) && int.TryParse(XTextbox.Text, out var X))
            {
                var loc = Location.SnapToGrid(X, Y);
                var location = new Location(name, loc.X, loc.Y);
                if(!AddLocation(location))
                {
                    MessageBox.Show("Lokalizacja o identycznej nazwie lub położeniu już istnieje", "Błąd");
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
            var newLocation = new Location(replaced.Name, replaced.X, replaced.Y, true);
            Locations[index] = newLocation;
            CityList.SelectedIndex = Locations.IndexOf(loc);
            XManager.ReplaceLocation(replaced, newLocation);
        }

        private void AutoUpdateCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            var loc = (Location)CityList.SelectedItem;
            if (loc.Update == false)
                return;
            int index = Locations.IndexOf(loc);
            var replaced = Locations[index];
            var newLocation = new Location(replaced.Name, replaced.X, replaced.Y, false);
            Locations[index] = newLocation;
            CityList.SelectedIndex = Locations.IndexOf(loc);
            XManager.ReplaceLocation(replaced, newLocation);
        }

        private void CityList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CityList.SelectedIndex < 0 || CityList.SelectedIndex >= CityList.Items.Count)
            {
                return;
            }
            SelectedLocation = (Location)CityList.SelectedItem;
            if (XManager.LastLocation == SelectedLocation)
                SetDefaultButton.IsEnabled = false;
            else
                SetDefaultButton.IsEnabled = true;
            AutoUpdateCheckbox.IsChecked = SelectedLocation.Update;

            SelectedLocationChanged();
        }

        private void RemoveLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLocation != XManager.LastLocation)
            {
                MessageBoxResult message = MessageBox.Show($"Jesteś pewny, że chcesz usunąć {SelectedLocation.ToString()}?", "Potwierdź", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (message == MessageBoxResult.Yes)
                {
                    RemoveLocation(SelectedLocation);
                }
            }
            else
            {
                MessageBox.Show("Nie można usnąć domyślnej lokalizacji, zmień domyślną lokalizację a następnie spróbuj usnąć lokalizację ponownie", "Błąd");
            }
        }

        private void LocatonEditButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditWindow(SelectedLocation, true);
            var result = editWindow.ShowDialog();
            var location = editWindow.InitialLocation;
            if (result == true)
            {
                if(!ReplaceLocation(SelectedLocation, location))
                {
                    //TODO: Error massage
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //TODO: Change this check
            if (this.FontWeight == FontWeights.UltraBold)
            {
                Notifycation.Dispose();
                return;
            }
            else
            {
                (this as Window).Visibility = Visibility.Hidden;
                e.Cancel = true;
                return;
            }
        }

    }
}
