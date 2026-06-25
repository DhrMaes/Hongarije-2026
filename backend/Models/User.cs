using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace HongarijePlanner.Api.Models;

public class User
{
    [Key]
    public string Name { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }

    [JsonIgnore]
    public string? PinHash { get; set; }

    [NotMapped]
    public bool HasPin => !string.IsNullOrEmpty(PinHash);
}
