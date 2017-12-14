using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Meteo
{
    partial class MainWindow
    {
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ForceUpdate();
        }

        private void SetDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            XManager.SetLastLocation((Location)CityList.SelectedItem);
            SetDefaultButton.IsEnabled = false;
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
            var editWindow = new EditWindow(SelectedLocation,XManager.AllLocations, true);
            var result = editWindow.ShowDialog();
            var location = editWindow.InitialLocation;
            if (result == true)
            {
                if(!ReplaceLocation(SelectedLocation, location))
                {
                    MessageBox.Show("Nie można zamienić domyślnej lokalizacji, zmień domyślną lokalizację a następnie zedytuj ponownie", "Błąd");
                }
            }
        }

        private void CreateLocationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CreateLocationComboBox.SelectedIndex == 1)
            {
                var location = new Location("", XManager.LastLocation.X, XManager.LastLocation.Y, false);
                var editWindow = new EditWindow(location, XManager.AllLocations, false);
                var result = editWindow.ShowDialog();
                var choosenLocation = editWindow.InitialLocation;
                if (result == true)
                {
                    if (!AddLocation(choosenLocation))
                    {
                        MessageBox.Show("Nie można zamienić domyślnej lokalizacji, zmień domyślną lokalizację a następnie zedytuj ponownie", "Błąd");
                        return;
                    }
                }
                CreateLocationComboBox.SelectedIndex = -1;
                CityList.SelectedItem = choosenLocation;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            MapGenerator.DisplayLocation(SelectedLocation);
            e.Handled = true;
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
