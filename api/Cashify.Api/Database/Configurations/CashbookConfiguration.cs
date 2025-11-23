using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashify.Api.Database.Configurations;

public class CashbookConfiguration : IEntityTypeConfiguration<Cashbook>
{
    public void Configure(EntityTypeBuilder<Cashbook> builder)
    {
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(10).IsRequired();
    }
}
