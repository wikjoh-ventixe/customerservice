using Business.Interfaces;
using Business.Services;
using Data.Context;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddDbContext<CustomerDbContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("CustomerSqlConnection")));
builder.Services.AddIdentity<CustomerEntity, IdentityRole>(x =>
{
    x.User.RequireUniqueEmail = true;
    x.Password.RequiredLength = 8;
})
    .AddEntityFrameworkStores<CustomerDbContext>()
    .AddDefaultTokenProviders();

var app = builder.Build();
app.MapOpenApi();
app.UseHttpsRedirection();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
