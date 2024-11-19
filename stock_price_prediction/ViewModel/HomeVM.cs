using stock_price_prediction.Utilities;
using stock_price_prediction.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace stock_price_prediction.ViewModel
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Threading;

    class HomeVM : ViewModelBase
    {
        private string _loadingText;
        private DispatcherTimer _ellipsisTimer;
        private int _ellipsisState;

        public string LoadingText
        {
            get => _loadingText;
            set
            {
                _loadingText = value;
                OnPropertyChanged(nameof(LoadingText));
            }
        }

        private string _infoText;

        public string InfoText
        {
            get { return _infoText; }
            set
            {
                _infoText = value;
                OnPropertyChanged(nameof(InfoText));
            }
        }

        public ICommand DownloadCommand { get; set; }

        public HomeVM()
        {
            InfoText = "YFinance — це Python бібліотека, що дозволяє отримувати фінансові дані з Yahoo Finance, який є популярним джерелом для інформації про акції, фондові індекси, валютні пари, криптовалюти та інші фінансові інструменти. У даному випадку бібліотека буде використовуватись для аналізу та прогнозування цін на акції, забезпечуючи доступ до історичних цін, фінансових звітів та інших важливих економічних показників.";
            DownloadCommand = new RelayCommand(async _ => await UpdateCsvFileIfNewDataAsync());

            // Initialize timer for ellipsis animation
            _ellipsisTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // Update every half second
            };
            _ellipsisTimer.Tick += EllipsisAnimationTick;
            LoadingText = "Download/Update";
        }

        private async Task UpdateCsvFileIfNewDataAsync()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string pythonScriptPath = Path.Combine(projectRoot, "scripts", "update_csv_if_new_data.py");

            // Start ellipsis animation
            _ellipsisState = 0;
            _ellipsisTimer.Start();

            try
            {
                await Task.Run(() =>
                {
                    // Run the Python script to update CSV if there's new data
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "python",
                        Arguments = pythonScriptPath,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process process = Process.Start(startInfo))
                    {
                        if (process != null)
                        {
                            using (StreamReader reader = process.StandardOutput)
                            {
                                string result = reader.ReadToEnd();
                                process.WaitForExit();
                                Console.WriteLine(result); // Output result to console
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating CSV: {ex.Message}");
            }
            finally
            {
                // Stop ellipsis animation
                _ellipsisTimer.Stop();
                LoadingText = "Download/Update"; // Reset text after completion
            }
        }

        private void EllipsisAnimationTick(object sender, EventArgs e)
        {
            // Cycle through ellipsis states
            _ellipsisState = (_ellipsisState + 1) % 4;
            LoadingText =  new string('.', _ellipsisState);
        }
    }

}
