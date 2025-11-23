using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashify.Api.Database.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasIndex(x => new { x.BusinessId, x.CashbookId, x.TransactionDate });
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Amount).IsRequired();
    }
}
