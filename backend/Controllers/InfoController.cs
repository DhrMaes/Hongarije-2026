using System.ComponentModel.DataAnnotations;
using HongarijePlanner.Api.Data;
using HongarijePlanner.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HongarijePlanner.Api.Controllers;

[ApiController]
[Route("api/info")]
public class InfoController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InfoItem>>> GetInfoItems()
    {
        var items = await dbContext.InfoItems
            .OrderBy(item => item.Category)
            .ThenBy(item => item.Title)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<InfoItem>> CreateInfoItem([FromBody] CreateInfoItemRequest request)
    {
        var item = new InfoItem
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Category = request.Category,
            Title = request.Title,
            Body = request.Body,
            Link = request.Link,
            Special = request.Special
        };

        dbContext.InfoItems.Add(item);
        await dbContext.SaveChangesAsync();

        return Created($"/api/info/{item.Id}", item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInfoItem(string id)
    {
        var item = await dbContext.InfoItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.InfoItems.Remove(item);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    public class CreateInfoItemRequest
    {
        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Body { get; set; }

        public string? Link { get; set; }

        public string? Special { get; set; }
    }
}
