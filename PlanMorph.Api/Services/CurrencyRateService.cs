using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace PlanMorph.Api.Services;

public interface ICurrencyRateService
{
    Task<CurrencyRatesResult> GetRatesAsync(string? baseCurrency = null);
}

public sealed class CurrencyRateService : ICurrencyRateService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrencyRateService> _logger;

    public CurrencyRateService(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        ILogger<CurrencyRateService> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CurrencyRatesResult> GetRatesAsync(string? baseCurrency = null)
    {
        var defaultBase = _configuration["Currency:Base"] ?? "KES";
        var normalizedBase = string.IsNullOrWhiteSpace(baseCurrency)
            ? defaultBase.Trim().ToUpperInvariant()
            : baseCurrency.Trim().ToUpperInvariant();

        var supported = _configuration.GetSection("Currency:Supported").Get<string[]>() 
            ?? new[] { defaultBase, "USD", "EUR", "GBP" };

        var supportedList = supported
            .Select(code => code.Trim().ToUpperInvariant())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!supportedList.Contains(normalizedBase, StringComparer.OrdinalIgnoreCase))
        {
            supportedList.Insert(0, normalizedBase);
        }

        var cacheKey = $"currency:rates:{normalizedBase}";
        if (_cache.TryGetValue(cacheKey, out CurrencyRatesResult? cached) && cached != null)
        {
            return cached;
        }

        var cacheMinutes = int.TryParse(_configuration["Currency:CacheMinutes"], out var minutes)
            ? Math.Max(5, minutes)
            : 720;

        var providerUrl = _configuration["Currency:ProviderUrl"];
        if (string.IsNullOrWhiteSpace(providerUrl) || string.Equals(providerUrl.Trim(), "<set-in-env>", StringComparison.OrdinalIgnoreCase))
        {
            providerUrl = "https://api.exchangerate.host/latest?base={BASE}";
        }

        var fallbackRates = _configuration
            .GetSection("Currency:FallbackRates")
            .Get<Dictionary<string, decimal>>()
            ?? new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var rates = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            [normalizedBase] = 1m
        };

        var source = "fallback";
        try
        {
            var url = providerUrl.Replace("{BASE}", normalizedBase, StringComparison.OrdinalIgnoreCase);
            var client = _httpClientFactory.CreateClient("currency");
            using var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var payload = await response.Content.ReadAsStringAsync();
                var providerResponse = JsonSerializer.Deserialize<CurrencyProviderResponse>(payload,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (providerResponse?.Rates != null && providerResponse.Rates.Count > 0)
                {
                    foreach (var code in supportedList)
                    {
                        if (string.Equals(code, normalizedBase, StringComparison.OrdinalIgnoreCase))
                        {
                            rates[code] = 1m;
                            continue;
                        }

                        if (providerResponse.Rates.TryGetValue(code, out var rate))
                        {
                            rates[code] = rate;
                        }
                    }

                    source = "live";
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch live currency rates. Falling back to configured rates.");
        }

        if (source != "live")
        {
            foreach (var code in supportedList)
            {
                if (rates.ContainsKey(code))
                    continue;

                if (fallbackRates.TryGetValue(code, out var fallback))
                {
                    rates[code] = fallback;
                }
            }
        }

        var result = new CurrencyRatesResult
        {
            Base = normalizedBase,
            AsOf = DateTime.UtcNow,
            Source = source,
            Supported = supportedList,
            Rates = rates
        };

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(cacheMinutes));

        return result;
    }

    private sealed class CurrencyProviderResponse
    {
        public string? Base { get; set; }
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}

public sealed class CurrencyRatesResult
{
    public string Base { get; set; } = "KES";
    public DateTime AsOf { get; set; }
    public string Source { get; set; } = "fallback";
    public IReadOnlyCollection<string> Supported { get; set; } = Array.Empty<string>();
    public IReadOnlyDictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
}
