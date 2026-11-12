namespace ServerMedical.Models.Response.HealthScreening;

public sealed class ResponseHealthScreening
{
    public int ScreeningId { get; init; }

    public int VolunteerId { get; init; }

    public string VolunteerName { get; init; } = string.Empty;

    public int CampaignId { get; init; }

    public string CampaignTitle { get; init; } = string.Empty;

    public string ScreeningResult { get; init; } = string.Empty;

    public string? RejectionReason { get; init; }

    public int? BloodVolume { get; init; }

    public int StaffId { get; init; }

    public DateTime? CreatedAt { get; init; }
}
