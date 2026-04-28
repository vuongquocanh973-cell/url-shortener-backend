using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Models;

namespace UrlShortener.Services;

public class UrlShortenerService
{
    private readonly AppDbContext _db;

    public UrlShortenerService(AppDbContext db)
    {
        _db = db;
    }

    private string GenerateShortCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public async Task<ShortUrl> CreateShortUrlAsync(string originalUrl)
    {
        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out _))
            throw new ArgumentException("Invalid URL format.");

        string code;
        do { code = GenerateShortCode(); }
        while (await _db.ShortUrls.AnyAsync(u => u.ShortCode == code));

        var shortUrl = new ShortUrl
        {
            OriginalUrl = originalUrl,
            ShortCode = code
        };

        _db.ShortUrls.Add(shortUrl);
        await _db.SaveChangesAsync();
        return shortUrl;
    }

    public async Task<ShortUrl?> GetByCodeAsync(string code)
    {
        var entry = await _db.ShortUrls.FirstOrDefaultAsync(u => u.ShortCode == code);
        if (entry != null)
        {
            entry.ClickCount++;
            await _db.SaveChangesAsync();
        }
        return entry;
    }

    public async Task<List<ShortUrl>> GetAllAsync()
    {
        return await _db.ShortUrls.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }
}