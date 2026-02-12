using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlanMorph.Api.Services;

namespace PlanMorph.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController : ControllerBase
{
    private readonly ICurrencyRateService _currencyRateService;

    public CurrencyController(ICurrencyRateService currencyRateService)
    {
        _currencyRateService = currencyRateService;
    }

    [AllowAnonymous]
    [HttpGet("rates")]
    public async Task<IActionResult> GetRates([FromQuery] string? baseCurrency = null)
    {
        var result = await _currencyRateService.GetRatesAsync(baseCurrency);
        return Ok(result);
    }
}
