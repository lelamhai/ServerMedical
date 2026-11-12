using System;
using System.Collections.Generic;

namespace ServerMedical.Entities;

public partial class Campaign
{
    public int CampaignId { get; set; }

    public string Title { get; set; } = null!;

    public string Location { get; set; } = null!;

    public DateTime EventDate { get; set; }

    public int AdminId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User Admin { get; set; } = null!;

    public virtual ICollection<CampaignAssignment> CampaignAssignments { get; set; } = new List<CampaignAssignment>();

    public virtual ICollection<HealthScreening> HealthScreenings { get; set; } = new List<HealthScreening>();

    public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
