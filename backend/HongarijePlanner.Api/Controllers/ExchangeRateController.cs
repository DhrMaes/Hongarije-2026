using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HongarijePlanner.Api.Controllers;

[ApiController]
[Route("api/exchange-rate")]
public class ExchangeRateController(IHttpClientFactory httpClientFactory) : ControllerBase
{
    private static (double Rate, DateTime FetchedAt) _cache = (410, DateTime.MinValue);

    [HttpGet]
    public async Task<ActionResult<ExchangeRateResponse>> GetRate()
    {
        // Cache for 1 hour to avoid hammering the external API
        if ((DateTime.UtcNow - _cache.FetchedAt).TotalHours < 1)
            return Ok(new ExchangeRateResponse(_cache.Rate, cached: true));

        try
        {
            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var json = await client.GetStringAsync("https://api.frankfurter.app/latest?from=EUR&to=HUF");
            using var doc = JsonDocument.Parse(json);
            var rate = doc.RootElement.GetProperty("rates").GetProperty("HUF").GetDouble();
            _cache = (rate, DateTime.UtcNow);
            return Ok(new ExchangeRateResponse(rate, cached: false));
        }
        catch
        {
            return Ok(new ExchangeRateResponse(_cache.Rate, cached: true));
        }
    }

    public record ExchangeRateResponse(double Rate, bool Cached);
}
