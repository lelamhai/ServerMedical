namespace ServerMedical.Models.Request.HealthScreening;

public sealed class RequestCreateHealthScreening
{
    public int VolunteerId { get; set; }

    public int CampaignId { get; set; }

    public string ScreeningResult { get; set; } = string.Empty;

    public string? RejectionReason { get; set; }
}
