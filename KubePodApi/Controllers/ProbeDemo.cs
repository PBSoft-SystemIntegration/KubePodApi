using KubePodApi.HealthCheck;
using Microsoft.AspNetCore.Mvc;

namespace KubePodApi.Controllers;

[ApiController]
[Route("demo")]
public class ProbeDemo : ControllerBase
{
    [HttpPost("readiness/{healthy:bool}")]
    public string SetReadiness(bool healthy)
    {
        ProbeState.Ready = healthy;
        return $"Readiness: {healthy}";
    }

    [HttpPost("liveness/{healthy:bool}")]
    public string SetLiveness(bool healthy)
    {
        ProbeState.Alive = healthy;
        return $"Liveness: {healthy}";
    }
}
