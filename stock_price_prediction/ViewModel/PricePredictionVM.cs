using CsvHelper;
using CsvHelper.Configuration;
using stock_price_prediction.Models;
using stock_price_prediction.Utilities;
using stock_price_prediction.View;
using System.Diagnostics;
using System.Globalization;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows;

namespace stock_price_prediction.ViewModel
{
    class PricePredictionVM : ViewModelBase
    {
        #region Fields and Properties

        private CompanyData _selectedCompany;
        private List<CompanyData> _companies;
        private string _selectedForecastOption;
        private int _forecastDays;
        private DateTime _startDate;
        private DateTime _endDate;
        private BitmapImage _imagePeriodPrice;
        private BitmapImage _imageInfoTrain;
        private BitmapImage _imageForecastPrice;
        
        public List<DateTime> StartDateOptions { get; set; }
        public List<DateTime> EndDateOptions { get; set; }

        public List<CompanyData> Companies
        {
            get => _companies;
            set
            {
                if (_companies != value)
                {
                    _companies = value;
                    OnPropertyChanged(nameof(Companies));
                }
            }
        }
        private List<StockPredictionResult> _stockPredictionResult;
        public List<StockPredictionResult> StockPredictionResult
        {
            get => _stockPredictionResult;
            set
            {
                if (_stockPredictionResult != value)
                {
                    _stockPredictionResult = value;
                    OnPropertyChanged(nameof(StockPredictionResult));
                }
            }
        }

        private StockPredictionResult _selectedStockPredictionResult;
        public StockPredictionResult SelectedStockPredictionResult
        {
            get => _selectedStockPredictionResult;
            set
            {
                if (_selectedStockPredictionResult != value)
                {
                    _selectedStockPredictionResult = value;
                    OnPropertyChanged(nameof(SelectedStockPredictionResult));
                    // Додаткові дії при зміні вибраного елемента
                    HandleSelectionChanged(_selectedStockPredictionResult);
                    UpdatePredictionResults(_selectedStockPredictionResult);
                }
            }
        }
        private string _minDate;
        public string MinDate
        {
            get => _minDate;
            set
            {
                if (_minDate != value)
                {
                    _minDate = value;
                    OnPropertyChanged(nameof(MinDate));
                }
            }
        }

        private string _maxDate;
        public string MaxDate
        {
            get => _maxDate;
            set
            {
                if (_maxDate != value)
                {
                    _maxDate = value;
                    OnPropertyChanged(nameof(MaxDate));
                }
            }
        }

        private string _rmse;
        public string RMSE
        {
            get => _rmse;
            set
            {
                if (_rmse != value)
                {
                    _rmse = value;
                    OnPropertyChanged(nameof(RMSE));
                }
            }
        }
        private async void HandleSelectionChanged(StockPredictionResult selectedResult)
        {
            if (selectedResult != null)
            {
                string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
                string imagePath = Path.Combine(projectRoot, "TrainedModels", "Images", $"{selectedResult.ModelFilename}.png");
                ImageInfoTrain = await LoadImageAsync(imagePath);
            }
        }
        public CompanyData SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                if (_selectedCompany != value)
                {
                    _selectedCompany = value;
                    OnPropertyChanged(nameof(SelectedCompany));
                    UpdateDates();
                    StockPredictionResult = ReadCsvToStockPredictionResults();
                    UpdatePredictionResults(_selectedStockPredictionResult);
                }
            }
        }

        public string SelectedForecastOption
        {
            get => _selectedForecastOption;
            set
            {
                if (_selectedForecastOption != value)
                {
                    _selectedForecastOption = value;
                    OnPropertyChanged(nameof(SelectedForecastOption));
                    UpdateForecastDays();
                }
            }
        }

        public int ForecastDays
        {
            get => _forecastDays;
            set
            {
                if (_forecastDays != value)
                {
                    _forecastDays = value;
                    OnPropertyChanged(nameof(ForecastDays));
                }
            }
        }

        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (_startDate != value)
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                }
            }
        }

        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
            }
        }

        public BitmapImage ImagePeriodPrice
        {
            get => _imagePeriodPrice;
            set
            {
                if (_imagePeriodPrice != value)
                {
                    _imagePeriodPrice = value;
                    OnPropertyChanged(nameof(ImagePeriodPrice));
                }
            }
        }

        public BitmapImage ImageInfoTrain
        {
            get => _imageInfoTrain;
            set
            {
                if (_imageInfoTrain != value)
                {
                    _imageInfoTrain = value;
                    OnPropertyChanged(nameof(ImageInfoTrain));
                }
            }
        }
        public BitmapImage ImageForecastPrice
        {
            get => _imageForecastPrice;
            set
            {
                if (_imageForecastPrice != value)
                {
                    _imageForecastPrice = value;
                    OnPropertyChanged(nameof(ImageForecastPrice));
                }
            }
        }
        public List<string> ForecastOptions { get; set; }

        #endregion

        #region Commands

        public ICommand CreatePeriodGraphCommand { get; set; }
        public ICommand ShowPeriodGraphCommand { get; set; }
        public ICommand InfoTrainCommand { get; set; }
        public ICommand ForecastCommand { get; set; }
        public ICommand ForecastShowImageCommand { get; set; }
        public ICommand TrainCommand
        {
            get; set;
        }
        #endregion

            #region Constructor

        public PricePredictionVM()
        {
            CreatePeriodGraphCommand = new RelayCommand(async (obj) => await CreatePeriodGraphScriptAsync(obj));
            ShowPeriodGraphCommand = new RelayCommand(async (obj) => await ShowPeriodGraphScriptAsync(obj));
            TrainCommand = new RelayCommand(async (obj) => await TrainModelScriptAsync(obj));
            InfoTrainCommand = new RelayCommand(async (obj) => await InfoTrainOnClick(obj));
            ForecastShowImageCommand = new RelayCommand(async (obj) => await ForecastShowOnClickOnClick(obj));
            ForecastCommand = new RelayCommand(async (obj) => await ForecastScriptAsync(obj));
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string filePath = Path.Combine(projectRoot, "DataSets", "company_date_ranges.csv");
            Companies = LoadCompanyData(filePath);
            _loadingText = InfoTrainModel;
            ForecastOptions = new List<string> { "1 Day", "1 Week", "1 Month", "1 Year" };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads company data from a CSV file.
        /// </summary>
        public static List<CompanyData> LoadCompanyData(string filePath)
        {
            var companies = new List<CompanyData>();

            try
            {
                if (!File.Exists(filePath))
                {
                    Console.WriteLine("Error: File not found.");
                    return companies;
                }

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ","
                };

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    companies = csv.GetRecords<CompanyData>().ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return companies;
        }

        /// <summary>
        /// Updates the start and end date options based on the selected company.
        /// </summary>
        private void UpdateDates()
        {
            if (SelectedCompany != null)
            {
                StartDate = SelectedCompany.MinDate;
                EndDate = SelectedCompany.MaxDate;
                UpdateDateOptions();
            }
        }

        /// <summary>
        /// Sets the date options for start and end date dropdowns.
        /// </summary>
        private void UpdateDateOptions()
        {
            if (SelectedCompany == null || SelectedCompany.MinDate > SelectedCompany.MaxDate)
            {
                StartDateOptions = new List<DateTime>();
                EndDateOptions = new List<DateTime>();
                return;
            }

            StartDateOptions = GenerateDateOptions(SelectedCompany.MinDate, SelectedCompany.MaxDate);
            EndDateOptions = new List<DateTime>(StartDateOptions);

            OnPropertyChanged(nameof(StartDateOptions));
            OnPropertyChanged(nameof(EndDateOptions));
        }

        /// <summary>
        /// Generates a list of date options from a given min date to max date.
        /// </summary>
        private List<DateTime> GenerateDateOptions(DateTime minDate, DateTime maxDate)
        {
            var options = new List<DateTime> { minDate };

            for (int i = 1; i < maxDate.Year - minDate.Year; i++)
            {
                options.Add(new DateTime(minDate.Year + i, 1, 1));
            }

            options.Add(new DateTime(maxDate.Year, 1, 1));
            if (maxDate.Date != new DateTime(maxDate.Year, 1, 1))
            {
                options.Add(maxDate);
            }

            return options;
        }

        /// <summary>
        /// Updates forecast days based on the selected forecast option.
        /// </summary>
        private void UpdateForecastDays()
        {
            ForecastDays = SelectedForecastOption switch
            {
                "1 Day" => 1,
                "1 Week" => 7,
                "1 Month" => 30,
                "1 Year" => 365,
                _ => 1
            };
        }

        #endregion

        #region Script Execution Methods

        /// <summary>
        /// Runs the Python script to create and save a graph.
        /// </summary>
        private async Task CreatePeriodGraphScriptAsync(object parameter)
        {
            await ExecutePythonScriptAsync("false");
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string imagePath = Path.Combine(projectRoot, "scripts", "images", "period_price.png");
            string imagePath2 = Path.Combine(projectRoot, "scripts", "images", "period_price2.png");

            string selectedImagePath = File.Exists(imagePath2) ? imagePath2 : imagePath;
            ImagePeriodPrice = await LoadImageAsync(selectedImagePath );
        }

        /// <summary>
        /// Runs the Python script to display the graph without saving.
        /// </summary>
        private async Task ShowPeriodGraphScriptAsync(object parameter)
        {
            await ExecutePythonScriptAsync("true");
        }

        /// <summary>
        /// Executes the Python script with specified show/save option.
        /// </summary>
        private async Task ExecutePythonScriptAsync(string isShow)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string pythonScriptPath = Path.Combine(projectRoot, "scripts", "scriptShowClose.py");

            if (SelectedCompany == null || StartDate == DateTime.MinValue || EndDate == DateTime.MinValue)
            {
                Console.WriteLine("Insufficient data for prediction.");
                return;
            }

            string arguments = $"{SelectedCompany.Ticker} {StartDate:yyyy-MM-dd} {EndDate:yyyy-MM-dd} {isShow}";

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"{pythonScriptPath} {arguments}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string result = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    Console.WriteLine(result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing script: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the generated image.
        /// </summary>
        private async Task<BitmapImage> LoadImageAsync(string path)
        {
            BitmapImage image = null;
            if (File.Exists(path))
            {
                await Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        image = new BitmapImage();
                        image.BeginInit();
                        image.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.EndInit();
                    });
                });
            }
            return image;
        }


        #endregion


        private string _tbNameModel = string.Empty;
        public string tbNameModel
        {
            get => _tbNameModel;
            set
            {
                if (_tbNameModel != value)
                {
                    _tbNameModel = value;
                    OnPropertyChanged();
                }
            }
        }

        
      
        private async Task TrainModelScriptAsync(object parameter)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string pythonScriptPath = Path.Combine(projectRoot, "scripts", "trainModel.py");

            if (SelectedCompany == null || StartDate == DateTime.MinValue || EndDate == DateTime.MinValue)
            {
                Console.WriteLine("Insufficient data for prediction.");
                return;
            }

            string arguments = $"{SelectedCompany.Ticker} {StartDate:yyyy-MM-dd} {EndDate:yyyy-MM-dd} {tbNameModel}.h5";

            try
            {
                var animationTask = AnimateLoadingDotsAsync();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "python",  // або вкажіть повний шлях до python, якщо потрібно
                    Arguments = $"{pythonScriptPath} {arguments}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,  // додано для читання помилок
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // Читання стандартного виведення
                    string result = await process.StandardOutput.ReadToEndAsync();

                    // Читання стандартних помилок, якщо є
                    string errorResult = await process.StandardError.ReadToEndAsync();
                    if (!string.IsNullOrEmpty(errorResult))
                    {
                        Console.WriteLine($"Error: {errorResult}");
                    }

                    await process.WaitForExitAsync();
                    Console.WriteLine(result);
                }
                StockPredictionResult = ReadCsvToStockPredictionResults();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing script: {ex.Message}");
            }
            finally
            {
                
                string imagePath = Path.Combine(projectRoot, "TrainedModels", "Images", $"{_tbNameModel}.h5.png");
                _isTrainingInProgress = false;  // Зупиняє анімацію крапок після завершення
                tbNameModel = "";
                await Task.Delay(500);  // Затримка перед встановленням повідомлення "Training complete"
            }
        }
        private string InfoTrainModel = "Info: ";
        private string _loadingText ;
        private bool _isTrainingInProgress;
        public string LoadingText
        {
            get => _loadingText;
            set
            {
                _loadingText = value;
                OnPropertyChanged();
            }
        }
        private async Task AnimateLoadingDotsAsync()
        {
            int dotCount = 0;
            _isTrainingInProgress = true;

            while (_isTrainingInProgress)
            {
                dotCount = (dotCount % 3) + 1;  // циклічно змінює кількість крапок від 1 до 3
                LoadingText = InfoTrainModel+ $"Training model{new string('.', dotCount)}";
                await Task.Delay(500);  // Затримка між оновленнями тексту
            }

            LoadingText =InfoTrainModel+ "Training complete";  // Повідомлення після завершення
        }



        private List<StockPredictionResult> ReadCsvToStockPredictionResults()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string filePath = Path.Combine(projectRoot, "DataSets", "prediction_results.csv");
            var results = new List<StockPredictionResult>();

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecords<StockPredictionResult>().ToList();
                    results.AddRange(records);
                }

                // Сортуємо результати за вибраною компанією
                results = results.Where(r => r.Company==_selectedCompany.Ticker).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading CSV: {ex.Message}");
            }
           
            return results;
        }
 
        private async Task ForecastShowOnClickOnClick(object parameter)
        {
            if (ImageForecastPrice == null || ImageForecastPrice.UriSource == null)
            {
                Console.WriteLine("Image source is not set.");
                return;
            }

            string imagePath = ImageForecastPrice.UriSource.LocalPath;

            if (File.Exists(imagePath))
            {
                try
                {
                    await Task.Run(() =>
                    {
                        var processStartInfo = new ProcessStartInfo
                        {
                            FileName = imagePath,
                            UseShellExecute = true  // Відкриває файл у програмі за замовчуванням
                        };
                        Process.Start(processStartInfo);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error opening image: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Image file not found.");
            }
        }
        private async Task InfoTrainOnClick(object parameter)
        {
            if (ImageInfoTrain == null || ImageInfoTrain.UriSource == null)
            {
                Console.WriteLine("Image source is not set.");
                return;
            }

            string imagePath = ImageInfoTrain.UriSource.LocalPath;

            if (File.Exists(imagePath))
            {
                try
                {
                    await Task.Run(() =>
                    {
                        var processStartInfo = new ProcessStartInfo
                        {
                            FileName = imagePath,
                            UseShellExecute = true  // Відкриває файл у програмі за замовчуванням
                        };
                        Process.Start(processStartInfo);
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error opening image: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Image file not found.");
            }
        }

        public void UpdatePredictionResults(StockPredictionResult selectedResult)
        {
            if (selectedResult != null)
            {
                MinDate = "Start: " +selectedResult.MinDate.ToString();
                MaxDate = "End: "+selectedResult.MaxDate.ToString();
                RMSE = "RMSE: "+ selectedResult.RMSE.ToString();
            }
            else
            {
                MinDate = "Start: ";
                MaxDate = "End: ";
                RMSE = "RMSE: ";
            }
        }

        private async Task ForecastScriptAsync(object parameter)
        {
            string projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\.."));
            string scriptPath = Path.Combine(projectRoot, "scripts", "priceForecast.py");

            // Аргументи для скрипта
            string ticker = $"{SelectedStockPredictionResult.Company}"; // Назва компанії (тикер)
            string startDate = $"{SelectedStockPredictionResult.MinDate}";
            string endDate = $"{SelectedStockPredictionResult.MaxDate}";

            // Парсимо та конвертуємо дати
            DateTime parsedStartDate = DateTime.ParseExact(startDate, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
            string convertedStartDate = parsedStartDate.ToString("yyyy-MM-dd");

            DateTime parsedEndDate = DateTime.ParseExact(endDate, "M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
            string convertedEndDate = parsedEndDate.ToString("yyyy-MM-dd");

            int daysAhead = ForecastDays; // Кількість днів для прогнозу
            string modelFilename = Path.Combine(projectRoot, "TrainedModels", $"{SelectedStockPredictionResult.ModelFilename}"); // Назва файлу моделі

            // Формування аргументів для скрипта
            string arguments = $"\"{scriptPath}\" {ticker} {convertedStartDate} {convertedEndDate} {daysAhead} \"{modelFilename}\"";

            // Налаштування процесу для запуску Python
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                // Запускаємо процес
                using (Process process = Process.Start(psi))
                {
                    // Асинхронно читаємо потоки виводу та помилок
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();

                    await process.WaitForExitAsync();

                    // Логіка обробки результату
                    if (!string.IsNullOrEmpty(output))
                    {
                        Console.WriteLine("Output:");
                        Console.WriteLine(output);
                    }

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine("Error:");
                        Console.WriteLine(error);

                        // Додатково можна зберегти помилки у файл або показати в UI
                        string errorLogPath = Path.Combine(projectRoot, "error_log.txt");
                        await File.WriteAllTextAsync(errorLogPath, error);
                        Console.WriteLine($"Python errors saved to: {errorLogPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while running the script:");
                Console.WriteLine(ex.Message);
            }
            scriptPath = Path.Combine(projectRoot, "TrainedModels", "ImagesForecast", "ForecastPrice.png");
            string scriptPath2 = Path.Combine(projectRoot, "TrainedModels", "ImagesForecast", "ForecastPrice2.png");
            if (File.Exists(scriptPath))
            {
                // Якщо файли існують, викликаємо LoadImageAsync
                ImageForecastPrice = await LoadImageAsync(scriptPath);
                
            }
            else if (File.Exists(scriptPath2))
            {
                ImageForecastPrice = await LoadImageAsync(scriptPath2);
            }
            
        }

    }

}
