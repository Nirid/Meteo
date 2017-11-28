using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meteo
{
    public class CustomObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        protected override void ClearItems()
        {
            foreach (var item in Items) item.PropertyChanged -= ItemPropertyChanged;
            base.ClearItems();
        }

        protected override void InsertItem(int index, T item)
        {
            item.PropertyChanged += ItemPropertyChanged;
            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this[index].PropertyChanged -= ItemPropertyChanged;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, T item)
        {
            this[index].PropertyChanged -= ItemPropertyChanged;
            item.PropertyChanged += ItemPropertyChanged;
            base.SetItem(index, item);
        }

        public event PropertyChangedEventHandler ItemPropertyChanged;

        protected void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ItemPropertyChanged(sender, e);
        }

    }
}
