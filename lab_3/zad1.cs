using System;
using System.Collections.Generic;

// Aplikacja wykorzystująca 3 wzorce w celu wyświetlania danych pogodowych. Wykorzystane wzorce to Observer, Adapter i Singleton

// Observer
public interface IObserver
{
    void Update(float temperature);
}

public class WeatherStation
{
    private readonly List<IObserver> observers = new List<IObserver>();
    private float _temperature;

    public void AddObserver(IObserver observer)
    {
        observers.Add(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        observers.Remove(observer);
    }

    public void SetTemperature(float temperature)
    {
        _temperature = temperature;
        NotifyObservers();
    }

    private void NotifyObservers()
    {
        foreach (var observer in observers)
        {
            observer.Update(_temperature);
        }
    }
}

// Singleton 
public class Logger
{
    private static Logger _instance;

    private Logger() { }

    public static Logger GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Logger();
        }
        return _instance;
    }

    public void Log(string message)
    {
        Console.WriteLine($"[LOG]: {message}");
    }
}

// Adapter 

public class CelsiusDisplay : IObserver
{
    public void Update(float temperature)
    {
        Logger.GetInstance().Log($"Aktualna temperatura: {temperature}°C");
    }
}


public class FahrenheitDisplayAdapter : IObserver
{
    private readonly CelsiusDisplay _celsiusDisplay;

    public FahrenheitDisplayAdapter(CelsiusDisplay celsiusDisplay)
    {
        _celsiusDisplay = celsiusDisplay;
    }

    public void Update(float temperature)
    {
        float fahrenheit = (temperature * 9 / 5) + 32;
        Logger.GetInstance().Log($"Wyświetlanie w farenchaitach: {fahrenheit}°F z {temperature}°C.");
    }
}

public class Program
{
    public static void Main()
    {
        WeatherStation weatherStation = new WeatherStation();

        var celsiusDisplay = new CelsiusDisplay();
        var fahrenheitDisplayAdapter = new FahrenheitDisplayAdapter(celsiusDisplay);

        weatherStation.AddObserver(celsiusDisplay);
        weatherStation.AddObserver(fahrenheitDisplayAdapter);


        Console.WriteLine("Pogoda: ");

        weatherStation.SetTemperature(25.0f);
        weatherStation.SetTemperature(30.0f);

    }
}
