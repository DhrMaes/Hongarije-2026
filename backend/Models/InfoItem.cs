using System.ComponentModel.DataAnnotations;

namespace HongarijePlanner.Api.Models;

public class InfoItem
{
    [Key]
    public string Id { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Body { get; set; }

    public string? Link { get; set; }

    public string? Special { get; set; }
}
