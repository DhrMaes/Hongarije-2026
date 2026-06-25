using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;

namespace HongarijePlanner.Api.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    private static readonly CultureInfo NlCulture = new("nl-BE");

    private static readonly WeatherResponse DefaultWeather =
        new(
            24,
            55,
            3.6,
            "Partly cloudy",
            "05:18",
            "20:44",
            5.3,
            new List<ForecastDay>
            {
                new("maandag 29/07", 19, 27, "Partly cloudy"),
                new("dinsdag 30/07", 18, 26, "Clear sky"),
                new("woensdag 31/07", 17, 25, "Rain showers")
            },
            new List<HourlyForecast>
            {
                new("09:00", 23, 4.8, "Partly cloudy"),
                new("10:00", 24, 5.1, "Partly cloudy"),
                new("11:00", 25, 5.4, "Clear sky")
            },
            true,
            DateTime.MinValue);

    private static WeatherResponse _cache = DefaultWeather;

    [HttpGet]
    public async Task<ActionResult<WeatherResponse>> GetWeather()
    {
        // Cache for 30 minutes to keep the external calls low.
        if ((DateTime.UtcNow - _cache.FetchedAt).TotalMinutes < 30)
            return Ok(_cache with { Cached = true });

        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            var url =
                "https://api.open-meteo.com/v1/forecast?latitude=46.7654&longitude=17.8243&current=temperature_2m,relative_humidity_2m,wind_speed_10m,weather_code,uv_index&hourly=temperature_2m,weather_code,uv_index&daily=weather_code,temperature_2m_max,temperature_2m_min,sunrise,sunset&forecast_days=5&timezone=auto";

            var json = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var current = doc.RootElement.GetProperty("current");
            var hourly = doc.RootElement.GetProperty("hourly");
            var daily = doc.RootElement.GetProperty("daily");

            var times = daily.GetProperty("time");
            var maxTemps = daily.GetProperty("temperature_2m_max");
            var minTemps = daily.GetProperty("temperature_2m_min");
            var weatherCodes = daily.GetProperty("weather_code");
            var sunriseTimes = daily.GetProperty("sunrise");
            var sunsetTimes = daily.GetProperty("sunset");

            var forecast = new List<ForecastDay>();
            for (var i = 0; i < times.GetArrayLength(); i++)
            {
                var dayDate = DateOnly.Parse(times[i].GetString() ?? string.Empty);
                var label = dayDate.ToDateTime(TimeOnly.MinValue)
                    .ToString("dddd dd/MM", NlCulture)
                    .ToLowerInvariant();

                forecast.Add(new ForecastDay(
                    label,
                    minTemps[i].GetDouble(),
                    maxTemps[i].GetDouble(),
                    MapWeatherCode(weatherCodes[i].GetInt32())));
            }

            var hourlyTimes = hourly.GetProperty("time");
            var hourlyTemps = hourly.GetProperty("temperature_2m");
            var hourlyCodes = hourly.GetProperty("weather_code");
            var hourlyUv = hourly.GetProperty("uv_index");
            var now = DateTime.Now;
            var hourlyForecast = new List<HourlyForecast>();
            for (var i = 0; i < hourlyTimes.GetArrayLength(); i++)
            {
                var hourTime = DateTime.Parse(
                    hourlyTimes[i].GetString() ?? string.Empty,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal);

                if (hourTime < now) continue;

                hourlyForecast.Add(new HourlyForecast(
                    hourTime.ToString("HH:mm"),
                    hourlyTemps[i].GetDouble(),
                    hourlyUv[i].GetDouble(),
                    MapWeatherCode(hourlyCodes[i].GetInt32())));

                if (hourlyForecast.Count >= 12) break;
            }

            var weatherCode = current.GetProperty("weather_code").GetInt32();
            var sunrise = FormatClockTime(sunriseTimes[0].GetString());
            var sunset = FormatClockTime(sunsetTimes[0].GetString());
            var response = new WeatherResponse(
                current.GetProperty("temperature_2m").GetDouble(),
                current.GetProperty("relative_humidity_2m").GetDouble(),
                current.GetProperty("wind_speed_10m").GetDouble(),
                MapWeatherCode(weatherCode),
                sunrise,
                sunset,
                current.GetProperty("uv_index").GetDouble(),
                forecast,
                hourlyForecast,
                false,
                DateTime.UtcNow);

            _cache = response;
            return Ok(response);
        }
        catch
        {
            return Ok(_cache with { Cached = true });
        }
    }

    private static string MapWeatherCode(int code)
    {
        return code switch
        {
            0 => "Clear sky",
            1 or 2 => "Partly cloudy",
            3 => "Overcast",
            45 or 48 => "Fog",
            51 or 53 or 55 => "Drizzle",
            56 or 57 => "Freezing drizzle",
            61 or 63 or 65 => "Rain",
            66 or 67 => "Freezing rain",
            71 or 73 or 75 or 77 => "Snow",
            80 or 81 or 82 => "Rain showers",
            85 or 86 => "Snow showers",
            95 => "Thunderstorm",
            96 or 99 => "Thunderstorm with hail",
            _ => "Unknown"
        };
    }

    public record WeatherResponse(
        double Temperature,
        double Humidity,
        double WindSpeed,
        string Condition,
        string Sunrise,
        string Sunset,
        double UvIndex,
        List<ForecastDay> Forecast,
        List<HourlyForecast> Hourly,
        bool Cached,
        DateTime FetchedAt);

    public record ForecastDay(
        string Day,
        double MinTemp,
        double MaxTemp,
        string Condition);

    public record HourlyForecast(
        string Time,
        double Temperature,
        double UvIndex,
        string Condition);

    private static string FormatClockTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "--:--";
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
            return parsed.ToString("HH:mm");
        return value;
    }
}