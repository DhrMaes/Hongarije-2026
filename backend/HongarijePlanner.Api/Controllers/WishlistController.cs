using System.ComponentModel.DataAnnotations;
using HongarijePlanner.Api.Data;
using HongarijePlanner.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HongarijePlanner.Api.Controllers;

[ApiController]
[Route("api/wishlist")]
public class WishlistController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WishlistItem>>> GetWishlist()
    {
        var items = await dbContext.WishlistItems
            .Include(item => item.Votes)
            .ToListAsync();

        var orderedItems = items
            .OrderByDescending(GetNetScore)
            .ThenByDescending(item => item.CreatedAt)
            .ToList();

        return Ok(orderedItems);
    }

    [HttpPost]
    public async Task<ActionResult<WishlistItem>> CreateWishlistItem([FromBody] CreateWishlistItemRequest request)
    {
        var item = new WishlistItem
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Title = request.Title,
            Description = request.Description,
            Link = request.Link,
            Author = request.Author,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.WishlistItems.Add(item);
        await dbContext.SaveChangesAsync();

        return Created($"/api/wishlist/{item.Id}", item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWishlistItem(string id)
    {
        var item = await dbContext.WishlistItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.WishlistItems.Remove(item);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/vote")]
    public async Task<ActionResult<WishlistItem>> VoteWishlistItem(string id, [FromBody] VoteRequest request)
    {
        if (!IsValidDirection(request.Direction))
        {
            return BadRequest("Direction must be 'up' or 'down'.");
        }

        var item = await dbContext.WishlistItems
            .Include(wishlistItem => wishlistItem.Votes)
            .FirstOrDefaultAsync(wishlistItem => wishlistItem.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        var existingVote = item.Votes.FirstOrDefault(vote => vote.UserName == request.User);
        if (existingVote is not null && existingVote.Direction == request.Direction)
        {
            item.Votes.Remove(existingVote);
            dbContext.WishlistVotes.Remove(existingVote);
        }
        else if (existingVote is not null)
        {
            existingVote.Direction = request.Direction;
        }
        else
        {
            item.Votes.Add(new WishlistVote
            {
                ItemId = item.Id,
                UserName = request.User,
                Direction = request.Direction
            });
        }

        await dbContext.SaveChangesAsync();

        return Ok(item);
    }

    private static int GetNetScore(WishlistItem item) =>
        item.Votes.Count(vote => vote.Direction == "up") - item.Votes.Count(vote => vote.Direction == "down");

    private static bool IsValidDirection(string direction) =>
        direction is "up" or "down";

    public class CreateWishlistItemRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Link { get; set; }

        [Required]
        public string Author { get; set; } = string.Empty;
    }

    public class VoteRequest
    {
        [Required]
        public string User { get; set; } = string.Empty;

        [Required]
        public string Direction { get; set; } = string.Empty;
    }
}
