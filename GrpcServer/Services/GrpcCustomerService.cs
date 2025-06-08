using Business.Dtos;
using Business.Interfaces;
using Grpc.Core;
using Protos;
using System.Diagnostics;

namespace GrpcServer.Services;

public class GrpcCustomerAuthService(ICustomerService customerService) : GrpcCustomerAuth.GrpcCustomerAuthBase
{
    private readonly ICustomerService _customerService = customerService;


    public override async Task<CustomerRegistrationResponse> RegisterCustomer(CustomerRegistrationRequest request, ServerCallContext context)
    {
        Debug.WriteLine($"Attempting to register customer with email {request.Email}");

        var result = await _customerService.CreateCustomerWithPasswordWithoutProfileAsync(request.Email, request.Password);

        var response = new CustomerRegistrationResponse
        {
            Succeeded = result.Succeeded,
            StatusCode = result.StatusCode,
            ErrorMessage = result.ErrorMessage ?? string.Empty,
        };
        if (result.Data != null)
            response.AuthInfo = new AuthInfo
            {
                UserId = result.Data.Id
            };

        return response;
    }


    public override async Task<CustomerLoginResponse> LoginCustomer(CustomerLoginRequest request, ServerCallContext context)
    {
        Debug.WriteLine($"Attempting to login customer");

        var result = await _customerService.LoginCustomerAsync(new CustomerLoginRequestDto
        {
            Email = request.Email,
            Password = request.Password,
        });

        var response = new CustomerLoginResponse
        {
            Succeeded = result.Succeeded,
            StatusCode = result.StatusCode,
            ErrorMessage = result.ErrorMessage ?? string.Empty,
        };
        if (result.Data != null)
        {
            response.AuthInfo = new AuthInfo
            {
                UserId = result.Data.UserId,
                EmailConfirmed = result.Data.EmailConfirmed
            };
        }

        return response;
    }
}
