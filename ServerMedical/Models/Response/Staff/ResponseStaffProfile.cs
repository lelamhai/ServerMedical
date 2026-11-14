namespace ServerMedical.Models.Response.Staff;

public sealed class ResponseStaffProfile
{
    public int StaffId { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    public string? Gender { get; init; }

    public string? Avatar { get; init; }

    public DateTime? CreatedAt { get; init; }
}
