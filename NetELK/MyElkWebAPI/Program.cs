using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;
// ...existing code...

internal class Program
{
    private static void Main(string[] args)
    {
        var logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", Assembly.GetExecutingAssembly().GetName().Name)
    .WriteTo.File("logs/myelkwebapi-logs-.log", rollingInterval: RollingInterval.Day) // Add this line
    // .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("https://localhost:9200"))
    // {
    //     AutoRegisterTemplate = true,
    //     //IndexFormat = "myelkwebapi-logs-{0:yyyy.MM.dd}"
    //     IndexFormat = "myelkwebapi-logs"
    // })
    .CreateLogger();

        Log.Logger = logger;


        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog(Log.Logger);
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();


        var app = builder.Build();


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        Log.Information("Application started and logging to Elasticsearch!");


        var summaries = new[]
        {
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

        app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
             var json = System.Text.Json.JsonSerializer.Serialize(forecast);
            Log.Information($"Weather forecast generated: {json}");
            return forecast;
        })
        .WithName("GetWeatherForecast");

        app.Run();
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
