using Business.Dtos;
using Business.Models;

namespace Business.Interfaces;

public interface ICustomerService
{
    Task<CustomerResult<Customer?>> CreateCustomerAsync(CreateCustomerRequestDto request);
    Task<CustomerResult<Customer?>> CreateCustomerWithPasswordAsync(CreateCustomerRequestDto request, string password);
    Task<CustomerResult<Customer?>> CreateCustomerWithPasswordWithoutProfileAsync(string email, string password);
    Task<CustomerResult<AuthData>> LoginCustomerAsync(CustomerLoginRequestDto request);
    Task<CustomerResult<bool?>> ValidateEmailToken(string userId, string emailToken);
}
