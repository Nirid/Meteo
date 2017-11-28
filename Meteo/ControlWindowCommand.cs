using System;
using System.Windows;
using System.Windows.Input;
using Hardcodet.Wpf.TaskbarNotification;

namespace Meteo
{
    public class ControlWindowCommand : ICommand
    {

        public void Execute(object parameter)
        {
            if(parameter is Window win)
            {
                if (win.Visibility == Visibility.Visible)
                {
                    win.Visibility = Visibility.Collapsed;
                }else
                {
                    win.Visibility = Visibility.Visible;
                    win.Activate();
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
