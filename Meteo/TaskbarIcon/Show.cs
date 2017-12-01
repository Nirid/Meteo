using System;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;

namespace Meteo.TaskbarIcon
{
    public class Show : ICommand
    {

        public void Execute(object parameter)
        {
            if (parameter is Window win)
            {
                win.Visibility = Visibility.Visible;
                win.Activate();
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
