using System;
using System.Collections.Generic;

public interface IObserver
{
    void Update(string message);
}

public class Temperature
{
    private float _value;
    private List<IObserver> _observers = new List<IObserver>();

    public float Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                NotifyObservers();
            }
        }
    }

    public void AddObserver(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void RemoveObserver(IObserver observer)
    {
        _observers.Remove(observer);
    }

    private void NotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.Update($"Temperatura zmieniona na: {_value}°C");
        }
    }
}

public class WeatherStation
{
    private List<Temperature> _temperatures = new List<Temperature>();

    public void AddTemperature(Temperature temperature)
    {
        _temperatures.Add(temperature);
    }

    public void RemoveTemperature(Temperature temperature)
    {
        _temperatures.Remove(temperature);
    }

    public void DisplayTemperatures()
    {
        Console.WriteLine("Aktualne temperatury w stacji pogodowej:");
        foreach (var temperature in _temperatures)
        {
            Console.WriteLine($"{temperature.Value}°C");
        }
    }
}
public class ConsoleObserver : IObserver
{
    public void Update(string message)
    {
        Console.WriteLine(message); 
    }
}

public class Program
{
    public static void Main()
    {
        WeatherStation weatherStation = new WeatherStation();

        var temp1 = new Temperature();
        var temp2 = new Temperature();

        var consoleObserver = new ConsoleObserver();

        temp1.AddObserver(consoleObserver);
        temp2.AddObserver(consoleObserver);

        weatherStation.AddTemperature(temp1);
        weatherStation.AddTemperature(temp2);

        weatherStation.DisplayTemperatures();

        Console.WriteLine("\nZmiana temperatury:");
        temp1.Value = 25.0f; 
        temp2.Value = 18.5f; 

        weatherStation.DisplayTemperatures();

        weatherStation.RemoveTemperature(temp1);
        Console.WriteLine("\nPo usunięciu jednej temperatury:");
        weatherStation.DisplayTemperatures();
    }
}
