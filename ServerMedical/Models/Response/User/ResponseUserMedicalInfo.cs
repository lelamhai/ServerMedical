namespace ServerMedical.Models.Response.User;

public sealed class ResponseUserMedicalInfo
{
    public ResponseUserProfile User { get; init; } = null!;

    public string? BloodType { get; init; }

    public string? DiseaseNotes { get; init; }

    public DateTime? ResultDate { get; init; }
}
