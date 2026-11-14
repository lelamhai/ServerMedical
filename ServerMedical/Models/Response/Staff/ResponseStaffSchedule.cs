namespace ServerMedical.Models.Response.Staff;

public sealed class ResponseStaffSchedule
{
    public int CampaignId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Location { get; init; } = string.Empty;

    public DateTime EventDate { get; init; }

    public string? ShiftTime { get; init; }
}
