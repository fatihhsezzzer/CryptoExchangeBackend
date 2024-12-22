using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class MarketDataController : ControllerBase
{
    [HttpGet("market-data")]
    public async Task<IActionResult> GetMarketData()
    {
        using var client = new HttpClient();
        var response = await client.GetAsync("https://www.okx.com/api/v5/market/tickers?instType=SPOT");
        var content = await response.Content.ReadAsStringAsync();

        return Ok(content);
    }
}
