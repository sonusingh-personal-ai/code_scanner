using CodeScanner.Application.Common.Interfaces;
using CodeScanner.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeScanner.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Model> Models => Set<Model>();
    public DbSet<OfficeMember> OfficeMembers => Set<OfficeMember>();
    public DbSet<Line> Lines => Set<Line>();
    public DbSet<PrinterModel> PrinterModels => Set<PrinterModel>();
    public DbSet<PrinterModelValue> PrinterModelValues => Set<PrinterModelValue>();
    public DbSet<Response> Responses => Set<Response>();
    public DbSet<ResponseSummary> ResponseSummaries => Set<ResponseSummary>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<SettingInfo> SettingInfos => Set<SettingInfo>();
    public DbSet<DailyReport> DailyReports => Set<DailyReport>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Model
        modelBuilder.Entity<Model>(entity =>
        {
            entity.ToTable("Model");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(255).IsRequired();
        });

        // OfficeMember (Column ID in uppercase in legacy DB)
        modelBuilder.Entity<OfficeMember>(entity =>
        {
            entity.ToTable("OfficeMember");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });

        // Line
        modelBuilder.Entity<Line>(entity =>
        {
            entity.ToTable("Line");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        // PrinterModel & PrinterModelValue
        modelBuilder.Entity<PrinterModel>(entity =>
        {
            entity.ToTable("PrinterModel");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
            entity.HasMany(e => e.ModelValues)
                  .WithOne(v => v.PrinterModel)
                  .HasForeignKey(v => v.ModelId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrinterModelValue>(entity =>
        {
            entity.ToTable("PrinterModelValue");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(255).IsRequired();
        });

        // Setting & SettingInfo
        modelBuilder.Entity<Setting>(entity =>
        {
            entity.ToTable("Setting");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.SettingInfos)
                  .WithOne(i => i.Setting)
                  .HasForeignKey(i => i.SettingId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SettingInfo>(entity =>
        {
            entity.ToTable("SettingInfo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Parameters).HasMaxLength(255).IsRequired();
        });

        // Response & ResponseSummary (Column Dispaly in legacy DB)
        modelBuilder.Entity<Response>(entity =>
        {
            entity.ToTable("Response");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Barcode).HasMaxLength(255).IsRequired();
            entity.HasMany(e => e.Summaries)
                  .WithOne(s => s.Response)
                  .HasForeignKey(s => s.ResponseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ResponseSummary>(entity =>
        {
            entity.ToTable("ResponseSummary");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Display).HasColumnName("Dispaly"); // Legacy typo mapped cleanly
        });

        // DailyReport
        modelBuilder.Entity<DailyReport>(entity =>
        {
            entity.ToTable("DailyReport");
            entity.HasKey(e => e.Id);
        });
    }
}
