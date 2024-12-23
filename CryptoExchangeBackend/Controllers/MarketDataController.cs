using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

[ApiController]
[Route("api/[controller]")]
public class MarketDataController : ControllerBase
{
    public class Ticker
    {
        public string InstType { get; set; }
        public string InstId { get; set; }
        public string Last { get; set; }
        public string AskPx { get; set; }
        public string BidPx { get; set; }
        public string Open24h { get; set; }
        public string High24h { get; set; }
        public string Low24h { get; set; }
        public string Vol24h { get; set; }
        public string Ts { get; set; }
    }

    public class ApiResponse
    {
        public string Code { get; set; }
        public string Msg { get; set; }
        public List<Ticker> Data { get; set; }
    }

    [HttpGet("ws-market-data")]
    public async Task<IActionResult> GetMarketDataWebSocket()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleWebSocketConnection(webSocket);
            return Ok();
        }
        else
        {
            return BadRequest("WebSocket isteği değil.");
        }
    }

    private async Task HandleWebSocketConnection(WebSocket webSocket)
    {
        using var client = new HttpClient();
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
            
                var response = await client.GetAsync("https://www.okx.com/api/v5/market/tickers?instType=SPOT");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Data != null)
                    {
                        var first10Records = apiResponse.Data.Take(10).ToList();
                        var message = JsonSerializer.Serialize(first10Records);

                        var messageBytes = Encoding.UTF8.GetBytes(message);
                        await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }

                
                await Task.Delay(5000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                break;
            }
        }

        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bağlantı kapatıldı", CancellationToken.None);
    }
}
