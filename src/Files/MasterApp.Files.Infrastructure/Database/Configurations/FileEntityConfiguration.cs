
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MasterApp.Files.Infrastructure.Database.Configurations;

internal sealed class FileEntityConfiguration : IEntityTypeConfiguration<File>
{
    public void Configure(EntityTypeBuilder<File> builder)
    {
        builder.ToTable(Tables.Files);

        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileName)
            .IsRequired()
            .HasDefaultValue("");
    }
}


