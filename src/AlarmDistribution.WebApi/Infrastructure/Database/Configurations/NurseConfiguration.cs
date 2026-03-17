using System.Text.Json;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlarmDistribution.WebApi.Infrastructure.Database.Configurations;

public class NurseConfiguration : IEntityTypeConfiguration<Nurse>
{
    public void Configure(EntityTypeBuilder<Nurse> builder)
    {
        builder.ToTable("Nurses");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        builder.Property(n => n.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Ignore(n => n.PendingAlarms);

        var pendingAlarmsComparer = new ValueComparer<List<Guid>>(
            (left, right) => left!.SequenceEqual(right!),
            value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
            value => value.ToList());

        builder.Property<List<Guid>>("_pendingAlarms")
            .HasColumnName("PendingAlarms")
            .HasConversion(
                value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
                value => JsonSerializer.Deserialize<List<Guid>>(value, (JsonSerializerOptions?)null) ?? new List<Guid>())
            .Metadata.SetValueComparer(pendingAlarmsComparer);
    }
}
