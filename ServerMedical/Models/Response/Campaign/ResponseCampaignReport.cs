namespace ServerMedical.Models.Response.Campaign;

public sealed class ResponseCampaignReport
{
    public int CampaignId { get; init; }

    public string Title { get; init; } = string.Empty;

    public DateTime EventDate { get; init; }

    public int TotalRegisteredVolunteers { get; init; }

    public int TotalSuccessfulDonations { get; init; }

    public int TotalUnsuccessfulDonations { get; init; }

    public int TotalBloodVolume { get; init; }

    public List<ResponseBloodTypeReport> BloodVolumeByType { get; init; } = new();
}
