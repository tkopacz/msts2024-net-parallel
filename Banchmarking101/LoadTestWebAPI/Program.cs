var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

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
    return forecast;
});


app.MapGet("/long", () =>
{
    var forecast = Enumerable.Range(1, 500).Select(index =>
        new LongRecord
        (
            new string('t', index *2 + Random.Shared.Next(255)),
            new string('d', index * 3 + Random.Shared.Next(255)),
            1000 + Random.Shared.Next(2000)
        ))
        .ToArray();
    return forecast;
});

app.MapGet("/longasync", async () =>
{
    return await Task.Run(() =>
    {
        var forecast = Enumerable.Range(1, 500).Select(index =>
        new LongRecord
        (
            new string('t', index * 2 + Random.Shared.Next(255)),
            new string('d', index * 3 + Random.Shared.Next(255)),
            1000 + Random.Shared.Next(2000)
        ))
        .ToArray();
        return forecast;
    }
    );
});
app.Run();


internal record LongRecord (string Title,string Desc,int num)
{
    public int TotalLen =>  Title.Length + Desc.Length + num; 
}

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
