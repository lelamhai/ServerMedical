using System;
using System.Collections.Generic;

namespace ServerMedical.Entities;

public partial class RefreshToken
{
    public int TokenId { get; set; }

    public int? UserId { get; set; }

    public string Token { get; set; } = null!;

    public bool? IsRevoked { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
