using Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Context;

public class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : IdentityDbContext<CustomerEntity>(options)
{
}
