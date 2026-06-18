using System.ComponentModel.DataAnnotations;

namespace HongarijePlanner.Api.Models;

public class SharedPackingItem
{
    [Key]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Note { get; set; }

    public bool IsPacked { get; set; }

    public string? PackedBy { get; set; }

    public string AddedBy { get; set; } = string.Empty;
}
