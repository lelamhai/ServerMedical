namespace ServerMedical.Models.Response.Volunteer;

public sealed class ResponseDonationHistory
{
    public int ScreeningId { get; init; }

    public int VolunteerId { get; init; }

    public int CampaignId { get; init; }

    public string CampaignTitle { get; init; } = string.Empty;

    public string CampaignLocation { get; init; } = string.Empty;

    public DateTime CampaignEventDate { get; init; }

    public DateTime? DonationDate { get; init; }

    public string ScreeningResult { get; init; } = string.Empty;

    public string? RejectionReason { get; init; }

    public int? BloodVolume { get; init; }

    public string? BloodType { get; init; }

    public bool IsSuccessful { get; init; }
}
