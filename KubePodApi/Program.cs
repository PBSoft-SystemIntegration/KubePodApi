using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

var app = builder.Build();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapControllers();

Console.WriteLine($"pod {Environment.GetEnvironmentVariable("POD_NAME")} " +
                  $"internal cluster IP: {Environment.GetEnvironmentVariable("POD_IP")}");
app.Run();
