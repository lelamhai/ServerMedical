using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ServerMedical.Entities;

public partial class MedicalContext : DbContext
{
    public MedicalContext()
    {
    }

    public MedicalContext(DbContextOptions<MedicalContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BloodTestResult> BloodTestResults { get; set; }

    public virtual DbSet<Campaign> Campaigns { get; set; }

    public virtual DbSet<CampaignAssignment> CampaignAssignments { get; set; }

    public virtual DbSet<HealthScreening> HealthScreenings { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDevice> UserDevices { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BloodTestResult>(entity =>
        {
            entity.HasKey(e => e.TestResultId).HasName("PK__BloodTes__E2463A67E2605A24");

            entity.HasIndex(e => e.ScreeningId, "UQ__BloodTes__7734E46D54F0284D").IsUnique();

            entity.Property(e => e.TestResultId).HasColumnName("TestResultID");
            entity.Property(e => e.BloodType)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.DiseaseNotes).HasMaxLength(255);
            entity.Property(e => e.ResultDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ScreeningId).HasColumnName("ScreeningID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");

            entity.HasOne(d => d.Screening).WithOne(p => p.BloodTestResult)
                .HasForeignKey<BloodTestResult>(d => d.ScreeningId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BloodTest__Scree__5DCAEF64");

            entity.HasOne(d => d.Staff).WithMany(p => p.BloodTestResults)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BloodTest__Staff__5EBF139D");
        });

        modelBuilder.Entity<Campaign>(entity =>
        {
            entity.HasKey(e => e.CampaignId).HasName("PK__Campaign__3F5E8D7969F55F39");

            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EventDate).HasColumnType("datetime");
            entity.Property(e => e.Location).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Admin).WithMany(p => p.Campaigns)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Campaigns__Admin__49C3F6B7");
        });

        modelBuilder.Entity<CampaignAssignment>(entity =>
        {
            entity.HasKey(e => new { e.CampaignId, e.StaffId }).HasName("PK__Campaign__5633C7D67A2A621F");

            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.ShiftTime).HasMaxLength(100);

            entity.HasOne(d => d.Campaign).WithMany(p => p.CampaignAssignments)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CampaignA__Campa__4CA06362");

            entity.HasOne(d => d.Staff).WithMany(p => p.CampaignAssignments)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CampaignA__Staff__4D94879B");
        });

        modelBuilder.Entity<HealthScreening>(entity =>
        {
            entity.HasKey(e => e.ScreeningId).HasName("PK__HealthSc__7734E46C1AD02A4E");

            entity.Property(e => e.ScreeningId).HasColumnName("ScreeningID");
            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RejectionReason).HasMaxLength(255);
            entity.Property(e => e.ScreeningResult).HasMaxLength(50);
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.VolunteerId).HasColumnName("VolunteerID");

            entity.HasOne(d => d.Campaign).WithMany(p => p.HealthScreenings)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HealthScr__Campa__5812160E");

            entity.HasOne(d => d.Staff).WithMany(p => p.HealthScreeningStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HealthScr__Staff__59063A47");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.HealthScreeningVolunteers)
                .HasForeignKey(d => d.VolunteerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HealthScr__Volun__571DF1D5");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__RefreshT__658FEE8AD379C345");

            entity.HasIndex(e => e.Token, "UQ__RefreshT__1EB4F817B85DF61E").IsUnique();

            entity.Property(e => e.TokenId).HasColumnName("TokenID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.IsRevoked).HasDefaultValue(false);
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RefreshTokens_Users");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasKey(e => e.RegistrationId).HasName("PK__Registra__6EF588300519407E");

            entity.HasIndex(e => new { e.VolunteerId, e.CampaignId }, "UQ_Volunteer_Campaign").IsUnique();

            entity.Property(e => e.RegistrationId).HasColumnName("RegistrationID");
            entity.Property(e => e.CampaignId).HasColumnName("CampaignID");
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.VolunteerId).HasColumnName("VolunteerID");

            entity.HasOne(d => d.Campaign).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Campa__534D60F1");

            entity.HasOne(d => d.Volunteer).WithMany(p => p.Registrations)
                .HasForeignKey(d => d.VolunteerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Registrat__Volun__52593CB8");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACE89EAF2E");

            entity.HasIndex(e => e.Phone, "UQ__Users__5C7E359E7D40F567").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105345801C93D").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Avatar)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Qrcode)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("QRCode");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Volunteer");
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("PK__UserDevi__49E123313E5D7204");

            entity.HasIndex(e => e.Fcmtoken, "UQ__UserDevi__BBACE64E2C20309A").IsUnique();

            entity.Property(e => e.DeviceId).HasColumnName("DeviceID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeviceName).HasMaxLength(100);
            entity.Property(e => e.Fcmtoken)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("FCMToken");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.UserDevices)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserDevices_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
