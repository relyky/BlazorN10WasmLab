using BlazorN10WasmLab.Shared.Contracts;
using ProtoBuf.Grpc;

namespace BlazorN10WasmLab.Services;

public class WeatherService : IWeatherService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public ValueTask<WeatherForecastReply> GetForecastsAsync(CallContext context = default)
    {
        var forecasts = GenerateForecasts().ToList();
        return ValueTask.FromResult(new WeatherForecastReply { Forecasts = forecasts });
    }

    public async IAsyncEnumerable<WeatherForecast> StreamForecastsAsync(CallContext context = default)
    {
        foreach (var forecast in GenerateForecasts())
        {
            await Task.Delay(300, context.CancellationToken);
            yield return forecast;
        }
    }

    private static IEnumerable<WeatherForecast> GenerateForecasts()
    {
        var startDate = DateOnly.FromDateTime(DateTime.Now);
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        });
    }
}
