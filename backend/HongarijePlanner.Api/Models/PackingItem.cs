using System.ComponentModel.DataAnnotations;

namespace HongarijePlanner.Api.Models;

public class PackingItem
{
    [Key]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;

    public bool IsPacked { get; set; }

    public bool IsDefault { get; set; }
}
