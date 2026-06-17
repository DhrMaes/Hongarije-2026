using System.ComponentModel.DataAnnotations;
using HongarijePlanner.Api.Data;
using HongarijePlanner.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HongarijePlanner.Api.Controllers;

[ApiController]
[Route("api/itinerary")]
public class ItineraryController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItineraryCategoryGroup>>> GetItinerary()
    {
        var items = await dbContext.ItineraryItems
            .Include(item => item.Votes)
            .ToListAsync();

        var groupedItems = items
            .GroupBy(item => item.Category)
            .OrderBy(group => group.Key)
            .Select(group => new ItineraryCategoryGroup(
                group.Key,
                group.OrderByDescending(GetNetScore)
                    .ThenByDescending(item => item.CreatedAt)
                    .ToList()))
            .ToList();

        return Ok(groupedItems);
    }

    [HttpPost]
    public async Task<ActionResult<ItineraryItem>> CreateItineraryItem([FromBody] CreateItineraryItemRequest request)
    {
        var item = new ItineraryItem
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Title = request.Title,
            Category = request.Category,
            Description = request.Description,
            Author = request.Author,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.ItineraryItems.Add(item);
        await dbContext.SaveChangesAsync();

        return Created($"/api/itinerary/{item.Id}", item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItineraryItem(string id)
    {
        var item = await dbContext.ItineraryItems.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        dbContext.ItineraryItems.Remove(item);
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/vote")]
    public async Task<ActionResult<ItineraryItem>> VoteItineraryItem(string id, [FromBody] VoteRequest request)
    {
        if (!IsValidDirection(request.Direction))
        {
            return BadRequest("Direction must be 'up' or 'down'.");
        }

        var item = await dbContext.ItineraryItems
            .Include(itineraryItem => itineraryItem.Votes)
            .FirstOrDefaultAsync(itineraryItem => itineraryItem.Id == id);

        if (item is null)
        {
            return NotFound();
        }

        var existingVote = item.Votes.FirstOrDefault(vote => vote.UserName == request.User);
        if (existingVote is not null && existingVote.Direction == request.Direction)
        {
            item.Votes.Remove(existingVote);
            dbContext.ItineraryVotes.Remove(existingVote);
        }
        else if (existingVote is not null)
        {
            existingVote.Direction = request.Direction;
        }
        else
        {
            item.Votes.Add(new ItineraryVote
            {
                ItemId = item.Id,
                UserName = request.User,
                Direction = request.Direction
            });
        }

        await dbContext.SaveChangesAsync();

        return Ok(item);
    }

    private static int GetNetScore(ItineraryItem item) =>
        item.Votes.Count(vote => vote.Direction == "up") - item.Votes.Count(vote => vote.Direction == "down");

    private static bool IsValidDirection(string direction) =>
        direction is "up" or "down";

    public record ItineraryCategoryGroup(string Category, List<ItineraryItem> Items);

    public class CreateItineraryItemRequest
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;

        public string? Description { get; set; }

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
