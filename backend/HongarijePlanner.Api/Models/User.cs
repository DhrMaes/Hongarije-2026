using System.ComponentModel.DataAnnotations;

namespace HongarijePlanner.Api.Models;

public class User
{
    [Key]
    public string Name { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
}
