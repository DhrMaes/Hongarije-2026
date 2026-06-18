using System.ComponentModel.DataAnnotations;
using HongarijePlanner.Api.Data;
using HongarijePlanner.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HongarijePlanner.Api.Controllers;

[ApiController]
[Route("api/packing")]
public class PackingController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PackingItem>>> GetPackingItems([FromQuery] string? user)
    {
        if (string.IsNullOrWhiteSpace(user))
        {
            return BadRequest("The 'user' query parameter is required.");
        }

        var items = await dbContext.PackingItems
            .Where(item => item.Owner == user)
            .OrderBy(item => item.Category)
            .ThenBy(item => item.Title)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<PackingItem>> CreatePackingItem([FromBody] CreatePackingItemRequest request)
    {
        var item = new PackingItem
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Title = request.Title,
            Category = request.Category,
            Owner = request.Owner,
            IsDefault = request.IsDefault ?? false,
            IsPacked = false
        };

        dbContext.PackingItems.Add(item);
        await dbContext.SaveChangesAsync();

        return Created($"/api/packing/{item.Id}", item);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<ActionResult<PackingItem>> TogglePackingItem(string id)
    {
        var item = await dbContext.PackingItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsPacked = !item.IsPacked;
        await dbContext.SaveChangesAsync();

        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePackingItem(string id)
    {
        var item = await dbContext.PackingItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.PackingItems.Remove(item);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("shared")]
    public async Task<ActionResult<IEnumerable<SharedPackingItem>>> GetSharedPackingItems()
    {
        var items = await dbContext.SharedPackingItems
            .OrderBy(item => item.Title)
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("shared")]
    public async Task<ActionResult<SharedPackingItem>> CreateSharedPackingItem([FromBody] CreateSharedPackingItemRequest request)
    {
        var item = new SharedPackingItem
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Title = request.Title,
            Note = request.Note,
            AddedBy = request.AddedBy,
            IsPacked = false,
            PackedBy = null
        };

        dbContext.SharedPackingItems.Add(item);
        await dbContext.SaveChangesAsync();

        return Created($"/api/packing/shared/{item.Id}", item);
    }

    [HttpPatch("shared/{id}/toggle")]
    public async Task<ActionResult<SharedPackingItem>> ToggleSharedPackingItem(string id, [FromBody] ToggleSharedPackingItemRequest request)
    {
        var item = await dbContext.SharedPackingItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        item.IsPacked = !item.IsPacked;
        item.PackedBy = item.IsPacked ? request.PackedBy : null;
        await dbContext.SaveChangesAsync();

        return Ok(item);
    }

    [HttpDelete("shared/{id}")]
    public async Task<IActionResult> DeleteSharedPackingItem(string id)
    {
        var item = await dbContext.SharedPackingItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.SharedPackingItems.Remove(item);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    public class CreatePackingItemRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string Owner { get; set; } = string.Empty;

        public bool? IsDefault { get; set; }
    }

    public class CreateSharedPackingItemRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Note { get; set; }

        [Required]
        public string AddedBy { get; set; } = string.Empty;
    }

    public class ToggleSharedPackingItemRequest
    {
        [Required]
        public string PackedBy { get; set; } = string.Empty;
    }
}
