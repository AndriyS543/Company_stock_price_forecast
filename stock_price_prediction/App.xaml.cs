using System.Configuration;
using System.Data;
using System.Windows;
using stock_price_prediction.Utilities;
using stock_price_prediction.ViewModel;
using System.Windows.Input;
 
namespace stock_price_prediction
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
 
        }
        protected async override void OnStartup(StartupEventArgs e)
        {

            var navigationVM = new NavigationVM();

            MainWindow = new MainWindow
            {
                DataContext = navigationVM
            };
            MainWindow.Show();
            base.OnStartup(e);
        }
    }
    
}
