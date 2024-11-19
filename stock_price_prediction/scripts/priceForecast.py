import yfinance as yf
import numpy as np
import pandas as pd
from sklearn.preprocessing import MinMaxScaler
from keras.models import load_model
import matplotlib.pyplot as plt
import sys
import os
from math import ceil

script_directory = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_directory)
sys.stdout.reconfigure(encoding='utf-8')
def load_and_predict(ticker, start_date, end_date, days_ahead=365, model_filename="lstm_model.h5"):
    # Завантаження історичних даних про акції з Yahoo Finance
    df = yf.download(ticker, start=start_date, end=end_date)
    
    df_new = df[['Close']]
    dataset = df_new.values
    shape = df.shape[0]
    train = df_new[:ceil(shape * 0.75)]
    valid = df_new[ceil(shape * 0.75):]

    # Масштабування даних
    scaler = MinMaxScaler(feature_range=(0, 1))
    scaled_data = scaler.fit_transform(dataset)

    # Підготовка тестового набору
    inputs = df_new[len(df_new) - len(valid) - 40:].values
    inputs = inputs.reshape(-1, 1)
    inputs = scaler.transform(inputs)

    X_test = []
    for i in range(40, inputs.shape[0]):
        X_test.append(inputs[i-40:i, 0])
    X_test = np.array(X_test)
    X_test = np.reshape(X_test, (X_test.shape[0], X_test.shape[1], 1))

    # Завантаження збереженої моделі
    model = load_model(model_filename)
    closing_price = model.predict(X_test)
    closing_price = scaler.inverse_transform(closing_price)

    # Прогноз на задану кількість днів
    last_40_days = inputs[-40:]  # Останні 40 днів для старту прогнозу
    predictions = []

    for _ in range(days_ahead):
        next_prediction = model.predict(last_40_days.reshape(1, -1, 1))
        predictions.append(scaler.inverse_transform(next_prediction)[0][0])

        # Оновлюємо вхідні дані для наступного прогнозу
        last_40_days = np.append(last_40_days, next_prediction)[1:].reshape(-1, 1)

    # Виведення останнього спрогнозованого значення
    print(f'Last predicted value: {predictions[-1]}')

    # Підготовка даних для відображення
    valid['Predictions'] = closing_price
    future_dates = pd.date_range(df.index[-1] + pd.Timedelta(days=1), periods=days_ahead)
    future_df = pd.DataFrame(predictions, index=future_dates, columns=['Predictions'])

    # Побудова графіка з налаштуванням стилю
    plt.figure(figsize=(12, 6), facecolor='#001f3f')  # Темносиній фон для всієї області

# Додаємо темносиній фон для області графіка
    ax = plt.gca()
    ax.set_facecolor('#001f3f')  # Темносиній фон для самого графіка

# Побудова графіків
    plt.plot(train['Close'], label='Training Data', color='white')
    plt.plot(valid['Close'], label='Actual Data', color='#00FF00')  # Зелений для фактичних даних
    plt.plot(valid['Predictions'], label='Predicted Data', color='#FF5733')  # Помаранчевий для передбачених
    plt.plot(future_df['Predictions'], label=f'{days_ahead}-Day Forecast', linestyle='--', color='#00FFFF')  # Блакитний для прогнозу

# Налаштування підписів
    plt.title(f"Stock Price Prediction for {ticker} by LSTM", color='white', fontsize=20)
    plt.xlabel('Date', color='white', fontsize=14)
    plt.ylabel('Stock Price', color='white', fontsize=14)

# Додавання легенди з темносинім фоном
    plt.legend(facecolor='#001f3f', edgecolor='white', labelcolor='white', fontsize=12)

# Додавання сітки
    plt.grid(True, color='gray')

# Налаштування міток осей
    ax.tick_params(colors='white')  # Колір тексту міток на осях
    
    file_name1 = "ForecastPrice.png"
    file_name2 = "ForecastPrice2.png"
    file_path1 = os.path.join(os.path.dirname(os.getcwd()), "TrainedModels", "ImagesForecast", file_name1)
    file_path2= os.path.join(os.path.dirname(os.getcwd()), "TrainedModels", "ImagesForecast", file_name2)
    if os.path.exists(file_path1):
        os.remove(file_path1)
        print(f"Файл {file_name1} видалено.")
        plt.savefig(file_path2)   
        print(f"Збережено новий файл {file_name2}.")

    elif os.path.exists(file_path2):
        os.remove(file_path2)
        print(f"Файл {file_name2} видалено.")
        plt.savefig(file_path1)   
        print(f"Збережено новий файл {file_name1}.")

    else:
        plt.savefig(file_path1)
        print(f"Збережено новий файл {file_name1}.")
     
    #plt.show()

# Виклик функції для завантаження моделі та прогнозу, використовуючи sys.argv
if len(sys.argv) != 6:
    print("Usage: python script.py <ticker> <start_date> <end_date> <days_ahead> <model_filename>")
    sys.exit(1)

ticker = sys.argv[1]  # Назва компанії (тикер)
start_date = sys.argv[2]  # Початкова дата
end_date = sys.argv[3]  # Кінцева дата
days_ahead = int(sys.argv[4])  # Кількість днів для прогнозу
model_filename = sys.argv[5]  # Назва файлу моделі

load_and_predict(ticker, start_date, end_date, days_ahead, model_filename)
