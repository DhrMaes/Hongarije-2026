using System.ComponentModel.DataAnnotations;

namespace HongarijePlanner.Api.Models;

public class ShoppingItem
{
    [Key]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Author { get; set; } = string.Empty;

    public bool IsBought { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
