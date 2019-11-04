using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RipExample.Wpf.Core
{
    public class Observable : INotifyPropertyChanged
    {
        public void Set<T>(ref T property, T value, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (value.Equals(property) == false)
            {
                property = value;
                OnPropertyChanged(propertyName);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
