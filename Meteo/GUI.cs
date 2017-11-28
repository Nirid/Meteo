using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                if (!XManager.AllLocations.Contains(location))
                {
                    XManager.AddLocation(location);
                    Locations = new ObservableCollection<Location>(XManager.AllLocations.ToList());
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

    }
}
