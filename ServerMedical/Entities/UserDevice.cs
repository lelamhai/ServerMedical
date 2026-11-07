using System;
using System.Collections.Generic;

namespace ServerMedical.Entities;

public partial class UserDevice
{
    public int DeviceId { get; set; }

    public int UserId { get; set; }

    public string Fcmtoken { get; set; } = null!;

    public string? DeviceName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
