using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrlShortener.Services;

namespace UrlShortener.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UrlController : ControllerBase
{
    private readonly UrlShortenerService _service;

    public UrlController(UrlShortenerService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUrl([FromBody] string originalUrl)
    {
        try
        {
            // Get userId if logged in
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;

            var result = await _service.CreateShortUrlAsync(originalUrl, userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;

        var urls = await _service.GetAllAsync(userId);
        return Ok(urls);
    }
}

[ApiController]
public class RedirectController : ControllerBase
{
    private readonly UrlShortenerService _service;

    public RedirectController(UrlShortenerService service)
    {
        _service = service;
    }

    [HttpGet("/{code}")]
    public async Task<IActionResult> RedirectToUrl(string code)
    {
        var entry = await _service.GetByCodeAsync(code);
        if (entry == null) return NotFound("Short URL not found.");
        return Redirect(entry.OriginalUrl);
    }
}