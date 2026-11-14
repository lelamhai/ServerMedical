namespace ServerMedical.Models.Response.Campaign;

public sealed class ResponseCampaignPage
{
    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalItems { get; init; }

    public int TotalPages { get; init; }

    public List<ResponseCampaign> Items { get; init; } = new();
}
