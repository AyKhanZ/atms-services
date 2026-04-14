using ATMS.Admin.Data.Entities.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATMS.Admin.Data.Configurations.Tokens;

public class RefreshRevokedTokenConfiguration : IEntityTypeConfiguration<RefreshRevokedToken>
{
    public void Configure(EntityTypeBuilder<RefreshRevokedToken> builder)
    {
        builder.HasIndex(e => e.UserId);
        
        builder.HasIndex(e => e.Token).IsUnique();
    }
}
