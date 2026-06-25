using System.Text.Json.Serialization;

namespace HongarijePlanner.Api.Models;

public class ItineraryVote
{
    public string ItemId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    [JsonIgnore]
    public ItineraryItem? Item { get; set; }
}
