using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;

namespace Meteo.TaskbarIcon
{
    class Hide : ICommand
    {
        public void Execute(object parameter)
        {
            if (parameter is Window win)
            {
                win.Visibility = Visibility.Hidden;
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
