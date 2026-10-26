using KubePodApi.HealthCheck;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.AddHealthChecks()
    .AddCheck<StartupHealthCheck>("startup")
    .AddCheck("readiness", () => ProbeState.Ready
        ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy())
    .AddCheck("liveness", () => ProbeState.Alive
        ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy());

var app = builder.Build();

app.MapHealthChecks("/healthz/startup", new HealthCheckOptions
{
    Predicate = check => check.Name == "startup"
});
app.MapHealthChecks("/healthz/readiness", new HealthCheckOptions
{
    Predicate = check => check.Name == "readiness"
});
app.MapHealthChecks("/healthz/liveness", new HealthCheckOptions
{
    Predicate = check => check.Name == "liveness"
});

app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

Console.WriteLine($"pod {Environment.GetEnvironmentVariable("POD_NAME")} " +
                  $"internal cluster IP: {Environment.GetEnvironmentVariable("POD_IP")}");
app.Run();
