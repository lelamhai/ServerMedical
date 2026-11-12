namespace ServerMedical.Models.Response.BloodTestResult;

public sealed class ResponseBloodTestResult
{
    public int TestResultId { get; init; }

    public int ScreeningId { get; init; }

    public int VolunteerId { get; init; }

    public string VolunteerName { get; init; } = string.Empty;

    public int CampaignId { get; init; }

    public string CampaignTitle { get; init; } = string.Empty;

    public string? DiseaseNotes { get; init; }

    public string BloodType { get; init; } = string.Empty;

    public int BloodVolume { get; init; }

    public DateTime? ResultDate { get; init; }

    public int StaffId { get; init; }
}
