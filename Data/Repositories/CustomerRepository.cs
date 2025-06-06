using Data.Context;
using Data.Entities;

namespace Data.Repositories;

public class CustomerRepository(CustomerDbContext context) : BaseRepository<CustomerEntity>(context)
{
}
