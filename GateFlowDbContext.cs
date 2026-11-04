using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using gateflow.Models;

namespace gateflow.Data;

public partial class GateFlowDbContext : DbContext
{
    public GateFlowDbContext(DbContextOptions<GateFlowDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Approval> Approvals { get; set; }

    public virtual DbSet<GateLog> GateLogs { get; set; }

    public virtual DbSet<GatePass> GatePasses { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Approval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId).HasName("PK__Approval__328477F4850A53A5");

            entity.HasIndex(e => e.PassId, "UQ__Approval__C6740AA9E2E92134").IsUnique();

            entity.Property(e => e.ApprovalDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Decision)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Remarks)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.Admin).WithMany(p => p.Approvals)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Approvals_Users");

            entity.HasOne(d => d.Pass).WithOne(p => p.Approval)
                .HasForeignKey<Approval>(d => d.PassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Approvals_GatePasses");
        });

        modelBuilder.Entity<GateLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__GateLogs__5E548648076B1727");

            entity.HasIndex(e => e.PassId, "UQ__GateLogs__C6740AA9EC79B79D").IsUnique();

            entity.Property(e => e.ExitTime).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Guard).WithMany(p => p.GateLogs)
                .HasForeignKey(d => d.GuardId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GateLogs_Users");

            entity.HasOne(d => d.Pass).WithOne(p => p.GateLog)
                .HasForeignKey<GateLog>(d => d.PassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GateLogs_GatePasses");

        });

        modelBuilder.Entity<GatePass>(entity =>
        {
            entity.HasKey(e => e.PassId).HasName("PK__GatePass__C6740AA840DB22FC");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.User).WithMany(p => p.GatePasses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GatePasses_Users");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1242904971");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C155CEADE");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534BB7472AE").IsUnique();

            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
