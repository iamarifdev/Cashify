using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashify.Api.Database.Configurations;

public class CashbookMemberConfiguration : IEntityTypeConfiguration<CashbookMember>
{
    public void Configure(EntityTypeBuilder<CashbookMember> builder)
    {
        builder.HasIndex(x => new { x.CashbookId, x.UserId }).IsUnique();
        
        builder.Property(x => x.Role)
            .HasConversion<string>()
            .IsRequired();
    }
}
