using System.Text.Json.Serialization;

namespace UrlShortener.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<ShortUrl> ShortUrls { get; set; } = new List<ShortUrl>();
}