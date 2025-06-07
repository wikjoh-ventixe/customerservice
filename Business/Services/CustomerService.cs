using Business.Dtos;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Protos;
using System.Reflection;

namespace Business.Services;

public class CustomerService(UserManager<CustomerEntity> userManager, RoleManager<IdentityRole> roleManager, ICustomerRepository customerRepository, GrpcCustomerProfile.GrpcCustomerProfileClient grpcUserProfileClient) : ICustomerService
{
    private readonly UserManager<CustomerEntity> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly ICustomerRepository _customerRepository = customerRepository;
    private readonly GrpcCustomerProfile.GrpcCustomerProfileClient _grpcUserProfileClient = grpcUserProfileClient;


    // CREATE
    public async Task<CustomerResult<Customer?>> CreateCustomerAsync(CreateCustomerRequestDto request)
    {
        if (request == null)
            return CustomerResult<Customer?>.BadRequest("Request cannot be null.");

        if (await _userManager.Users.AnyAsync(u => u.Email == request.Email))
            return CustomerResult<Customer?>.AlreadyExists("Customer with given email adress already exists.");

        var customerEntity = new CustomerEntity
        {
            Email = request.Email,
            UserName = request.Email,
        };

        try
        {
            await _customerRepository.BeginTransactionAsync();

            var createCustomerResult = await _userManager.CreateAsync(customerEntity);
            if (!createCustomerResult.Succeeded)
            {
                await _customerRepository.RollbackTransactionAsync();
                return CustomerResult<Customer?>.InternalServerError($"Failed creating customer with email {customerEntity.Email}. Rolling back.");
            }

            var createdCustomerEntity = _userManager.Users.FirstOrDefault(x => x.Id == customerEntity.Id);
            if (createdCustomerEntity == null)
            {
                await _customerRepository.RollbackTransactionAsync();
                return CustomerResult<Customer?>.InternalServerError($"Failed retrieving customer entity after creation. Rolling back.");
            }

            // Add user profile via gRPC. If adding user profile fails then rollback
            var grpcRequest = new CreateCustomerProfileRequest
            {
                UserId = createdCustomerEntity.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                StreetAddress = request.StreetAddress,
                PostalCode = request.PostalCode,
                City = request.City,
            };

            var grpcResponse = await _grpcUserProfileClient.CreateCustomerProfileAsync(grpcRequest);
            if (!grpcResponse.Succeeded)
            {
                await _customerRepository.RollbackTransactionAsync();
                return CustomerResult<Customer?>.InternalServerError($"Failed creating customer profile. Rolling back.");
            }

            await _customerRepository.CommitTransactionAsync();
            var createdCustomer = new Customer
            {
                Id = createdCustomerEntity.Id,
                Email = createdCustomerEntity.Email!,
                Created = createdCustomerEntity.Created
            };
            return CustomerResult<Customer?>.Created(createdCustomer);
        }
        catch (Exception ex)
        {
            await _customerRepository.RollbackTransactionAsync();
            return CustomerResult<Customer?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }

    public async Task<CustomerResult<Customer?>> CreateCustomerWithPasswordAsync(CreateCustomerRequestDto request, string password)
    {
        if (request == null)
            return CustomerResult<Customer?>.BadRequest("Request cannot be null.");

        if (await _userManager.Users.AnyAsync(u => u.Email == request.Email))
            return CustomerResult<Customer?>.AlreadyExists("Customer with given email adress already exists.");

        var customerEntity = new CustomerEntity
        {
            Email = request.Email,
            UserName = request.Email,
        };

        try
        {
            await _customerRepository.BeginTransactionAsync();

            var createUserResult = await _userManager.CreateAsync(customerEntity, password);
            if (!createUserResult.Succeeded)
            {
                await _customerRepository.RollbackTransactionAsync();
                return CustomerResult<Customer?>.InternalServerError($"Failed creating customer with email {customerEntity.Email}. Rolling back.");
            }

            var createdCustomerEntity = _userManager.Users.FirstOrDefault(x => x.Id == customerEntity.Id);
            if (createdCustomerEntity == null)
            {
                await _customerRepository.RollbackTransactionAsync();
                return CustomerResult<Customer?>.InternalServerError($"Failed retrieving customer entity after creation. Rolling back.");
            }

            // Add user profile via gRPC. If adding user profile fails then rollback
            var grpcRequest = new CreateCustomerProfileRequest
            {
                UserId = createdCustomerEntity.Id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                StreetAddress = request.StreetAddress,
                PostalCode = request.PostalCode,
                City = request.City,
            };

            var grpcResponse = await _grpcUserProfileClient.CreateCustomerProfileAsync(grpcRequest);
            if (!grpcResponse.Succeeded)
            {
                await _customerRepository.RollbackTransactionAsync();
                return CustomerResult<Customer?>.InternalServerError($"Failed creating customer profile. Rolling back.");
            }

            await _customerRepository.CommitTransactionAsync();
            var createdCustomer = new Customer
            {
                Id = createdCustomerEntity.Id,
                Email = createdCustomerEntity.Email!,
                Created = createdCustomerEntity.Created
            };
            return CustomerResult<Customer?>.Created(createdCustomer);
        }
        catch (Exception ex)
        {
            await _customerRepository.RollbackTransactionAsync();
            return CustomerResult<Customer?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }
}
