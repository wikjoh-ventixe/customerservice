using Data.Context;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories;

public class CustomerRepository(CustomerDbContext context) : BaseRepository<CustomerEntity>(context), ICustomerRepository
{
}
