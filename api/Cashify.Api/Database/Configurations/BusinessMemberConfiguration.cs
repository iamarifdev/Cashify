using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashify.Api.Database.Configurations;

public class BusinessMemberConfiguration : IEntityTypeConfiguration<BusinessMember>
{
    public void Configure(EntityTypeBuilder<BusinessMember> builder)
    {
        builder.HasIndex(x => new { x.BusinessId, x.UserId }).IsUnique();
        
        builder.Property(x => x.Role)
            .HasConversion<string>()
            .IsRequired();
    }
}
