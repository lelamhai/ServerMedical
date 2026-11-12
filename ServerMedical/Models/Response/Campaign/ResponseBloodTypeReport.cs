namespace ServerMedical.Models.Response.Campaign;

public sealed class ResponseBloodTypeReport
{
    public string BloodType { get; init; } = string.Empty;

    public int TotalBloodVolume { get; init; }
}
