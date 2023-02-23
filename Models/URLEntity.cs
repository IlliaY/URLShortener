namespace URLShortener.Models;


public class URLEntity
{
    public Guid Id { get; set; }
    public string URL { get; set; } = String.Empty;
    public string ShortURL { get; set; } = String.Empty;
}