using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashify.Api.Database.Configurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.Property(x => x.Name).IsRequired();
        builder.HasMany(x => x.Cashbooks).WithOne(x => x.Business).HasForeignKey(x => x.BusinessId);
    }
}
