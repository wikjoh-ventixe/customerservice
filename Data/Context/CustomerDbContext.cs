using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : IdentityDbContext<CustomerEntity>(options)
{

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Add prefix for customer-related identity tables
        builder.Entity<CustomerEntity>().ToTable("Customer_IdentityUsers");
        builder.Entity<IdentityRole>().ToTable("Customer_IdentityRoles");
        builder.Entity<IdentityUserRole<string>>().ToTable("Customer_IdentityUserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("Customer_IdentityUserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("Customer_IdentityUserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("Customer_IdentityUserTokens");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("Customer_IdentityRoleClaims");
    }
}
