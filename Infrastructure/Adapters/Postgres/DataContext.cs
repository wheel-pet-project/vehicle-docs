using Domain.OsagoAggregate;
using Domain.VehicleDocumentsAggregate;
using Infrastructure.Adapters.Postgres.Inbox;
using Infrastructure.Adapters.Postgres.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Adapters.Postgres;

public class DataContext : DbContext
{
    public DbSet<VehicleDocuments> VehicleDocuments { get; set; }
    public DbSet<Osago> Osagos { get; set; }
    public DbSet<InboxEvent> Inbox { get; set; }
    public DbSet<OutboxEvent> Outbox { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new VehicleDocumentsEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OsagoEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ExpiryStatusEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxEventTypeConfiguration());
        modelBuilder.ApplyConfiguration(new InboxEventTypeConfiguration());
    }
}

internal class VehicleDocumentsEntityTypeConfiguration : IEntityTypeConfiguration<VehicleDocuments>
{
    public void Configure(EntityTypeBuilder<VehicleDocuments> builder)
    {
        builder.ToTable("vehicle_documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever().HasColumnName("id").IsRequired();
        builder.Property(x => x.VehicleId).ValueGeneratedNever().HasColumnName("vehicle_id").IsRequired();

        builder.OwnsOne(x => x.Status, cfg =>
        {
            cfg.Property(x => x.IsPtsAdded).HasColumnName("is_pts_added").IsRequired();
            cfg.Property(x => x.IsStsAdded).HasColumnName("is_sts_added").IsRequired();
            cfg.Property(x => x.IsOsagoAdded).HasColumnName("is_osago_added").IsRequired();
        });

        builder.OwnsOne(x => x.Pts, cfg =>
        {
            cfg.Property(x => x.FrontPhotoStorageBucketAndKey)
                .HasColumnName("pts_front_photo_storage_bucket_and_key")
                .IsRequired();
            cfg.Property(x => x.BackPhotoStorageBucketAndKey)
                .HasColumnName("pts_back_photo_storage_bucket_and_key")
                .IsRequired();
            cfg.Property(x => x.YearOfManufacture).HasColumnName("pts_year_of_manufacture").IsRequired();
            cfg.OwnsOne(x => x.Color,
                colorCfg => { colorCfg.Property(x => x.Name).HasColumnName("pts_color").IsRequired(); });
            cfg.OwnsOne(x => x.Vin,
                vinCfg => { vinCfg.Property(x => x.Number).HasColumnName("pts_vin").IsRequired(); });
        });
        builder.Navigation(x => x.Pts).IsRequired(false);

        builder.OwnsOne(x => x.Sts, cfg =>
        {
            cfg.Property(x => x.FrontPhotoStorageBucketAndKey)
                .HasColumnName("sts_front_photo_storage_bucket_and_key")
                .IsRequired();
            cfg.Property(x => x.BackPhotoStorageBucketAndKey)
                .HasColumnName("sts_back_photo_storage_bucket_and_key")
                .IsRequired();
        });
        builder.Navigation(x => x.Sts).IsRequired(false);

        builder.Ignore(x => x.DomainEvents);
    }
}

internal class OsagoEntityTypeConfiguration : IEntityTypeConfiguration<Osago>
{
    public void Configure(EntityTypeBuilder<Osago> builder)
    {
        builder.ToTable("osago");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever().HasColumnName("id").IsRequired();

        builder.HasOne<VehicleDocuments>()
            .WithMany()
            .HasForeignKey(x => x.VehicleDocumentsId)
            .HasConstraintName("FK_vehicle_documents")
            .IsRequired();

        builder.HasOne(x => x.ExpiryStatus)
            .WithMany()
            .HasForeignKey("expiry_status_id")
            .HasConstraintName("FK_expiry_status_id")
            .IsRequired();

        builder.HasIndex(x => x.VehicleDocumentsId);

        builder.Property(x => x.VehicleDocumentsId).HasColumnName("vehicle_documents_id").IsRequired();
        builder.Property(x => x.PhotoStorageBucketAndKey).HasColumnName("photo_storage_bucket_and_key").IsRequired();
        builder.Property(x => x.DateOfIssue).HasColumnName("date_of_issue").IsRequired();
        builder.Property(x => x.DateOfExpiry).HasColumnName("date_of_expiry").IsRequired();

        builder.Ignore(x => x.DomainEvents);
    }
}

internal class ExpiryStatusEntityTypeConfiguration : IEntityTypeConfiguration<ExpiryStatus>
{
    public void Configure(EntityTypeBuilder<ExpiryStatus> builder)
    {
        builder.ToTable("expiry_status");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever().HasColumnName("id").IsRequired();
        builder.Property(x => x.Name).HasColumnName("name").IsRequired();

        builder.HasData(ExpiryStatus.All());
    }
}

internal class OutboxEventTypeConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.ToTable("outbox");

        builder.HasKey(x => x.EventId);

        builder.HasIndex(x => new { x.OccurredOnUtc, x.ProcessedOnUtc }, "IX_outbox_messages_unprocessed")
            .IncludeProperties(x => new { x.EventId, x.Type })
            .IsDescending(false, false)
            .HasFilter("processed_on_utc IS NULL");

        builder.Property(x => x.EventId).ValueGeneratedNever().HasColumnName("event_id").IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").IsRequired();
        builder.Property(x => x.OccurredOnUtc).HasColumnName("occurred_on_utc").IsRequired();
        builder.Property(x => x.ProcessedOnUtc).HasColumnName("processed_on_utc").IsRequired(false);
    }
}

internal class InboxEventTypeConfiguration : IEntityTypeConfiguration<InboxEvent>
{
    public void Configure(EntityTypeBuilder<InboxEvent> builder)
    {
        builder.ToTable("inbox");

        builder.HasKey(x => x.EventId);

        builder.HasIndex(x => new { x.OccurredOnUtc, x.ProcessedOnUtc }, "IX_inbox_messages_unprocessed")
            .IncludeProperties(x => new { x.EventId, x.Type })
            .IsDescending(false, false)
            .HasFilter("processed_on_utc IS NULL");

        builder.Property(x => x.EventId).ValueGeneratedNever().HasColumnName("event_id").IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").IsRequired();
        builder.Property(x => x.Content).HasColumnName("content").IsRequired();
        builder.Property(x => x.OccurredOnUtc).HasColumnName("occurred_on_utc").IsRequired();
        builder.Property(x => x.ProcessedOnUtc).HasColumnName("processed_on_utc").IsRequired(false);
    }
}