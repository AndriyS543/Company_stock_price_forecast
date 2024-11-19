import yfinance as yf
import pandas as pd
import os

script_directory = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_directory)

def update_csv_if_new_data():
    # List of companies and their tickers
    companies = {
        "Apple": "AAPL",
        "Microsoft": "MSFT",
        "Alphabet Class A (Google)": "GOOGL",
        "Alphabet Class C (Google)": "GOOG",
        "Amazon": "AMZN",
        "Meta (Facebook)": "META",
        "Tesla": "TSLA",
        "NVIDIA": "NVDA",
        "Intel": "INTC",
        "AMD": "AMD",
        "JPMorgan Chase": "JPM",
        "Bank of America": "BAC",
        "Wells Fargo": "WFC",
        "Goldman Sachs": "GS",
        "Citigroup": "C",
        "Coca-Cola": "KO",
        "PepsiCo": "PEP",
        "Procter & Gamble": "PG",
        "Johnson & Johnson": "JNJ",
        "Walmart": "WMT",
        "McDonald's": "MCD",
        "Nike": "NKE",
        "ExxonMobil": "XOM",
        "Chevron": "CVX",
        "ConocoPhillips": "COP",
        "Ford": "F",
        "General Motors": "GM",
        "Visa": "V",
        "Mastercard": "MA",
        "Berkshire Hathaway Class A": "BRK-A",
        "Berkshire Hathaway Class B": "BRK-B",
        "Pfizer": "PFE",
        "AbbVie": "ABBV"
    }

    # Initialize list to store data
    data = []
    parent_directory = os.path.dirname(script_directory)
    csv_file = os.path.join(parent_directory, "DataSets", "company_date_ranges.csv")

    # If the CSV file exists, delete it
    if os.path.exists(csv_file):
        os.remove(csv_file)
        print(f"Existing file '{csv_file}' deleted.")

    # Fetch date range for each ticker
    for company, ticker in companies.items():
        stock = yf.Ticker(ticker)
        hist = stock.history(period="max")

        if not hist.empty:
            min_date = hist.index.min().strftime('%Y-%m-%d')
            max_date = hist.index.max().strftime('%Y-%m-%d')
        else:
            min_date = max_date = "No Data"

        data.append([company, ticker, min_date, max_date])

    # Create DataFrame from new data and save it to CSV
    df = pd.DataFrame(data, columns=["Company", "Ticker", "MinDate", "MaxDate"])
    df.to_csv(csv_file, index=False, encoding='utf-8')
    print(f"New CSV file '{csv_file}' has been created with updated data.")

# Run the function
update_csv_if_new_data()
