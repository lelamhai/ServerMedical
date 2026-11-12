using System;
using System.Collections.Generic;

namespace ServerMedical.Entities;

public partial class Registration
{
    public int RegistrationId { get; set; }

    public int VolunteerId { get; set; }

    public int CampaignId { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public virtual Campaign Campaign { get; set; } = null!;

    public virtual User Volunteer { get; set; } = null!;
}
