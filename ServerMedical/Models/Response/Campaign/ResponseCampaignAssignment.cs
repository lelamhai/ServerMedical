namespace ServerMedical.Models.Response.Campaign;

public sealed class ResponseCampaignAssignment
{
    public int StaffId { get; init; }

    public string StaffName { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? ShiftTime { get; init; }
}
