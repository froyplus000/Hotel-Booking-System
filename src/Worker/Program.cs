using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder();

// Basic Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Worker");
logger.LogInformation("Hotel Booking Worker Starting Up.....");

app.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        while (true)
        {
            logger.LogInformation("Worker hearbeat at: {time}", DateTimeOffset.Now);
            await Task.Delay(TimeSpan.FromSeconds(30));
        }
    });
});

await app.RunAsync();   