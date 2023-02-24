namespace URLShortener.Services.RandomStringService;

public class RandomStringService : IRandomStringService
{
    public string GenerateRandomString()
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
        var seed = Math.Abs((int)DateTime.Now.Ticks);
        var rng = new Random(seed);

        var randomString = new String(Enumerable.Repeat(alphabet, 16).Select(s => s[rng.Next(s.Length)]).ToArray());
        return randomString;
    }
}