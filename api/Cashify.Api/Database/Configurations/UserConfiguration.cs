using Cashify.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cashify.Api.Database.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.GoogleUserId).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).IsRequired();
        builder.Property(u => u.Name).IsRequired();
    }
}
