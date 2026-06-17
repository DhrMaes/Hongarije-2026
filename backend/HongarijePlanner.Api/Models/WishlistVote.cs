namespace HongarijePlanner.Api.Models;

public class WishlistVote
{
    public string ItemId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public WishlistItem? Item { get; set; }
}
