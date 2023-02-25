using URLShortener.DTOs;
using StackExchange.Redis;
using URLShortener.Services.RandomStringService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<IRandomStringService, RandomStringService>();

var app = builder.Build();
var redis = ConnectionMultiplexer.Connect("localhost:6379");
var db = redis.GetDatabase(0);

app.MapPost("/shorturl", async (URLDTO url, HttpContext context, IRandomStringService randomStringService) =>
{
    if (!Uri.TryCreate(url.URL, UriKind.Absolute, out var uri))
    {
        return Results.BadRequest();
    }

    var shortUrl = randomStringService.GenerateRandomString();

    await db.StringSetAndGetAsync($"{shortUrl}", $"{url.URL}", TimeSpan.FromHours(24));

    var result = $"{context.Request.Scheme}://{context.Request.Host}/{shortUrl}";

    return Results.Ok(new ResponseDTO(result));

});

app.MapFallback(async (HttpContext context) =>
{
    var path = context.Request.Path.Value!.Trim().Remove(0, 1);

    var redisValue = await db.StringGetAsync(path);

    if (redisValue.HasValue)
    {
        return Results.Redirect($"{redisValue.ToString()}");
    }

    return Results.BadRequest("There are no links with this address");
});


app.Run();
