using System.ComponentModel.DataAnnotations;

namespace HongarijePlanner.Api.Models;

public class WishlistItem
{
    [Key]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Link { get; set; }

    public string Author { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public List<WishlistVote> Votes { get; set; } = [];
}
