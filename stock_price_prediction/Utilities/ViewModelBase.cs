using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace stock_price_prediction.Utilities
{
    // Base class implementing INotifyPropertyChanged for MVVM pattern
    public class ViewModelBase : INotifyPropertyChanged
    {
        // Event triggered when a property value changes
        public event PropertyChangedEventHandler PropertyChanged;

        // Method to notify when a property value changes
        public void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            // Checks if there are any subscribers to the event and raises the event with the property name
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
