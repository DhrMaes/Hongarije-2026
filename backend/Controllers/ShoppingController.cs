using System.ComponentModel.DataAnnotations;
using HongarijePlanner.Api.Data;
using HongarijePlanner.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HongarijePlanner.Api.Controllers;

[ApiController]
[Route("api/shopping")]
public class ShoppingController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShoppingItem>>> GetShoppingItems()
    {
        var items = await dbContext.ShoppingItems
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<ShoppingItem>> CreateShoppingItem([FromBody] CreateShoppingItemRequest request)
    {
        var item = new ShoppingItem
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Title = request.Title,
            Description = request.Description,
            Author = request.Author,
            IsBought = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ShoppingItems.Add(item);
        await dbContext.SaveChangesAsync();

        return Created($"/api/shopping/{item.Id}", item);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<ShoppingItem>> ToggleShoppingItem(string id)
    {
        var item = await dbContext.ShoppingItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsBought = !item.IsBought;
        await dbContext.SaveChangesAsync();

        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteShoppingItem(string id)
    {
        var item = await dbContext.ShoppingItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.ShoppingItems.Remove(item);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    public class CreateShoppingItemRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Author { get; set; } = string.Empty;
    }
}
