using System;
using System.Collections.Generic;

namespace ServerMedical.Entities;

public partial class HealthScreening
{
    public int ScreeningId { get; set; }

    public string ScreeningResult { get; set; } = null!;

    public string? RejectionReason { get; set; }

    public int? BloodVolume { get; set; }

    public int VolunteerId { get; set; }

    public int CampaignId { get; set; }

    public int StaffId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual BloodTestResult? BloodTestResult { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual User Staff { get; set; } = null!;

    public virtual User Volunteer { get; set; } = null!;
}
