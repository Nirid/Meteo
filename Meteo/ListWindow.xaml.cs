using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for ListWindow.xaml
    /// </summary>
    public partial class ListWindow : Window
    {
        public ListWindow(IEnumerable<Location> locations, Location defaultLocation)
        {
            InitializeComponent();
            Locations = new ObservableCollection<Location>(locations);
            LocationsListBox.DataContext = Locations;
            DefaultLocation = defaultLocation;

        }

        public ObservableCollection<Location> Locations;
        private Location DefaultLocation;

        /// <summary>
        /// Moves LocationsListBox.SelectedItem by requested number of places, to move to top use int.MinValue, to bottom int.MaxValue
        /// </summary>
        /// <param name="placesToMove">Number of places to move item by</param>
        private void MoveSelectedWithinList(int placesToMove)
        {
            int selectedIndex = LocationsListBox.SelectedIndex;
            if (selectedIndex == -1)
                return;
            int newIndex = SwapListItems((Location)LocationsListBox.SelectedItem, placesToMove);
            LocationsListBox.SelectedIndex = newIndex > -1 ? newIndex : selectedIndex;
        }

        private int SwapListItems(Location toBeMoved, int placesToMove)
        {
            int locationIndex = Locations.IndexOf(toBeMoved);
            int lastIndex = Locations.Count - 1;
            int targetIndex = locationIndex + placesToMove;
            if (targetIndex < 0)
                targetIndex = 0;
            if (targetIndex > lastIndex)
                targetIndex = lastIndex;
            Locations[locationIndex] = Locations[targetIndex];
            Locations[targetIndex] = toBeMoved;
            return targetIndex;
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedWithinList(-1);
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedWithinList(+1);
        }

        private void MoveTopButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedWithinList(int.MinValue);
        }

        private void MoveBottomButton_Click(object sender, RoutedEventArgs e)
        {
            MoveSelectedWithinList(int.MaxValue);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void DisplayLocationButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddNewLocationButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
