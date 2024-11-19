using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stock_price_prediction.Utilities;

using System.Windows.Input;


namespace stock_price_prediction.ViewModel
{
    class NavigationVM : ViewModelBase
    {

        private object? _currentView;
        public object? CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ICommand HomeCommand { get; set; }
 

        public ICommand PricePredictionCommand { get; set; }

        private void Home(object obj) => CurrentView = new HomeVM();
        private void PricePrediction (object obj) => CurrentView = new PricePredictionVM();


        public NavigationVM()
        {

            HomeCommand = new RelayCommand(Home);
            PricePredictionCommand = new RelayCommand(PricePrediction);
            // Startup Page
            CurrentView = new HomeVM();
        }
    }
}
