using System;
using System.Collections.Generic;

namespace ServerMedical.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? PasswordHash { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Qrcode { get; set; }

    public string? Avatar { get; set; }

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<BloodTestResult> BloodTestResults { get; set; } = new List<BloodTestResult>();

    public virtual ICollection<CampaignAssignment> CampaignAssignments { get; set; } = new List<CampaignAssignment>();

    public virtual ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();

    public virtual ICollection<HealthScreening> HealthScreeningStaffs { get; set; } = new List<HealthScreening>();

    public virtual ICollection<HealthScreening> HealthScreeningVolunteers { get; set; } = new List<HealthScreening>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    public virtual ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();
}
