using Business.Dtos;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Protos;
using System.Net;
using System.Reflection;

namespace Business.Services;

public class CustomerService(UserManager<CustomerEntity> userManager, RoleManager<IdentityRole> roleManager, ICustomerRepository customerRepository, GrpcCustomerProfile.GrpcCustomerProfileClient grpcUserProfileClient, SignInManager<CustomerEntity> signInManager) : ICustomerService
{
    private readonly UserManager<CustomerEntity> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly SignInManager<CustomerEntity> _signInManager = signInManager;
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

    public async Task<CustomerResult<Customer?>> CreateCustomerWithPasswordWithoutProfileAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return CustomerResult<Customer?>.BadRequest("Parameters cannot be null.");

        if (await _userManager.Users.AnyAsync(u => u.Email == email))
            return CustomerResult<Customer?>.AlreadyExists("Customer with given email adress already exists.");

        var customerEntity = new CustomerEntity
        {
            Email = email,
            UserName = email,
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

            await _customerRepository.CommitTransactionAsync();
            var createdCustomer = new Customer
            {
                Id = createdCustomerEntity.Id,
                Email = createdCustomerEntity.Email!,
                Created = createdCustomerEntity.Created
            };

            // Generate email confirmation token and include in response
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(createdCustomerEntity);
            var encodedToken = WebUtility.UrlEncode(token);
            createdCustomer.ConfirmEmailToken = encodedToken;

            return CustomerResult<Customer?>.Created(createdCustomer);
        }
        catch (Exception ex)
        {
            await _customerRepository.RollbackTransactionAsync();
            return CustomerResult<Customer?>.InternalServerError($"Exception occurred in {MethodBase.GetCurrentMethod()!.Name}.");
        }
    }

    public async Task<CustomerResult<bool?>> ValidateEmailToken(string email, string emailToken)
    {
        var userEntity = await _userManager.FindByEmailAsync(email);
        if (userEntity == null)
            return CustomerResult<bool?>.NotFound($"User with email {email} not found.");

        var result = await _userManager.ConfirmEmailAsync(userEntity, emailToken);
        if (!result.Succeeded)
            return CustomerResult<bool?>.Unauthorized("Token failed validation.");

        return CustomerResult<bool?>.Ok(true);
    }

    // READ
    public async Task<CustomerResult<AuthData>> LoginCustomerAsync(CustomerLoginRequestDto request)
    {
        if (request == null)
            return CustomerResult<AuthData>.BadRequest("Request cannot be null.");

        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            return CustomerResult<AuthData>.Ok(new AuthData
            {
                UserId = user!.Id,
                EmailConfirmed = user.EmailConfirmed,
            });
        }

        return CustomerResult<AuthData>.Unauthorized("Customer authentication failed.");
    }
}
