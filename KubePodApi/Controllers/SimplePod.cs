using Microsoft.AspNetCore.Mvc;

namespace KubePodApi.Controllers;

[ApiController]
public class SimplePod(IHttpClientFactory httpClientFactory) : ControllerBase
{
    [HttpGet("Info")]
    public string GetInternalInfo()
    {
        var name = Environment.GetEnvironmentVariable("POD_NAME");
        var ip = Environment.GetEnvironmentVariable("POD_IP");
        return $"pod {name} internal cluster IP: {ip} Version: 0.1.0";
    }

    [HttpGet("GetFromOtherService/{serviceUrlAndPort}")]
    public async Task<string> GetFromOtherService(string serviceUrlAndPort)
    {
        var client = httpClientFactory.CreateClient();
        return await client.GetStringAsync($"http://{serviceUrlAndPort}/Info");
    }
}
