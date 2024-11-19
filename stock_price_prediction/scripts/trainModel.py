import yfinance as yf
import numpy as np
import pandas as pd
import os
from math import ceil
from sklearn.preprocessing import MinMaxScaler
from keras.models import Sequential
from keras.layers import Dense, LSTM
import matplotlib.pyplot as plt
import sys
import csv

script_directory = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_directory)
sys.stdout.reconfigure(encoding='utf-8')

def lstm_prediction(ticker, start_date, end_date, model_filename="lstm_model.h5", csv_filename="prediction_results.csv"):
    try:
        # Завантаження історичних даних про акції з Yahoo Finance
        print(f"Downloading data for {ticker} from {start_date} to {end_date}...")
        df = yf.download(ticker, start=start_date, end=end_date)
        
        # Перевірка на наявність даних
        if df.empty:
            print(f"No data found for ticker {ticker}.")
            return

        shape = df.shape[0]
        df_new = df[['Close']]
        dataset = df_new.values
        train = df_new[:ceil(shape * 0.75)]
        valid = df_new[ceil(shape * 0.75):]

        print('-----------------------------------------------------------------------------')
        print('-----------STOCK PRICE PREDICTION BY LONG SHORT TERM MEMORY (LSTM)-----------')
        print('-----------------------------------------------------------------------------')
        print('Shape of Training Set', train.shape)
        print('Shape of Validation Set', valid.shape)

        # Масштабування даних
        scaler = MinMaxScaler(feature_range=(0, 1))
        scaled_data = scaler.fit_transform(dataset)

        # Підготовка навчального набору
        x_train, y_train = [], []
        for i in range(40, len(train)):
            x_train.append(scaled_data[i-40:i, 0])
            y_train.append(scaled_data[i, 0])
        x_train, y_train = np.array(x_train), np.array(y_train)
        x_train = np.reshape(x_train, (x_train.shape[0], x_train.shape[1], 1))

        # Створення та навчання моделі LSTM
        model = Sequential()
        model.add(LSTM(units=50, return_sequences=True, input_shape=(x_train.shape[1], 1)))
        model.add(LSTM(units=50))
        model.add(Dense(1))
        model.compile(loss='mean_squared_error', optimizer='adam')
        model.fit(x_train, y_train, epochs=1, batch_size=1, verbose=2)
        filename = os.path.join(os.path.dirname(os.getcwd()), "TrainedModels", f"{model_filename}")
        model.save(filename)
        print(f"Model saved as '{model_filename}'.")

        # Підготовка тестового набору
        inputs = df_new[len(df_new) - len(valid) - 40:].values
        inputs = inputs.reshape(-1, 1)
        inputs = scaler.transform(inputs)

        X_test = []
        for i in range(40, inputs.shape[0]):
            X_test.append(inputs[i-40:i, 0])
        X_test = np.array(X_test)
        X_test = np.reshape(X_test, (X_test.shape[0], X_test.shape[1], 1))

        # Прогнозування на валідаційному наборі
        closing_price = model.predict(X_test)
        closing_price = scaler.inverse_transform(closing_price)

        # RMSE
        rms = np.sqrt(np.mean(np.power((valid['Close'].values - closing_price), 2)))
        print('RMSE value on validation set:', rms)
        print('-----------------------------------------------------------')
        print('-----------------------------------------------------------')

        # Відображення результатів
        valid['Predictions'] = closing_price
       # Налаштування графіку для прогнозу цін акцій із темною темою
        plt.figure(figsize=(12, 6), facecolor='#001f3f')  # Темносиній фон для всього графіка

# Темносиній фон для області графіка
        ax = plt.gca()
        ax.set_facecolor('#001f3f')

# Графіки для навчальних, актуальних та прогнозованих даних
        plt.plot(train['Close'], label='Training Data', color='cyan')        # Блакитний для навчальних даних
        plt.plot(valid['Close'], label='Actual Data', color='white')         # Білий для актуальних даних
        plt.plot(valid['Predictions'], label='Predicted Data', color='lime') # Лайм для прогнозованих даних

# Додавання підписів та форматування тексту
        plt.title(f"Stock Price Prediction for {ticker} by LSTM", color='white', size=20)
        plt.xlabel('Date', color='white', size=20)
        plt.ylabel('Stock Price (USD)', color='white', size=20)

# Налаштування легенди
        plt.legend(facecolor='#001f3f', edgecolor='white', labelcolor='white')
        ax.tick_params(colors='white')
# Налаштування сітки
        plt.grid(True, color='gray')

# Зберігання графіку як зображення
        png_filename = os.path.join(os.path.dirname(os.getcwd()), "TrainedModels", "Images", f"{model_filename}.png")
        plt.savefig(png_filename, facecolor='#001f3f')  # Зберігаємо з темносинім фоном

        
        csv_filename = os.path.join(os.path.dirname(os.getcwd()), "DataSets", "prediction_results.csv")
        # Запис результатів в CSV
        with open(csv_filename, mode='a', newline='', encoding='utf-8-sig') as file:
            writer = csv.writer(file)
            
            # Перевірка на наявність заголовка в CSV файлі
            if file.tell() == 0:
                writer.writerow(["Company", "MinDate", "MaxDate", "RMSE", "ModelFilename"])
            
            writer.writerow([ticker, start_date, end_date, rms, model_filename])

    except Exception as e:
        print(f"Error during stock prediction: {str(e)}", file=sys.stderr)
        
if __name__ == "__main__":
    try:
        # Отримуємо аргументи з командного рядка
        ticker = sys.argv[1]
        start_date = sys.argv[2]
        end_date = sys.argv[3]
        model_filename = sys.argv[4] if len(sys.argv) > 4 else "lstm_model.h5"
        csv_filename = sys.argv[5] if len(sys.argv) > 5 else "prediction_results.csv"

        # Викликаємо функцію з переданими аргументами
        lstm_prediction(ticker, start_date, end_date, model_filename, csv_filename)

    except Exception as e:
        print(f"Error in script execution: {str(e)}", file=sys.stderr)
