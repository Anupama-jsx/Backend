using Backend.Models.Racing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public sealed class PassageConfiguration : IEntityTypeConfiguration<Passage>
{
    public void Configure(EntityTypeBuilder<Passage> entity)
    {
        entity.ToTable("Passages", table =>
            table.HasCheckConstraint("CK_Passages_Content", "LEN(LTRIM(RTRIM([Content]))) > 0"));
        entity.HasKey(passage => passage.Id);
        entity.HasData(new Passage
        {
            Id = Guid.Parse("118f5ad7-31de-4713-93df-c26a6d683012"),
            Content = "Small steps lead to steady progress. Take your time, find your rhythm, and focus on each word. Every race is another chance to learn and improve.",
            CreatedAtUtc = new DateTime(2026, 11, 17, 0, 0, 0, DateTimeKind.Utc)
        });
        entity.Property(passage => passage.Content).IsRequired();
        entity.Property(passage => passage.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
