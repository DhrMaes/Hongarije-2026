using System.ComponentModel.DataAnnotations;

namespace HongarijePlanner.Api.Requests;

public class VoteRequest
{
    [Required]
    public string User { get; set; } = string.Empty;

    [Required]
    public string Direction { get; set; } = string.Empty;
}
