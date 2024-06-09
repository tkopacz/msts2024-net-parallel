using System.Text.Json.Serialization;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var sampleTodos = new Todo[] {
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);
todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.MapGet("/long", () =>
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

internal record LongRecord(string Title, string Desc, int num)
{
    public int TotalLen => Title.Length + Desc.Length + num;
}

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(LongRecord[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
