using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Meteo.TaskbarIcon
{
    class Close : ICommand
    {
        public void Execute(object parameter)
        {
            if (parameter is Window win)
            {
                win.FontWeight = FontWeights.UltraBold;
                win.Close();
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
