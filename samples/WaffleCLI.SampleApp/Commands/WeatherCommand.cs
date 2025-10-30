using WaffleCLI.Abstractions.Commands;
using WaffleCLI.Core.Attributes;
using WaffleCLI.Core.Output;
using WaffleCLI.SampleApp.Models;

namespace WaffleCLI.SampleApp.Commands;

[Command("weather", "Get weather information")]
public class WeatherCommand : ICommand
{
    private readonly WeatherSettings _settings;
    private readonly IConsoleOutput _output;

    public WeatherCommand(WeatherSettings settings, IConsoleOutput output)
    {
        _settings = settings;
        _output = output;
    }

    public string Name => "weather";
    public string Description => "Get weather information";

    public async Task ExecuteAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var city = args.Length > 0 ? args[0] : _settings.DefaultCity;

        if (string.IsNullOrEmpty(_settings.ApiKey) || _settings.ApiKey == "your-api-key-here")
        {
            _output.WriteError("API key is not configured");
            _output.WriteInfo("Set it in appsettings.json: Weather:ApiKey");
            return;
        }

        try
        {
            await ShowWeatherAsync(city, cancellationToken);
        }
        catch (Exception ex)
        {
            _output.WriteError($"Error: {ex.Message}");
        }
    }

    private async Task ShowWeatherAsync(string city, CancellationToken cancellationToken)
    {
        _output.WriteInfo($"Fetching weather data for {city}...");

        // Simulate loading with progress
        for (int i = 0; i <= 100; i += 20)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            Console.Write($"\rLoading weather data... {i}%");
            await Task.Delay(300, cancellationToken);
        }

        Console.WriteLine(); // New line after progress

        // Mock weather data
        var random = new Random();
        var weatherData = new
        {
            Temperature = random.Next(15, 30),
            Conditions = new[] { "Sunny", "Cloudy", "Rainy", "Partly Cloudy" }[random.Next(4)],
            Humidity = random.Next(30, 90),
            WindSpeed = random.Next(0, 25),
            Pressure = 1010 + random.Next(-10, 10)
        };

        _output.WriteLine($"Weather in {city}:", ConsoleColor.Cyan);
        _output.WriteLine("==================", ConsoleColor.Cyan);
        _output.WriteLine($"Temperature: {weatherData.Temperature}°C");
        _output.WriteLine($"Conditions: {weatherData.Conditions}");
        _output.WriteLine($"Humidity: {weatherData.Humidity}%");
        _output.WriteLine($"Wind Speed: {weatherData.WindSpeed} km/h");
        _output.WriteLine($"Pressure: {weatherData.Pressure} hPa");
    }
}