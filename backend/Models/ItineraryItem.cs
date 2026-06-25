using System.ComponentModel.DataAnnotations;

namespace HongarijePlanner.Api.Models;

public class ItineraryItem
{
    [Key]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Author { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public List<ItineraryVote> Votes { get; set; } = [];
}
