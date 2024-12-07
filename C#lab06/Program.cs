using System;                    
using System.Net.Http;           
using System.Threading.Tasks;    
using Newtonsoft.Json.Linq;      
using System.Collections.Generic; 
using System.Linq;               


public struct Weather
{
    public string Country { get; set; }      // Страна
    public string Name { get; set; }         // Город или название местности
    public double Temp { get; set; }         // Температура воздуха
    public string Description { get; set; }  // Описание погоды
}

class Program
{
    //readonly может быть инициализированы только в момент их объявления или в конструкторе класса. После этого их значение нельзя изменить.
    private static readonly string apiKey = "5bfc799b03e9e6c939836dcf2904f262";

    // Поле для хранения URL API, с возможностью вставки координат и ключа
    private static readonly string apiUrl = "https://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&appid={2}&units=metric";

    // Асинхронный метод для получения данных о погоде по координатам
    static async Task<Weather> GetWeatherData(double lat, double lon)
    {
        // Создаем HTTP-клиент, чтобы отправить запрос к API
        using (HttpClient client = new HttpClient())
        {
            // Форматируем URL, подставляя широту, долготу и API-ключ
            string url = string.Format(apiUrl, lat, lon, apiKey);

            // response сервер отправляет клиенту в ответ на запрос, который клиент сделал ранее.Отправляем GET-запрос к API и ждем ответа в виде строки
            var response = await client.GetStringAsync(url);

            
            JObject weatherJson = JObject.Parse(response);

            // Извлекаем страну из раздела "sys" JSON-ответа
            var country = (string)weatherJson["sys"]?["country"];

            // Извлекаем название местности из раздела "name" JSON-ответа
            var name = (string)weatherJson["name"];


            // Извлекаем температуру из раздела "main" JSON-ответа
            var temp = (double)weatherJson["main"]["temp"];

            // Извлекаем описание погоды из первого элемента массива "weather" JSON-ответа
            var description = (string)weatherJson["weather"][0]["description"];

            // Возвращаем заполненную структуру Weather, если данные о стране или местности отсутствуют, подставляется "Unknown"
            return new Weather
            {
                Country = country ?? "Unknown",
                Name = name ?? "Unknown",
                Temp = temp,
                Description = description
            };
        }
    }

    // Асинхронный метод для получения коллекции данных о погоде
    static async Task<List<Weather>> GetWeatherCollectionAsync(int count)
    {
        // Создаем пустой список для хранения данных о погоде
        List<Weather> weatherList = new List<Weather>();

        // Создаем объект Random для генерации случайных координат
        Random rand = new Random();

        // Счетчик попыток получения данных
        int attempts = 0;

       
        while (weatherList.Count < count && attempts < count * 2)
        {
            // Генерируем случайную широту в диапазоне от -90 до 90
            double lat = rand.NextDouble() * 180 - 90;

            // Генерируем случайную долготу в диапазоне от -180 до 180
            double lon = rand.NextDouble() * 360 - 180;

            // Получаем данные о погоде для сгенерированных координат
            var weather = await GetWeatherData(lat, lon);

            // Проверяем, что страна и название местности определены
            if (weather.Country != "Unknown" && weather.Name != "Unknown")
            {
                // Добавляем данные о погоде в список
                weatherList.Add(weather);
            }

            
            attempts++;
        }

        return weatherList;
    }

    static async Task Main(string[] args)
    {
        // Получаем коллекцию данных о погоде, состоящую из 50 записей
        var weatherData = await GetWeatherCollectionAsync(50);

        // Передаем коллекцию данных для дальнейшей обработки
        ProcessWeatherData(weatherData);
    }

    // Метод для обработки данных о погоде с использованием LINQ
    static void ProcessWeatherData(List<Weather> weatherList)
    {
        // Находим запись с максимальной температурой, упорядочив список по убыванию температуры и взяв первый элемент
        var maxTemp = weatherList.OrderByDescending(w => w.Temp).FirstOrDefault();

        // Находим запись с минимальной температурой, упорядочив список по возрастанию температуры и взяв первый элемент
        var minTemp = weatherList.OrderBy(w => w.Temp).FirstOrDefault();

        // Выводим данные о местоположении с максимальной и минимальной температурой
        Console.WriteLine($"Страна с максимальной температурой: {maxTemp.Country}, {maxTemp.Name}, Температура: {maxTemp.Temp}°C");
        Console.WriteLine($"Страна с минимальной температурой: {minTemp.Country}, {minTemp.Name}, Температура: {minTemp.Temp}°C");

        // Вычисляем среднюю температуру для всех местоположений
        var avgTemp = weatherList.Average(w => w.Temp);

        
        Console.WriteLine($"Средняя температура в мире: {avgTemp}°C");

        // Подсчитываем количество уникальных стран в коллекции
        var countryCount = weatherList.Select(w => w.Country).Distinct().Count();

        
        Console.WriteLine($"Количество стран в коллекции: {countryCount}");

        // Находим первую запись с описанием погоды "clear sky", "rain" или "few clouds"
        var specificWeather = weatherList.FirstOrDefault(w =>
            w.Description == "clear sky" ||
            w.Description == "rain" ||
            w.Description == "few clouds");

        
        if (specificWeather.Country != null)
        {
            Console.WriteLine($"Первая страна с описанием погоды (clear sky, rain, few clouds): {specificWeather.Country}, {specificWeather.Name}, Описание: {specificWeather.Description}");
        }
        else
        {
            // Если таких записей нет, выводим соответствующее сообщение
            Console.WriteLine("Нет стран с описанием погоды (clear sky, rain, few clouds) в коллекции.");
        }
    }
}
