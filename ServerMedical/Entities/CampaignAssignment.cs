using System;
using System.Collections.Generic;

namespace ServerMedical.Entities;

public partial class CampaignAssignment
{
    public int CampaignId { get; set; }

    public int StaffId { get; set; }

    public string? ShiftTime { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual User Staff { get; set; } = null!;
}
