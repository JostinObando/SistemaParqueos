using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SistemaParqueos.Models.Entidades;

namespace SistemaParqueos.Data;

public partial class ParqueosContext : DbContext
{
    public ParqueosContext(DbContextOptions<ParqueosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PRQAutomoviles> PRQAutomoviles { get; set; }

    public virtual DbSet<PRQIngresos> PRQIngresos { get; set; }

    public virtual DbSet<PRQParqueos> PRQParqueos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("user_08", "PRQTipoAutomovil", new[] { "sedan", "4x4", "moto" })
            .HasPostgresEnum("user_08", "match_kind", new[] { "competition", "friendly" })
            .HasPostgresEnum("user_08", "match_status", new[] { "scheduled", "live", "finished" })
            .HasPostgresEnum("user_08", "sport_type", new[] { "soccer", "basketball", "baseball", "volleyball", "rugby", "hockey" })
            .HasPostgresEnum("user_25", "match_kind", new[] { "friendly", "official" })
            .HasPostgresEnum("user_25", "match_status", new[] { "scheduled", "live", "finished" })
            .HasPostgresEnum("user_25", "sport", new[] { "soccer", "basketball", "baseball", "volleyball", "rugby", "hockey" });

        modelBuilder.Entity<PRQAutomoviles>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRQAutomoviles_pkey");

            entity.ToTable("PRQAutomoviles", "user_09");

            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Fabricante).HasMaxLength(100);
            entity.Property(e => e.Tipo).HasMaxLength(20);
        });

        modelBuilder.Entity<PRQIngresos>(entity =>
        {
            entity.HasKey(e => e.Consecutivo).HasName("PRQIngresos_pkey");

            entity.ToTable("PRQIngresos", "user_09");

            entity.Property(e => e.FechaHoraEntrada).HasColumnType("timestamp without time zone");
            entity.Property(e => e.FechaHoraSalida).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.IdAutomovilNavigation).WithMany(p => p.PRQIngresos)
                .HasForeignKey(d => d.IdAutomovil)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PRQIngresos_IdAutomovil_fkey");

            entity.HasOne(d => d.IdParqueoNavigation).WithMany(p => p.PRQIngresos)
                .HasForeignKey(d => d.IdParqueo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("PRQIngresos_IdParqueo_fkey");
        });

        modelBuilder.Entity<PRQParqueos>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRQParqueos_pkey");

            entity.ToTable("PRQParqueos", "user_09");

            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.PrecioPorHora).HasPrecision(10, 2);
            entity.Property(e => e.Provincia).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
