namespace ServerMedical.Models.Response.Campaign;

public sealed class ResponseCampaignRegistration
{
    public int RegistrationId { get; init; }

    public int CampaignId { get; init; }

    public int VolunteerId { get; init; }

    public string VolunteerName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public DateTime? RegistrationDate { get; init; }
}
