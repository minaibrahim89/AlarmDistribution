using AlarmDistribution.WebApi.Domain.Aggregates.Nurses;
using AlarmDistribution.WebApi.Domain.Aggregates.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlarmDistribution.WebApi.Infrastructure.Database.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.PrimaryNurseId)
            .IsRequired();

        builder.Property(p => p.SecondaryNurseId)
            .IsRequired();

        builder.HasOne<Nurse>()
            .WithMany()
            .HasForeignKey(p => p.PrimaryNurseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Nurse>()
            .WithMany()
            .HasForeignKey(p => p.SecondaryNurseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
