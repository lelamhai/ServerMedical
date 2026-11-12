namespace ServerMedical.Models.Request.BloodTestResult;

public sealed class RequestUpdateBloodTestResult
{
    public string? DiseaseNotes { get; set; }

    public string BloodType { get; set; } = string.Empty;

    public int BloodVolume { get; set; }
}
