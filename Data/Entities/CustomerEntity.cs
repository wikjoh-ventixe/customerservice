using Microsoft.AspNetCore.Identity;

namespace Data.Entities;

public class CustomerEntity : IdentityUser
{
    public DateTime Created { get; set; } = DateTime.Now;
}
