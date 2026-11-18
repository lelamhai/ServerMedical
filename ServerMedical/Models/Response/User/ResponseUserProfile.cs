namespace ServerMedical.Models.Response.User;

public sealed class ResponseUserProfile
{
    public int UserId { get; init; }

    public string FirstName { get; init; } = string.Empty;

    public string LastName { get; init; } = string.Empty;

    public string FullName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Phone { get; init; }

    public DateOnly? DateOfBirth { get; init; }

    public string? Gender { get; init; }

    public string? Qrcode { get; init; }

    public string? Avatar { get; init; }

    public string Role { get; init; } = string.Empty;

    public DateTime? CreatedAt { get; init; }
}
