using System;
using System.Collections.Generic;

namespace ServerMedical.Entities;

public partial class BloodTestResult
{
    public int TestResultId { get; set; }

    public string BloodType { get; set; } = null!;

    public string? DiseaseNotes { get; set; }

    public int ScreeningId { get; set; }

    public int StaffId { get; set; }

    public DateTime? ResultDate { get; set; }

    public virtual HealthScreening Screening { get; set; } = null!;

    public virtual User Staff { get; set; } = null!;
}
