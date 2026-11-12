namespace ServerMedical.Models.Response.RegisterCampaign;

public sealed class ResponseRegisterCampaign
{
    public int RegistrationId { get; init; }

    public int VolunteerId { get; init; }

    public int CampaignId { get; init; }

    public DateTime? RegistrationDate { get; init; }

    public DateTime EventDate { get; init; }
}
