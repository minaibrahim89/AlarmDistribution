using AlarmDistribution.WebApi.Domain.Aggregates.Alarms;
using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlarmDistribution.WebApi.Infrastructure.Database.Configurations;

public class AlarmConfiguration : IEntityTypeConfiguration<Alarm>
{
    public void Configure(EntityTypeBuilder<Alarm> builder)
    {
        builder.ToTable("Alarms");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.PatientId)
            .IsRequired();

        builder.Property(a => a.Type)
            .IsRequired();

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.AcknowledgingNurseId);

        builder.Property(a => a.AcknowledgedAt);

        builder.HasOne<Patient>()
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Nurse>()
            .WithMany()
            .HasForeignKey(a => a.AcknowledgingNurseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
