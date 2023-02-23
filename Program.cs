using URLShortener.DTOs;
using URLShortener.Models;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var redis = ConnectionMultiplexer.Connect("localhost:6379");
var db = redis.GetDatabase(0);

app.MapPost("/shorturl", async (URLDTO url, HttpContext context) =>
{
    if (!Uri.TryCreate(url.URL, UriKind.Absolute, out var uri))
    {
        return Results.BadRequest();
    }

    const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
    var rng = new Random();

    var randomString = new String(Enumerable.Repeat(alphabet, 16).Select(s => s[rng.Next(s.Length)]).ToArray());

    var shortUrl = new URLEntity
    {
        Id = new Guid(),
        URL = url.URL,
        ShortURL = randomString
    };

    await db.StringSetAndGetAsync($"{shortUrl.ShortURL}", $"{url.URL}", TimeSpan.FromHours(24));

    var result = $"{context.Request.Scheme}://{context.Request.Host}/{shortUrl.ShortURL}";

    return Results.Ok(new ResponseDTO(result));

});

app.MapFallback(async (HttpContext context) =>
{
    var path = context.Request.Path.Value!.Trim().Remove(0, 1);

    var redisValue = await db.StringGetAsync(path);

    if (redisValue.HasValue)
    {
        return Results.Ok(new URLDTO($"{redisValue.ToString()}"));
    }

    return Results.BadRequest("There are no links with this address");
});


app.Run();
