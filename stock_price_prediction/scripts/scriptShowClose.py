import sys
import os
import yfinance as yf
import matplotlib.pyplot as plt
from datetime import datetime
import pandas as pd  # Додаємо імпорт бібліотеки pandas

# Перевірка параметрів
if len(sys.argv) != 5:
    print("Будь ласка, передайте всі необхідні параметри: ticker, start_date, end_date, isShow.")
    sys.exit(1)

# Зчитуємо параметри з командного рядка
ticker = sys.argv[1]
start_date = sys.argv[2]
end_date = sys.argv[3]
is_show = sys.argv[4].lower() == 'true'  # Перевіряємо, чи значення 'true' (регістр не має значення)

script_directory = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_directory)

# Перевірка на валідність дат
try:
    start_date = datetime.strptime(start_date, "%Y-%m-%d")
    end_date = datetime.strptime(end_date, "%Y-%m-%d")
except ValueError:
    print("Невірний формат дати. Використовуйте формат yyyy-mm-dd.")
    sys.exit(1)

# Завантажуємо дані
data = yf.download(ticker, start=start_date, end=end_date)

# Перевірка на порожні дані
if data.empty:
    print(f"Не вдалося завантажити дані для {ticker} у вказаний період.")
    sys.exit(1)

# Перетворення індексу на datetime, якщо це потрібно
data.index = pd.to_datetime(data.index)

# Побудова графіка ціни "Close" у часі
plt.figure(figsize=(12, 6), facecolor='#001f3f')  # Темносиній фон

# Додаємо темносиній фон для області графіка
ax = plt.gca()
ax.set_facecolor('#001f3f')  # Темносиній фон для самого графіка

plt.plot(data.index, data['Close'], label='Close Price', color='white')  # Білий графік

# Додавання підписів
plt.title(f"Ціна закриття акцій {ticker}", color='white')
plt.xlabel("Дата", color='white')
plt.ylabel("Ціна закриття (USD)", color='white')
plt.legend(facecolor='#001f3f', edgecolor='white', labelcolor='white')
plt.grid(True, color='gray')

# Змінюємо колір міток осей на білий
ax.tick_params(colors='white')

# Якщо isShow == false, ми виконуємо перевірку на існування файлів і зберігаємо їх
if not is_show:
    parent_directory = os.path.dirname(script_directory)
    file_name1 = "period_price.png"
    file_name2 = "period_price2.png"

    # Шлях до файлів
    file_path1 = os.path.join("images", file_name1)
    file_path2 = os.path.join("images", file_name2)
    # Перевіряємо, чи існують файли і змінюємо їх відповідно до умов
    if os.path.exists(file_path1):
        os.remove(file_path1)
        print(f"Файл {file_name1} видалено.")
        plt.savefig(file_path2)  # Зберігаємо як period_price2.png
        print(f"Збережено новий файл {file_name2}.")

    elif os.path.exists(file_path2):
        os.remove(file_path2)
        print(f"Файл {file_name2} видалено.")
        plt.savefig(file_path1)  # Зберігаємо як period_price.png
        print(f"Збережено новий файл {file_name1}.")

    else:
        plt.savefig(file_path1)
        print(f"Збережено новий файл {file_name1}.")

# Якщо isShow = true, показуємо графік
if is_show:
    plt.show()
