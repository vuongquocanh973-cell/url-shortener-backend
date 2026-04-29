using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Services;

namespace UrlShortener.Tests;

public class UrlShortenerServiceTests
{
    private AppDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateShortUrl_ValidUrl_ReturnsShortCode()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new UrlShortenerService(db);

        // Act
        var result = await service.CreateShortUrlAsync("https://www.google.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://www.google.com", result.OriginalUrl);
        Assert.Equal(6, result.ShortCode.Length);
    }

    [Fact]
    public async Task CreateShortUrl_InvalidUrl_ThrowsException()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new UrlShortenerService(db);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateShortUrlAsync("not-a-valid-url"));
    }

    [Fact]
    public async Task GetByCode_ExistingCode_ReturnsUrl()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new UrlShortenerService(db);
        var created = await service.CreateShortUrlAsync("https://www.youtube.com");

        // Act
        var result = await service.GetByCodeAsync(created.ShortCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("https://www.youtube.com", result.OriginalUrl);
        Assert.Equal(1, result.ClickCount);
    }

    [Fact]
    public async Task GetByCode_NonExistingCode_ReturnsNull()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new UrlShortenerService(db);

        // Act
        var result = await service.GetByCodeAsync("XXXXXX");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAll_ReturnsAllUrls()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new UrlShortenerService(db);
        await service.CreateShortUrlAsync("https://www.google.com");
        await service.CreateShortUrlAsync("https://www.youtube.com");

        // Act
        var result = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }
}