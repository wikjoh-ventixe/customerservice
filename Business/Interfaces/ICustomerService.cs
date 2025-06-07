using Business.Dtos;
using Business.Models;

namespace Business.Interfaces;

public interface ICustomerService
{
    Task<CustomerResult<Customer?>> CreateCustomerAsync(CreateCustomerRequestDto request);
    Task<CustomerResult<Customer?>> CreateCustomerWithPasswordAsync(CreateCustomerRequestDto request, string password);
}
