namespace ServerMedical.Models.Response.Campaign;

public sealed class ResponseRegisteredCampaign
{
    public int CampaignId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Location { get; init; } = string.Empty;

    public DateTime EventDate { get; init; }

    public int AdminId { get; init; }

    public DateTime? CreatedAt { get; init; }
}
