using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashify.Api.Database.Configurations;

public class TransactionChangeConfiguration : IEntityTypeConfiguration<TransactionChange>
{
    public void Configure(EntityTypeBuilder<TransactionChange> builder)
    {
        builder.Property(x => x.ChangesJson).HasColumnType("jsonb");
        builder.Property(x => x.ChangeType).IsRequired();
    }
}
