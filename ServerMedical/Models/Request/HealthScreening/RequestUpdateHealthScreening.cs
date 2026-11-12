namespace ServerMedical.Models.Request.HealthScreening;

public sealed class RequestUpdateHealthScreening
{
    public string ScreeningResult { get; set; } = string.Empty;

    public string? RejectionReason { get; set; }
}
